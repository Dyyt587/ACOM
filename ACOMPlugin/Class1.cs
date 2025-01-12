using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using DryIoc;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using Serilog;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
// 示例代码
using ShadowPluginLoader.MetaAttributes;
using ShadowPluginLoader.WinUI;
using ShadowPluginLoader.WinUI.Models;
using Windows.Devices.SerialCommunication;
using ACOMCommmon;
using Microsoft.UI.Xaml.Controls;

 namespace ACOMPlugin.Core;



[ExportMeta]
public class ACOMPluginMetaData : AbstractPluginMetaData
{
    [Meta(Required = true, PropertyGroupName = "Author")]
    public string Author { get; init; }
}

public enum SystemCommandEnum 
{
    Delete,
    Copy,
    Paste,
}
public abstract class ACOMPluginBase : AbstractPlugin
{

    public abstract string GetName();
    public abstract IconSourceElement GetIcon();
    public abstract string GetLabel();
    public abstract string GetTag();
    public abstract FrameworkElement Create();

    public abstract void UpdateData(List<CannelData> newData);
    /// <summary>
    /// cmd 为 cmd_name;cmd_arg1;cmd_arg2
    /// </summary>
    /// <param name="cmd"></param>
    public delegate void UpdateCommands(object sender, List<string[]> cmd);
    public delegate void SystemCommands(object sender, List<SystemCommandEnum> cmd);
    public event UpdateCommands UpdateCommand;//当外部设备变化触发
    public event SystemCommands SystemCommand;//当外部设备变化触发
    public ACOMPluginBase()
    {

    }
    public void OnUpdateCommand(object sender, List<string[]> cmd)
    {
        UpdateCommand?.Invoke(sender, cmd);
    }

    public void OnSystemCommand(object sender, List<SystemCommandEnum> cmd)
    {
        SystemCommand?.Invoke(sender, cmd);
    }
    ~ACOMPluginBase()
    {
        UpdateCommand = null;
        SystemCommand = null;
    }
}
 
public class ACOMPluginLoader : AbstractPluginLoader<ACOMPluginMetaData, ACOMPluginBase>
{
    protected override string PluginFolder { get; } = "ACOMPlugin";

    public ACOMPluginLoader(ILogger logger) : base(logger)
    {
    }
    public ACOMPluginLoader() : base()
    {
    }

}


