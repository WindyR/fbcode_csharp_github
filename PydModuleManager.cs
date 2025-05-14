using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using OpenCvSharp;
using Python.Runtime;

namespace WinFormsApp1;

public class PydModuleManager
{
    private static dynamic _pyModule = null; // 静态变量存储 Python 模块
    private static bool _isInitialized = false; // 标记是否已初始化
    private static readonly object Lock = new(); // 静态锁对象
    private static readonly object _initializationLock = new(); // 用于初始化的锁
    
    // 静态构造函数：在类第一次被访问时自动执行
    static PydModuleManager()
    {
        InitializePythonEngine();
    }
    private static void InitializePythonEngine()
    {
        // 确保初始化过程是线程安全的
        if (_isInitialized)
            return;
        string pythonDllPath = @"D:/01_software/46_Anaconda/env/vision2025/python39.dll";
        Python.Runtime.Runtime.PythonDLL = pythonDllPath;
        string sitePackagesDir = @"D:/01_software/46_Anaconda/env/vision2025/Lib/site-packages";
        string libDir = @"D:/01_software/46_Anaconda/env/vision2025/Lib";
        string dllsDir = @"D:/01_software/46_Anaconda/env/vision2025/DLLs";
        PythonEngine.PythonPath =
            $"{sitePackagesDir};" +
            $"{dllsDir};" +
            $"{libDir};" +
            $"{PythonEngine.PythonPath}";
        try
        {
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                // 加载 Python 模块
                _pyModule = Py.Import("process_3D_From_Byte");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error initializing Python engine: {e.Message}");
            throw;
        }

        _isInitialized = true;
    }
    
    public static dynamic GetPyModule()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("Python engine is not initialized.");
        }
        return _pyModule;
    }
    public static void ShutdownPythonEngine()
    {
        if (_isInitialized)
        {
            PythonEngine.Shutdown();
            _isInitialized = false;
        }
    }
    
    
    public ItemForModel PydProcessData(ItemForModel itemForModel)
    {
        DateTime time1 = DateTime.Now;
        try
        {
            var imgName = itemForModel.strDetectFileName;
            var rangeBuffer = itemForModel.rangeBuffer;
            var intensityBuffer = itemForModel.intensityBuffer;
            var rows = itemForModel.rows;
            var cols = itemForModel.cols;
            var xScale = itemForModel.xScale;
            var xOffset = itemForModel.xOffset;
            var yScale = itemForModel.yScale;
            var yOffset = itemForModel.yOffset;
            var zScale = itemForModel.zScale;
            var zOffset = itemForModel.zOffset;
            var deltaSaveDir = @"D:/02_Data/2025/fbcode_c#/testC#/WinFormsApp1/WinFormsApp1/out";
            var gpuNum = 0;
            PyObject pyObj = null;

            // 加锁，确保线程安全
            var gil = Py.GIL(); // 获取 GIL 锁
            try
            {
                // 获取已经加载的 Python 模块
                dynamic pyModule = PydModuleManager.GetPyModule();
                // 调用 Python 函数
                pyObj = pyModule.Process_3D_From_Byte(
                    gpuNum, deltaSaveDir, imgName, rangeBuffer, intensityBuffer,
                    rows, cols, xScale, xOffset, yScale, yOffset, zScale, zOffset
                );
                // Console.WriteLine("All process time: " + time4.Subtract(time3));
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                gil.Dispose();
            }
            

            DateTime time4 = DateTime.Now;
            if (pyObj == null || pyObj.GetAttr("size").As<int>() == 0)
            {
                itemForModel.strReturn = false;
                itemForModel.nDetectNum = 0;
            }
            else
            {
                itemForModel.strReturn = true;
                // 获取矩阵形状和数据类型
                dynamic shape = pyObj.GetAttr("shape").As<dynamic[]>();
                int npRows = shape[0]; // 矩阵的行数
                int npCols = shape[1]; // 矩阵的列数（已知为 7）
                string dtype = pyObj.GetAttr("dtype").ToString();
                // Console.WriteLine($"Matrix shape: {npRows} x {npCols}");
                // Console.WriteLine($"Matrix data type: {dtype}");
                // 将 NumPy 矩阵转换为 C# 的二维数组
                if (dtype.Contains("float"))
                {
                    var globalMinX = xOffset;
                    var globalMinY = yOffset;
                    var globalMaxX = xOffset + cols * xScale;
                    var globalMaxY = yOffset + rows * yScale;
                    List<TDDetect> tDDetects = new List<TDDetect>();
                    float[,] dataArray = new float[npRows, npCols];
                    itemForModel.nDetectNum = npRows;
                    for (int i = 0; i < npRows; i++)
                    {
                        TDDetect tdDetect = new TDDetect();
                        tdDetect.X0 = (pyObj[i][3].As<float>() - globalMinX) / (globalMaxX - globalMinX) * cols;
                        tdDetect.Y0 = (pyObj[i][4].As<float>() - globalMinY) / (globalMaxY - globalMinY) * rows;
                        tdDetect.X1 = (pyObj[i][1].As<float>() - globalMinX) / (globalMaxX - globalMinX) * cols;
                        tdDetect.Y1 = (pyObj[i][2].As<float>() - globalMinY) / (globalMaxY - globalMinY) * rows;
                        if (pyObj[i][6].As<float>() - 1.0 <= 1e-6)
                        {
                            tdDetect.DefectName = "scratch";
                        }
                        else if (pyObj[i][6].As<float>() - 2.0 <= 1e-6)
                        {
                            tdDetect.DefectName = "crack";
                        }
                        else
                        {
                            tdDetect.DefectName = "other";
                        }
                        tDDetects.Add(tdDetect);
                    }
                    itemForModel.defects = tDDetects;
                }
                else
                {
                    itemForModel.strReturn = false;
                    itemForModel.errMsg = "Prasing pyObj error, unsupported data type.";
                }
            }
        }
        catch (Exception ex)
        {
            itemForModel.strReturn = false;
            itemForModel.errMsg = ex.Message;
            Console.WriteLine($"Error: {ex.Message}");
        }

        DateTime endTime = DateTime.Now;
        Console.WriteLine("All process time is :" + endTime.Subtract(time1));
        return itemForModel;
    }
}