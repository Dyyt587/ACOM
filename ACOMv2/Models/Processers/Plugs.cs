using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ACOMPlug;
using ACOMPlugin.Core;
using ACOMv2.Common;
using DryIoc;
using ShadowPluginLoader.WinUI;
using STTech.BytesIO.Core;
using Windows.System;

namespace ACOMv2.Models.Processers;

public class Plugs 
{

    /// <summary>
    /// 递归读取文件夹中名字开头为Plug_的后缀为DLL的文件
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <returns>符合条件的文件列表</returns>
    //public static List<string> GetPlugDllFiles(string path)
    //{
    //    List<string> dllFiles = new List<string>();
    //    string ProcessersPath = path + "Processers";
    //    string DataMuxersPath = path + "DataMuxers";
    //    GetPlugDllFilesRecursive(ProcessersPath, dllFiles);
    //    GetPlugDllFilesRecursive(DataMuxersPath, dllFiles);
    //    return dllFiles;
    //}

    //private static void GetPlugDllFilesRecursive(string path, List<string> dllFiles)
    //{
    //    try
    //    {
    //        foreach (var file in Directory.GetFiles(path, "ProcesserPlug*.dll"))
    //        {
    //            dllFiles.Add(file);
    //        }
    //        foreach (var file in Directory.GetFiles(path, "DataMuxerPlug*.dll"))
    //        {
    //            dllFiles.Add(file);
    //        }
    //        foreach (var directory in Directory.GetDirectories(path))
    //        {
    //            GetPlugDllFilesRecursive(directory, dllFiles);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"Error accessing {path}: {ex.Message}");
    //    }
    //}


    /// <summary>
    /// 当前拥有的插件
    /// </summary>
    static public Dictionary<string, Type> IProcessersPlugins = new Dictionary<string, Type>();
    static public Dictionary<string, Type> IDataMuxersPlugins = new Dictionary<string, Type>();
    static public Dictionary<string, ACOMPluginBase> WidgetPlugins = new Dictionary<string, ACOMPluginBase>();

    /// <summary>
    /// 当前拥有的插件信息
    /// </summary>
    static public Dictionary<string, PluginInfo> IPluginInfos = new Dictionary<string, PluginInfo>();
    /// <summary>
    /// 文件监听
    /// </summary>
    //static FileListenerServer _fileListener = null;





    static protected void LoadDataMuxPlug(string _path)
    {
        List<string> dllFiles = new List<string>();
        string path = _path + "DataMuxers";
        try
        {
            foreach (var file in Directory.GetFiles(path, "DataMuxerPlug*.dll"))
            {
                dllFiles.Add(file);
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error accessing {path}: {ex.Message}");
        }
        Debug.WriteLine("find accept data mutex plug files:");

        foreach (var item in dllFiles)
        {
            Debug.WriteLine(item);
        }

        for (int i = 0; i < dllFiles.Count(); i++)
        {
            var fileData = File.ReadAllBytes(dllFiles[i]);
            Assembly asm = Assembly.Load(fileData);
            var manifestModuleName = asm.ManifestModule.ScopeName;
            var classLibrayName = manifestModuleName.Remove(manifestModuleName.LastIndexOf("."), manifestModuleName.Length - manifestModuleName.LastIndexOf("."));
            Type type = asm.GetType("DataMuxerPlugAutoOne.DataMuxerPlugAutoOne");
            Debug.WriteLine("加载" + dllFiles[i]);
            foreach (var item in asm.GetTypes())
            {
                Console.WriteLine(item.Name);
            }
            if (!typeof(IPlugDataMuxBase).IsAssignableFrom(type))
            {
                Debug.WriteLine("未继承插件接口");
                continue;
            }
            var instance = Activator.CreateInstance(type) as IPlugDataMuxBase;
            var protocolInfo = instance.GetPluginInformation();
            Debug.WriteLine($"插件名称：{protocolInfo.Name}");
            Debug.WriteLine($"插件版本：{protocolInfo.Version}");
            Debug.WriteLine($"插件作者：{protocolInfo.Author}");
            Debug.WriteLine($"插件时间：{protocolInfo.LastTime}");
            IDataMuxersPlugins.Add(protocolInfo.Name, type);
            IPluginInfos.Add(protocolInfo.Name, protocolInfo);
            instance.Dispose();
            instance = null;
        }
    }

    static protected void LoadProcesserPlug(string _path)
    {
        List<string> dllFiles = new List<string>();
        string path = _path + "Processers";
        try
        {
            foreach (var file in Directory.GetFiles(path, "ProcesserPlug*.dll"))
            {
                dllFiles.Add(file);
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error accessing {path}: {ex.Message}");
        }
        Debug.WriteLine("find accept processer plug files:");

        foreach (var item in dllFiles)
        {
            Debug.WriteLine(item);
        }

        for (int i = 0; i < dllFiles.Count(); i++)
        {
            var fileData = File.ReadAllBytes(dllFiles[i]);
            Assembly asm = Assembly.Load(fileData);
            var manifestModuleName = asm.ManifestModule.ScopeName;
            var classLibrayName = manifestModuleName.Remove(manifestModuleName.LastIndexOf("."), manifestModuleName.Length - manifestModuleName.LastIndexOf("."));
            Type type = asm.GetType("ProcesserPlugWaterFire.ProcesserPlugWaterFire");
            Debug.WriteLine("加载" + dllFiles[i]);
            foreach (var item in asm.GetTypes())
            {
                Console.WriteLine(item.Name);
            }
            if (!typeof(IPlugProcessBase).IsAssignableFrom(type))
            {
                Debug.WriteLine("未继承插件接口");
                continue;
            }
            var instance = Activator.CreateInstance(type) as IPlugProcessBase;
            var protocolInfo = instance.GetPluginInformation();
            Debug.WriteLine($"插件名称：{protocolInfo.Name}");
            Debug.WriteLine($"插件版本：{protocolInfo.Version}");
            Debug.WriteLine($"插件作者：{protocolInfo.Author}");
            Debug.WriteLine($"插件时间：{protocolInfo.LastTime}");
            IProcessersPlugins.Add(protocolInfo.Name, type);
            IPluginInfos.Add(protocolInfo.Name, protocolInfo);
            instance.Dispose();
            instance = null;
        }
    }

    private static async Task InitPlug()
    {
        var loader = DiFactory.Services.Resolve<ACOMPluginLoader>();
        //await loader.ImportFromZipAsync(@"C:\Users\80520\source\repos\ACOM\Packages");

        //
        string directoryPath = @"C:\ACOM\Packages"; // 替换为你的文件夹路径
        //string directoryPath = @"C:\Users\80520\source\repos\ACOM\plugin_test2\bin\Debug"; // 替换为你的文件夹路径

        if (Directory.Exists(directoryPath))
        {
            int cnt = 0;
            string[] subDirectories = Directory.GetDirectories(directoryPath);
            foreach (string subDirectory in subDirectories)
            {
                Debug.WriteLine("进入目录 " + subDirectory);
                await loader.ImportFromDirAsync(subDirectory);


                Debug.WriteLine("加载到 " + loader.GetPlugins().Count().ToString() + " 个插件");
                foreach (var plugin in loader.GetPlugins())
                {
                    cnt++;
                    WidgetPlugins.Add(plugin.Id, plugin);
                    Debug.WriteLine(plugin.Id);
                    FrameworkElement element = plugin.Create();
                }
            }
            await loader.ImportFromZipAsync(directoryPath);
            Debug.WriteLine("加载到 " + loader.GetPlugins().Count().ToString() + " 个插件");
            Debug.WriteLine("一共加载插件个数 " + cnt.ToString());

        }
        else
        {
            Console.WriteLine("目录不存在");
        }


    }
    /// <summary>
    /// 初始化插件
    /// </summary>
    static public async Task InitAsync(string path)
    {

        //LoadProcesserPlug(path);
        //LoadDataMuxPlug(path);
        //LoadWidgetPlug(path);


        await InitPlug();

        Debug.WriteLine(string.Format("==========【{0}】==========", "插件加载完成"));
        Debug.WriteLine(string.Format("==========【{0}】==========", "共加载插件{0}个"), IProcessersPlugins.Count+ IDataMuxersPlugins.Count + WidgetPlugins.Count);






    }

    static public IPlugProcessBase CreateInstance(string PlugName)
    {
        if (IProcessersPlugins.ContainsKey(PlugName))
        {
            return Activator.CreateInstance(IProcessersPlugins[PlugName]) as IPlugProcessBase;
        }
        else
        {
            Debug.WriteLine("error plug name:"+PlugName);
            return null;
        }
    }


}
