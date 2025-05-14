using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using OpenCvSharp;
using Python.Runtime;

namespace WinFormsApp1;

public class PydModuleMangerAsync
{
    private static dynamic _pyModule = null; // 静态变量存储 Python 模块
    private static bool _isInitialized = false; // 标记是否已初始化

    // 静态构造函数：在类第一次被访问时自动执行
    static PydModuleMangerAsync()
    {
        InitializePythonEngine();
    }

    private static void InitializePythonEngine()
    {
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


    public ItemForModel PydProcessDataAsync(ItemForModel itemForModel)
    {
        // 创建一个 float 数组, 27位数组的意义分别是，0位: 有无缺陷；1位：缺陷数量； 2-6：1缺陷位置+分类；7-11：2缺陷位置+分类 ......
        var defectArr = new float[27];
        // Array.Clear(defectArray, 0, defectArray.Length);
        // 获取数组的指针并分配非托管内存
        IntPtr arrayPtr = Marshal.AllocHGlobal(defectArr.Length * sizeof(float));
        long arrayPtrAsLong = arrayPtr.ToInt64();
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
            var inputDict = new Dictionary<string, object>
            {
                { "gpuNum", gpuNum },
                { "deltaSaveDir", deltaSaveDir },
                { "imgName", imgName },
                { "rangeBuffer", rangeBuffer },
                { "intensityBuffer", intensityBuffer },
                { "rows", rows },
                { "cols", cols },
                { "xScale", xScale },
                { "xOffset", xOffset },
                { "yScale", yScale },
                { "yOffset", yOffset },
                { "zScale", zScale },
                { "zOffset", zOffset },
                {"arrayPtr", arrayPtrAsLong}
            };
            using (Py.GIL())
            {
                // 调用 Python 函数
                dynamic pyModule = PydModuleMangerAsync.GetPyModule();
                pyModule.Process_3D_Async_Threads(inputDict);
                // pyModule.Process_3D_Async_Multi_Process(inputDict);
            }
            // 判断数组是否已经返回
            var overTimeCount = 0;
            while (overTimeCount < 20)
            {
                // 解决多线程中缓存一致性的问题！！！
                System.Threading.Thread.MemoryBarrier();
                float firstValue = Marshal.PtrToStructure<float>(arrayPtr);
                if (Math.Abs(firstValue - 1.0f) <= 1e-5 || Math.Abs(firstValue - 2.0f) <= 1e-5)
                {
                    break;
                }
                // Sleep - 0.05s
                Thread.Sleep(50);
                overTimeCount++;
            }
            Marshal.Copy(arrayPtr, defectArr, 0, defectArr.Length);
            if (Math.Abs(defectArr[0] - 1.0f) <= 1e-5)
            {
                var defectNum = (int)defectArr[1];
                if (defectNum <= 0 || defectNum > 5)
                {
                    itemForModel.strReturn = false;
                    return itemForModel;
                }
                itemForModel.strReturn = true;
                itemForModel.nDetectNum = defectNum;
                List<TDDetect> defectList = new List<TDDetect>();
                for (int i = 0; i < defectNum; i++)
                {
                    TDDetect defect = new TDDetect();
                    defect.X0 = defectArr[2 + 5 * i];
                    defect.Y0 = defectArr[3 + 5 * i];
                    defect.X1 = defectArr[4 + 5 * i];
                    defect.Y1 = defectArr[5 + 5 * i];
                    defect.DefectName = defectArr[6 + 5 * i] switch
                    {
                        float val when Math.Abs(val - 1.0) <= 1e-6 => "scratch",
                        float val when Math.Abs(val - 2.0) <= 1e-6 => "crack",
                        _ => "other"
                    };
                    defectList.Add(defect);
                }
                itemForModel.defects = defectList;
            }
            else
            {
                itemForModel.strReturn = false;
            }
        }
        catch (Exception e)
        {
            itemForModel.errMsg = e.Message;
            itemForModel.strReturn = false;
            Console.WriteLine($"Error initializing Python engine: {e.Message}");
        }
        finally
        {
            Marshal.FreeHGlobal(arrayPtr);
        }
        return itemForModel;
    }
}