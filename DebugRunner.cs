using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using OpenCvSharp;
using System.Numerics;
using System.Globalization;
using System.Runtime.InteropServices;
using Python.Runtime;
using System.Data;
using System.Diagnostics;

namespace WinFormsApp1;

public class DebugRunner
{
    public static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Starting PydProcessData debug run...");
            // 记录程序开始时间
            DateTime startTime = DateTime.Now;

            // 构造线程安全队列并填充数据
            var dataList = new List<ItemForModel>();
            for (int i = 0; i < 80; i++)
            {
                dataList.Add(GetMockData1(i));
                dataList.Add(GetMockData2(i + 80));
            }
            // 创建 PydModuleManager 实例
            var manager = new PydModuleMangerAsync();
            // 使用 Task 和 Parallel 并发处理数据
            int maxDegreeOfParallelism = 2; // 线程池大小为 2
            Parallel.ForEach(dataList, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, data =>
            {
                ItemForModel itemResult = manager.PydProcessDataAsync(data);
                if (itemResult.strReturn)
                {
                    Console.WriteLine($"This time detected some defects.");
                }
            });
            // 记录程序结束时间
            DateTime endTime = DateTime.Now;
            // 计算总耗时
            TimeSpan totalTime = endTime - startTime;
            Console.WriteLine($"All processing completed. Total time: {totalTime.TotalSeconds:F2} seconds.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during debug run: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Press any key to exit...");
        }
    }
    
    
    // 主入口方法，串联，用于调试PydModuleMangerAsync()方法
    public static void MainTest(string[] args)
    {
        try
        {
            Console.WriteLine("Starting PydProcessData debug run...");
            // 记录程序开始时间
            DateTime startTime = DateTime.Now;
            // 构造线程安全队列并填充数据
            var dataList = new List<ItemForModel>();
            for (int i = 0; i < 80; i++)
            {
                dataList.Add(GetMockData1(i));
                // dataList.Add(GetMockData2(i + 10));
            }
            // 创建 PydModuleManager 实例
            var manager = new PydModuleMangerAsync();
            foreach (var data in dataList)
            {
                manager.PydProcessDataAsync(data);
            }
            // 记录程序结束时间
            DateTime endTime = DateTime.Now;
            // 计算总耗时
            TimeSpan totalTime = endTime - startTime;
            Console.WriteLine($"All processing completed. Total time: {totalTime.TotalSeconds:F2} seconds.");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during debug run: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Press any key to exit...");
        }
    }
    
    // // 异步处理单个任务
    // private static async Task<ItemForModel> ProcessItemAsync(PydModuleMangerAsync manager, ItemForModel item)
    // {
    //     try
    //     {
    //         Console.WriteLine($"Processing item: {item.strDetectFileName}");
    //         var result = await manager.PydProcessDataAsync(item);
    //         Console.WriteLine($"Finished processing item: {item.strDetectFileName}, Result: {result.strReturn}");
    //         return result;
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error processing item: {item.strDetectFileName}, Error: {ex.Message}");
    //         throw;
    //     }
    // }
    
    
    public static async Task Main1(string[] args)
    {
        try
        {
            // 单步调用测试方法
            Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during debug run: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Press any key to exit...");
        }
    }

    // 测试方法：模拟调用 PydProcessData()
    public static void Run()
    {
        Queue<ItemForModel> items = new Queue<ItemForModel>();
        for (int i = 0; i < 10; i++)
        {
            // 模拟输入数据i
            var itemForModel1 = GetMockData1(i);
            var itemForModel2 = GetMockData2(2*i);
            items.Enqueue(itemForModel1);
            items.Enqueue(itemForModel2);
            // var itemForModel3 = GetMockData1(3);
            // var itemForModel4 = GetMockData2(4);
            // var itemForModel5 = GetMockData1(5);
            // 调用 PydProcessData() 方法
            // var manager = new PydModuleManager();
            // DateTime time1 = DateTime.Now;
            // manager.PydProcessData(itemForModel1);
            // DateTime time2 = DateTime.Now;
            // manager.PydProcessData(itemForModel2);
            // DateTime time3 = DateTime.Now;
            // manager.PydProcessData(itemForModel3);
            // DateTime time4 = DateTime.Now;
            // manager.PydProcessData(itemForModel4);
            // DateTime time5 = DateTime.Now;
            // manager.PydProcessData(itemForModel5);
            // DateTime time6 = DateTime.Now;
            // Console.WriteLine("1 time:   " + time2.Subtract(time1));
            // Console.WriteLine("2 time:   " + time3.Subtract(time2));
            // Console.WriteLine("3 time:   " + time4.Subtract(time3));
            // Console.WriteLine("4 time:   " + time5.Subtract(time4));
            // Console.WriteLine("5 time:   " + time6.Subtract(time5));
        }
        DateTime startTime = DateTime.Now;
        var manager = new PydModuleManager();
        while(items.Count > 0)
        {
            manager.PydProcessData(items.Dequeue());
        }
        DateTime endTime = DateTime.Now;
        Console.WriteLine("All time:   " + endTime.Subtract(startTime));
    }

    
    // 生产者函数：将数据放入队列
    static async Task ProducerAsync(ConcurrentQueue<ItemForModel> queue, int numItems, CancellationToken cancellationToken)
    {
        for (int i = 0; i < numItems; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            var item1 = GetMockData1(i);
            queue.Enqueue(item1);
            var item2 = GetMockData2(i);
            queue.Enqueue(item2);
        }
    }
    
    // 消费者函数：从队列中取出数据并处理
    static async Task ConsumerAsync(ConcurrentQueue<ItemForModel> queue, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (queue.TryDequeue(out var item))
            {
                var manager = new PydModuleManager();
                manager.PydProcessData(item);
            }
            else
            {
                await Task.Delay(100); // 等待队列中有新数据
            }
        }
        Console.WriteLine("Consumer exiting...");
    }
    
    public static ItemForModel GetMockData1(int count)
    {
        var dataDir = "D:\\02_Data\\2025\\fbcode_c#\\testC#\\WinFormsApp1\\WinFormsApp1\\testimg\\";
        var saveDir = "D:\\02_Data\\2025\\fbcode_c#\\testC#\\WinFormsApp1\\WinFormsApp1\\out\\";
        var datFiles = Directory.GetFiles(dataDir, "*.dat").OrderBy(f => f).ToArray();
        var xmlFiles = Directory.GetFiles(dataDir, "*.xml").OrderBy(f => f).ToArray();
        var items = new List<ItemForModel>();
        foreach (var (datPath, xmlPath) in datFiles.Zip(xmlFiles, (d, x) => (d, x)))
        {
            var xmlValues = ReadXml(xmlPath);
            float xScale = xmlValues[0], xOffset = xmlValues[1],
                yScale = xmlValues[2], yOffset = xmlValues[3],
                zScale = xmlValues[4], zOffset = xmlValues[5];
            int rows = 3000, cols = 2560;
            var (rangeData, intensityData) = ReadDat(datPath, rows, cols);
            // 测试构建一个ItemForModel
            var item = new ItemForModel();
            item.xOffset = xOffset;
            item.yOffset = yOffset;
            item.zOffset = zOffset;
            item.xScale = xScale;
            item.yScale = yScale;
            item.zScale = zScale;
            item.cols = cols;
            item.rows = rows;
            item.rangeBuffer = rangeData;
            item.intensityBuffer = intensityData;
            item.strDetectFileName = new string("img-133-" + count);
            items.Add(item);
        }
        var itemForModel = items[0];
        return itemForModel;
    }
    
    public static ItemForModel GetMockData2(int count)
    {
        var dataDir = "D:\\02_Data\\2025\\fbcode_c#\\testC#\\WinFormsApp1\\WinFormsApp1\\testimg0416\\";
        var saveDir = "D:\\02_Data\\2025\\fbcode_c#\\testC#\\WinFormsApp1\\WinFormsApp1\\out\\";
        var datFiles = Directory.GetFiles(dataDir, "*.dat").OrderBy(f => f).ToArray();
        var xmlFiles = Directory.GetFiles(dataDir, "*.xml").OrderBy(f => f).ToArray();
        var items = new List<ItemForModel>();
        foreach (var (datPath, xmlPath) in datFiles.Zip(xmlFiles, (d, x) => (d, x)))
        {
            var xmlValues = ReadXml(xmlPath);
            float xScale = xmlValues[0], xOffset = xmlValues[1],
                yScale = xmlValues[2], yOffset = xmlValues[3],
                zScale = xmlValues[4], zOffset = xmlValues[5];
            int rows = 3000, cols = 2560;
            var (rangeData, intensityData) = ReadDat(datPath, rows, cols);
            // 测试构建一个ItemForModel
            var item = new ItemForModel();
            item.xOffset = xOffset;
            item.yOffset = yOffset;
            item.zOffset = zOffset;
            item.xScale = xScale;
            item.yScale = yScale;
            item.zScale = zScale;
            item.cols = cols;
            item.rows = rows;
            item.rangeBuffer = rangeData;
            item.intensityBuffer = intensityData;
            item.strDetectFileName = new string("img-133-" + count);
            items.Add(item);
        }
        var itemForModel = items[0];
        return itemForModel;
    }
    
    private static float[] ReadXml(string xmlPath)
    {
        var doc = XDocument.Load(xmlPath);
        var component = doc.Descendants("component")
            .First(e => e.Attribute("name")?.Value == "Ranger3Range");

        string[] paramNames = {
            "a axis range scale", "a axis range offset",
            "b axis range scale", "b axis range offset",
            "c axis range scale", "c axis range offset"
        };

        return paramNames.Select(name =>
        {
            var paramElement = component.Descendants("parameter").FirstOrDefault(p => p.Attribute("name")?.Value == name);
            if (paramElement == null)
            {
                throw new InvalidOperationException($"Parameter '{name}' not found.");
            }
            return float.Parse(paramElement.Value, CultureInfo.InvariantCulture);
        }).ToArray();
    }

    private static (byte[] rangeData, byte[] intensityData) ReadDat(string path, int rows, int cols)
    {
        using (var reader = new BinaryReader(File.OpenRead(path)))
        {
            var rangeBytes = reader.ReadBytes(rows * cols * 2);
            var intensityBytes = reader.ReadBytes(rows * cols);
            return (rangeBytes, intensityBytes);
        }
    }
}