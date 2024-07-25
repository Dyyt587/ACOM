using System.Diagnostics;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace ACOM.Views;

using System.Collections.ObjectModel;
using System.Management;
using CommunityToolkit.Mvvm.ComponentModel;

using ICommand = System.Windows.Input.ICommand;

class SerialPortFindTool
{
    /// <summary>
    /// 枚举win32 api
    /// </summary>
    private enum HardwareEnum
    {
        // 硬件
        Win32_Processor, // CPU 处理器
        Win32_PhysicalMemory, // 物理内存条
        Win32_Keyboard, // 键盘
        Win32_PointingDevice, // 点输入设备，包括鼠标。
        Win32_FloppyDrive, // 软盘驱动器
        Win32_DiskDrive, // 硬盘驱动器
        Win32_CDROMDrive, // 光盘驱动器
        Win32_BaseBoard, // 主板
        Win32_BIOS, // BIOS 芯片
        Win32_ParallelPort, // 并口
        Win32_SerialPort, // 串口
        Win32_SerialPortConfiguration, // 串口配置
        Win32_SoundDevice, // 多媒体设置，一般指声卡。
        Win32_SystemSlot, // 主板插槽 (ISA & PCI & AGP)
        Win32_USBController, // USB 控制器
        Win32_NetworkAdapter, // 网络适配器
        Win32_NetworkAdapterConfiguration, // 网络适配器设置
        Win32_Printer, // 打印机
        Win32_PrinterConfiguration, // 打印机设置
        Win32_PrintJob, // 打印机任务
        Win32_TCPIPPrinterPort, // 打印机端口
        Win32_POTSModem, // MODEM
        Win32_POTSModemToSerialPort, // MODEM 端口
        Win32_DesktopMonitor, // 显示器
        Win32_DisplayConfiguration, // 显卡
        Win32_DisplayControllerConfiguration, // 显卡设置
        Win32_VideoController, // 显卡细节。
        Win32_VideoSettings, // 显卡支持的显示模式。

        // 操作系统
        Win32_TimeZone, // 时区
        Win32_SystemDriver, // 驱动程序
        Win32_DiskPartition, // 磁盘分区
        Win32_LogicalDisk, // 逻辑磁盘
        Win32_LogicalDiskToPartition, // 逻辑磁盘所在分区及始末位置。
        Win32_LogicalMemoryConfiguration, // 逻辑内存配置
        Win32_PageFile, // 系统页文件信息
        Win32_PageFileSetting, // 页文件设置
        Win32_BootConfiguration, // 系统启动配置
        Win32_ComputerSystem, // 计算机信息简要
        Win32_OperatingSystem, // 操作系统信息
        Win32_StartupCommand, // 系统自动启动程序
        Win32_Service, // 系统安装的服务
        Win32_Group, // 系统管理组
        Win32_GroupUser, // 系统组帐号
        Win32_UserAccount, // 用户帐号
        Win32_Process, // 系统进程
        Win32_Thread, // 系统线程
        Win32_Share, // 共享
        Win32_NetworkClient, // 已安装的网络客户端
        Win32_NetworkProtocol, // 已安装的网络协议
        Win32_PnPEntity,//all device
    }
    /// <summary>
    /// WMI取硬件信息
    /// </summary>
    /// <param name="hardType"></param>
    /// <param name="propKey"></param>
    /// <returns></returns>
    private static string[] MulGetHardwareInfo(HardwareEnum hardType, string propKey)
    {
        List<string> strs = new List<string>();
        try
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + hardType))
            {
                var hardInfos = searcher.Get();
                foreach (var hardInfo in hardInfos)
                {
                    if (hardInfo.Properties[propKey].Value != null && hardInfo.Properties[propKey].Value.ToString().Contains("COM"))
                    {
                        strs.Add(hardInfo.Properties[propKey].Value.ToString());
                    }

                }
                searcher.Dispose();
            }

            return strs.ToArray();
        }
        catch
        {
            return strs.ToArray();
        }
    }

    /// <summary>
    /// 串口信息
    /// </summary>
    /// <returns></returns>
    public static string[] GetSerialPort()
    {
        return MulGetHardwareInfo(HardwareEnum.Win32_PnPEntity, "Name");
    }
}






/// <summary>
/// 单例模式的实现
/// </summary>
public class MainPage_Singleton : ObservableObject
{
    // 定义一个静态变量来保存类的实例
    private static MainPage_Singleton? uniqueInstance;

    // 定义一个标识确保线程同步
    private static readonly object locker = new();

    string _DataList_ID;
    public string DataList_ID
    {
        get => _DataList_ID;
        set => SetProperty(ref _DataList_ID, value);
    }
    public bool Is_up = false;//appbar
    // 定义私有构造函数，使外界不能创建该类实例
    private MainPage_Singleton()
    {
        DataList_ID = string.Empty;
    }

    /// <summary>
    /// 定义公有方法提供一个全局访问点,同时你也可以定义公有属性来提供全局访问点
    /// </summary>
    /// <returns></returns>
    public static MainPage_Singleton GetInstance()
    {
        // 当第一个线程运行到这里时，此时会对locker对象 "加锁"，
        // 当第二个线程运行该方法时，首先检测到locker对象为"加锁"状态，该线程就会挂起等待第一个线程解锁
        // lock语句运行完之后（即线程运行完之后）会对该对象"解锁"
        // 双重锁定只需要一句判断就可以了
        if (uniqueInstance == null)
        {
            lock (locker)
            {
                // 如果类的实例不存在则创建，否则直接返回
                uniqueInstance ??= new MainPage_Singleton();
            }
        }
        return uniqueInstance;
    }
}
public class LinkDeviceDates : ObservableObject
{
    private string _DeviceName;
    private string _DeviceDesc = "NO desc";
    private int _boundRate = 115200;
    private int _dateBit = 8;
    private string _checkBit = "N";
    private string _stopBit = "1";
    private string _streamCtrl = "XON/XOFF";
    private string _overView = "NONE";

    public string is_connect = "false";


    public string ConnectState
    {
        get => is_connect;
        set
        {
            SetProperty(ref is_connect, value);
            if (is_connect == "true")
            {
                Connect();
            }
            else
            {
                DisConnect();
            }
        }
    }
    public void Connect()
    {
        Debug.WriteLine(_DeviceName + "connect");

        is_connect = "true";
    }
    public void DisConnect()
    {
        Debug.WriteLine(_DeviceName + "disconnect");
        is_connect = "false";
    }
    public string DeviceName
    {
        get => _DeviceName;
        set => SetProperty(ref _DeviceName, value);
    }

    public string DeviceDesc
    {
        get => _DeviceDesc;
        set => SetProperty(ref _DeviceDesc, value);
    }

    public int BoundRate
    {
        get => (int)_boundRate;
        set => SetProperty(ref _boundRate, value);
    }

    public int DateBit
    {
        get => (int)_dateBit;
        set { SetProperty(ref _dateBit, value); Update(); }
    }

    public string CheckBit
    {
        get => (string)_checkBit;
        set
        {
            SetProperty(ref _checkBit, value); Update();
        }
    }
    public string StopBit
    {
        get => (string)_stopBit;
        set
        {
            SetProperty(ref _stopBit, value); Update();
        }
    }
    public string StreamCtrl
    {
        get => (string)_streamCtrl;
        set
        {
            SetProperty(ref _streamCtrl, value); Update();
        }
    }

    public string OverView
    {
        get => (string)_overView;
        set => SetProperty(ref _overView, value);
    }
    public void Update()
    {
        OverView = _boundRate.ToString() + " " + _dateBit.ToString() + _checkBit.ToString() + _stopBit.ToString();
    }
    public LinkDeviceDates(string deviceName)
    {
        DeviceName = deviceName;
        Update();
    }

    public LinkDeviceDates(string deviceName, string deviceDesc)
    {
        DeviceName = deviceName;
        DeviceDesc = deviceDesc;
        //DeviceDesc.Replace(deviceName,"");
        Update();
    }


}
public class SmallPartDates : ObservableObject
{

    public SmallPartDates()
    {

    }
}

public class DataListDatas : ObservableObject
{
    private string _DataName;
    public string DataName
    {
        get => _DataName;
        set => SetProperty(ref _DataName, value);
    }
    public string ID
    {
        get; private set;
    }
    private double _Data;
    public double Data
    {
        get => _Data;
        set => SetProperty(ref _Data, value);
    }
    private SolidColorBrush _DataColor;
    public SolidColorBrush DataColor
    {
        get => _DataColor;
        set
        {
            SetProperty(ref _DataColor, value);

        }
    }
    public bool is_View
    {
        get; set;
    }
    public ICommand Command
    {
        get; set;
    }

    public DataListDatas(string dataName, double data, string id)
    {

        //DataColor = new SolidColorBrush(Colors.Salmon);
        DataColor = new SolidColorBrush(Microsoft.UI.Colors.RoyalBlue);
        DataName = dataName;
        ID = id;
        Data = data;
        var deleteCommand = new StandardUICommand(StandardUICommandKind.Stop);
        // deleteCommand.ExecuteRequested += DeleteCommand_ExecuteRequested;
        Command = deleteCommand;
        is_View = true;
    }
    public DataListDatas(string dataName, double data, string id, Windows.UI.Color color)
    {
        //DataColor = new SolidColorBrush(Colors.Salmon);
        DataColor = new SolidColorBrush(color);
        DataName = dataName;
        ID = id;
        Data = data;
        var deleteCommand = new StandardUICommand(StandardUICommandKind.Stop);
        // deleteCommand.ExecuteRequested += DeleteCommand_ExecuteRequested;
        Command = deleteCommand;
        is_View = true;
    }
    public bool is_equal(string id)
    {
        return ID == id;
    }
}



public sealed partial class HomeLandingPage : Page
{
    public string AppInfo { get; set; }
    public HomeLandingViewModel ViewModel { get; }


    //public string[] SerialPortsSource;
    public ObservableCollection<DataListDatas> dateSource = new(); //数据颜色
    public ObservableCollection<LinkDeviceDates> linkDeviceSource = new(); //连接设备
    public ObservableCollection<string> SerialPortsSource = new(); //连接设备

    public MainPage_Singleton mainPage_Singleton = MainPage_Singleton.GetInstance();

    ManagementEventWatcher USBInsert;
    ManagementEventWatcher USBRemove;
    string[] Ports = { "NULL" };
    string[] PortsDesc = { "NULL" };


    // 当文件系统发生变化时调用的事件处理器
    private static void USBOnChanged(object source, FileSystemEventArgs e)
    {
        Debug.WriteLine($"Port {e.FullPath} has changed.");
    }

    // 当文件被删除时调用的事件处理器
    private static void USBOnDeleted(object source, FileSystemEventArgs e)
    {
        Debug.WriteLine($"Port {e.FullPath} has been deleted.");
    }




    Windows.UI.Color ColorFromHSV(double hue, double saturation, double value)
    {
        double chroma = value * saturation;
        double huePrime = hue / 60.0;
        double x = chroma * (1 - Math.Abs(huePrime % 2 - 1));

        double red = 0, green = 0, blue = 0;

        if (huePrime >= 0 && huePrime <= 1)
        {
            red = chroma;
            green = x;
        }
        else if (huePrime > 1 && huePrime <= 2)
        {
            red = x;
            green = chroma;
        }
        else if (huePrime > 2 && huePrime <= 3)
        {
            green = chroma;
            blue = x;
        }
        else if (huePrime > 3 && huePrime <= 4)
        {
            green = x;
            blue = chroma;
        }
        else if (huePrime > 4 && huePrime <= 5)
        {
            red = x;
            blue = chroma;
        }
        else if (huePrime > 5 && huePrime <= 6)
        {
            red = chroma;
            blue = x;
        }

        double m = value - chroma;

        red += m;
        green += m;
        blue += m;

        byte redByte = (byte)(red * 255);
        byte greenByte = (byte)(green * 255);
        byte blueByte = (byte)(blue * 255);

        return Windows.UI.Color.FromArgb(255, redByte, greenByte, blueByte);
    }

    public Windows.UI.Color GenerateDistinctColor(int number)
    {
        if (number < 0 || number > 127)
        {
            number = 127;
        }

        double hue = (number * 2.83) % 360; // 将数字映射到色轮上

        return ColorFromHSV(hue, 1, 1);
    }


    /// <summary>
    /// USB设备插入
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void USBInsert_EventArrived(object sender, EventArrivedEventArgs e)
    {
        Debug.WriteLine("Insert start");

        string[] tempPorts = System.IO.Ports.SerialPort.GetPortNames();
        if (tempPorts.Count() == Ports.Count())
            return;
        else
            Ports = tempPorts;

        PortsDesc = SerialPortFindTool.GetSerialPort();
        ChangeCOM();

        Debug.WriteLine("Insert  end");

        //if (IsConnected)
        //    return;

        //if (blnDesireConnected && Open())
        //    commExecuteInterface?.DeviceArrivaled();
    }

    /// <summary>
    /// USB设备拔出
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void USBRemove_EventArrived(object sender, EventArrivedEventArgs e)
    {
        string[] tempPorts = System.IO.Ports.SerialPort.GetPortNames();
        if (tempPorts.Count() == Ports.Count())
            return;
        else
            Ports = tempPorts;
        ChangeCOM();

        //if (!IsConnected)
        //    return;

        //IsConnected = false;
        //spUSB.Close();
        //commExecuteInterface?.DeviceRemoved();
    }
    public void ChangeCOM()
    {
        Ports = System.IO.Ports.SerialPort.GetPortNames();
        // 使用lambda表达式创建线程


        DispatcherQueue.TryEnqueue(() =>
        {
            linkDeviceSource.Clear();
            foreach (string port in Ports)
            {
                Debug.WriteLine(port);
                foreach (string p in PortsDesc)
                {
                    if (p.Contains(port))
                    {
                        linkDeviceSource.Add(new LinkDeviceDates(port, p));
                    }
                }

                //SerialPortsSource.Add(port);
            }
        });        //});
        //thread1.Start();
    }


    public HomeLandingPage()
    {


        // 使用lambda表达式创建线程
        Thread thread = new Thread(() =>
        {
            Console.WriteLine("线程启动，执行任务。");
            PortsDesc = SerialPortFindTool.GetSerialPort();
            ChangeCOM();
            //建立监听
            ManagementScope scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;
            //建立插入监听
            try
            {
                WqlEventQuery USBInsertQuery = new WqlEventQuery("__InstanceCreationEvent", "TargetInstance ISA 'Win32_PnPEntity'");
                USBInsertQuery.WithinInterval = new TimeSpan(0, 0, 2);
                USBInsert = new ManagementEventWatcher(scope, USBInsertQuery);
                USBInsert.EventArrived += USBInsert_EventArrived;
                USBInsert.Start();
            }
            catch (Exception ex)
            {
                if (USBInsert != null)
                {
                    USBInsert.Stop();
                }
                throw ex;
            }
            //建立拔出监听
            try
            {
                WqlEventQuery USBRemoveQuery = new WqlEventQuery("__InstanceDeletionEvent", "TargetInstance ISA 'Win32_PnPEntity'");
                USBRemoveQuery.WithinInterval = new TimeSpan(0, 0, 2);
                USBRemove = new ManagementEventWatcher(scope, USBRemoveQuery);
                USBRemove.EventArrived += USBRemove_EventArrived;
                USBRemove.Start();
            }
            catch (Exception ex)
            {
                if (USBRemove != null)
                {
                    USBRemove.Stop();
                }
                throw ex;
            }

        });

        // 启动线程
        thread.Start();


        ChangeCOM();


        for (int i = 0; i < 20; i++)
        {
            dateSource.Add(new DataListDatas("data" + i.ToString(), 0, i.ToString()));
            dateSource[i].DataColor.Color = GenerateDistinctColor((i * 78) % 128);
        }


        //// 获取所有可用的串口名称
        //Ports = System.IO.Ports.SerialPort.GetPortNames();

        //// 检查是否有可用的串口
        //if (Ports.Length == 0)
        //{
        //    Debug.WriteLine("没有找到串口。");
        //    return;
        //}

        ViewModel = App.GetService<HomeLandingViewModel>();
        this.InitializeComponent();
        AppInfo = $"{App.Current.AppName} v{App.Current.AppVersion}";
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        allLandingPage.GetData(ViewModel.JsonNavigationViewService.DataSource);
        allLandingPage.OrderBy(i => i.Title);
    }




    private void TabView_Loaded(object sender, RoutedEventArgs e)
    {
        for (int i = 0; i < 1; i++)
        {
            if (sender != null)
            {
                (sender as TabView).TabItems.Add(CreateNewTab(i));
            }
        }
    }

    private void TabView_AddButtonClick(TabView sender, object args)
    {
        sender.TabItems.Add(CreateNewTab(sender.TabItems.Count));
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        sender.TabItems.Remove(args.Tab);
    }

    private TabViewItem CreateNewTab(int index)
    {
        TabViewItem newItem = new TabViewItem();

        newItem.Header = $"Document {index}";
        newItem.IconSource = new SymbolIconSource() { Symbol = Symbol.Document };

        // The content of the tab is often a frame that contains a page, though it could be any UIElement.
        Frame frame = new Frame();

        switch (index % 3)
        {
            case 0:
                frame.Navigate(typeof(MainCanvasPage));
                break;
            case 1:
                frame.Navigate(typeof(MainCanvasPage));
                break;
            case 2:
                frame.Navigate(typeof(MainCanvasPage));
                break;
        }

        newItem.Content = frame;

        return newItem;
    }

    private void HoverButton_Click(object sender, RoutedEventArgs e)
    {
        AppBarButton? button = sender as AppBarButton;
        Grid parent = button.FindParent<Grid>();
        foreach (var item in dateSource)
        {
            if (item.is_equal(parent.Tag as string) == true)
            {
                if (button != null)
                {
                    if (item.is_View == true)
                    {
                        FontIcon icon = new FontIcon();
                        icon.Glyph = "\uED1A";

                        button.Icon = icon;
                        item.is_View = false;
                    }
                    else
                    {
                        FontIcon icon = new FontIcon();
                        icon.Glyph = "\uE890";

                        button.Icon = icon;
                        item.is_View = true;
                    }

                }
            }
        }
    }

    private void ShowMenu(UIElement sender, bool isTransient)
    {
        FlyoutShowOptions myOption = new FlyoutShowOptions();
        myOption.ShowMode = isTransient ? FlyoutShowMode.Transient : FlyoutShowMode.Standard;
        CommandBarFlyout1.ShowAt(sender, myOption);
    }

    private void DataList_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
    {
        Grid grid = sender as Grid;
        if (grid != null)
        {
            mainPage_Singleton.DataList_ID = grid.Tag as string;
            ShowMenu(sender, true);
        }
    }

    private void DataList_ContextCanceled(UIElement sender, RoutedEventArgs e)
    {
        // Show a context menu in transient mode
        // Focus will not move to the menu
        // ShowMenu(sender, true);
    }

    private void AppBarButton_Click_Play(object sender, RoutedEventArgs e)
    {
        var barbutton = sender as AppBarToggleButton;

        if (barbutton != null)
        {
            if (barbutton.IsChecked == true)
            {
                FontIcon icon = new FontIcon();
                icon.Glyph = "\uE769";
                barbutton.Icon = icon;
            }
            else
            {
                FontIcon icon = new FontIcon();
                icon.Glyph = "\uE768";
                barbutton.Icon = icon;
            }
        }
    }
    private void AppBarButton_Click(object sender, RoutedEventArgs e)
    {
    }
    private void AppBarButton_Click_Down(object sender, RoutedEventArgs e)
    {
        var barbutton = sender as AppBarButton;

        if (barbutton != null)
        {

            if (mainPage_Singleton.Is_up == true)
            {
                FontIcon icon = new FontIcon();
                icon.Glyph = "\uE896";
                barbutton.Icon = icon;
                mainPage_Singleton.Is_up = false;
            }
            else
            {
                FontIcon icon = new FontIcon();
                icon.Glyph = "\uE898";
                barbutton.Icon = icon;
                mainPage_Singleton.Is_up = true;
            }
        }
    }
    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        //textBox.Background = new SolidColorBrush(Colors.RoyalBlue);
        // string str = mainPage_Singleton.DataList_ID.ToString();
        if (textBox != null)
        {
            var str = textBox.Tag as string;
            dateSource[Convert.ToInt32(str)].DataName = textBox.Text;
        }
    }

    private void myColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        var str = sender.Tag as string;
        dateSource[Convert.ToInt32(str)].DataColor.Color = sender.Color;
        //dateSource[Convert.ToInt32(str)].DataColor = new SolidColorBrush(sender.Color);
    }

    private void AppBarButton_Click_ADD(object sender, RoutedEventArgs e)
    {
    }

    private void ComboBox_DropDownOpened(object sender, object e)
    {
        Debug.WriteLine("start to scanf serial");
        string[] _SerialPortsSource = System.IO.Ports.SerialPort.GetPortNames();
        SerialPortsSource.Clear();
        foreach (string port in _SerialPortsSource)
        {
            Debug.WriteLine("serial com" + port);
            SerialPortsSource.Add(port);
        }
    }


    private void combox_COM_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 获取发送事件的 ComboBox
        var comboBox = sender as Microsoft.UI.Xaml.Controls.ComboBox;

        if (comboBox != null)
        {
            if (comboBox.SelectedItem != null)
            {
                foreach (LinkDeviceDates dev in linkDeviceSource)
                {
                    if (dev.DeviceName.Equals(comboBox.SelectedItem.ToString()))
                    {
                        LinkSerial_Boundrate.SelectedValue = dev.BoundRate.ToString();
                        LinkSerial_DataLength.SelectedValue = dev.DateBit;
                        LinkSerial_StopBit.SelectedValue = dev.StopBit;
                        LinkSerial_StreamCtrl.SelectedValue = dev.StreamCtrl;
                    }
                }
            }
            // 获取当前选中项
            var selectedItem = comboBox.SelectedItem;
            foreach (var port in PortsDesc)
            {
                Debug.WriteLine(port);
                if (selectedItem != null)
                {
                    if (port.Contains(selectedItem.ToString()))
                    {
                        TextBlockCOM_Desc.Text = port;
                        Console.WriteLine($"Changed selection to: {selectedItem}");
                        return;
                    }
                }
            }
        }
    }

    private void combox_COM_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
    {
        // 获取发送事件的 ComboBox
        var comboBox = sender as Microsoft.UI.Xaml.Controls.ComboBox;

        if (comboBox != null)
        {
            // 获取当前选中项
            var selectedItem = comboBox.SelectedItem;
            //PortsDesc = SerialPortFindTool.GetSerialPort();



            foreach (var port in PortsDesc)
            {
                Debug.WriteLine(port);
                if (selectedItem != null)
                {
                    if (port.Contains(selectedItem.ToString()))
                    {
                        TextBlockCOM_Desc.Text = port;
                        Console.WriteLine($"Changed selection to: {selectedItem}");
                        return;
                    }
                }
            }
            // 执行相应操作
            TextBlockCOM_Desc.Text = "没有这个COM口，请再次尝试";
        }
    }

    private void LinkSerial_COM_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Debug.Write("dei");
        if (LinkSerial_Boundrate != null && LinkSerial_DataLength != null &&
            LinkSerial_StopBit != null && LinkSerial_StreamCtrl != null &&
            LinkSerial_Boundrate.SelectedValue != null && LinkSerial_DataLength.SelectedValue != null &&
            LinkSerial_StopBit.SelectedValue != null && LinkSerial_StreamCtrl.SelectedValue != null
            )
        {
            //if()
            foreach (LinkDeviceDates dev in linkDeviceSource)
            {
                if (dev.DeviceName.Equals(combox_COM.SelectedItem.ToString()))
                {
                    Debug.WriteLine(LinkSerial_Boundrate.SelectedValue.ToString());
                    dev.BoundRate = Convert.ToInt32(LinkSerial_Boundrate.SelectedValue.ToString());
                    dev.DateBit = (int)LinkSerial_DataLength.SelectedValue;
                    dev.StopBit = (string)LinkSerial_StopBit.SelectedValue;
                    dev.StreamCtrl = (string)LinkSerial_StreamCtrl.SelectedValue;
                    if (LinkSerial_CheckBit.SelectedIndex == 0)
                    {
                        dev.CheckBit = "N";
                    }
                    else if (LinkSerial_CheckBit.SelectedIndex == 1)
                    {
                        dev.CheckBit = "O";
                    }
                    else if (LinkSerial_CheckBit.SelectedIndex == 2)
                    {
                        dev.CheckBit = "D";
                    }
                    else
                    {
                        dev.CheckBit = " ";
                    }
                }
            }
        }

    }

    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        if (LinkSerial_Boundrate != null && LinkSerial_DataLength != null &&
            LinkSerial_StopBit != null && LinkSerial_StreamCtrl != null &&
            LinkSerial_Boundrate.SelectedValue != null && LinkSerial_DataLength.SelectedValue != null &&
            LinkSerial_StopBit.SelectedValue != null && LinkSerial_StreamCtrl.SelectedValue != null
            )
        {
            if (combox_COM.SelectedItem != null)
            {
                foreach (LinkDeviceDates dev in linkDeviceSource)
                {
                    if (dev.DeviceName.Equals(combox_COM.SelectedItem.ToString()))
                    {
                        if (ConnectButton.IsChecked == false)
                        {
                            dev.DisConnect();
                        }
                        else
                        {
                            dev.Connect();
                        }
                    }
                }
            }
            else
            {
                // ConnectButton. = false;  
            }

        }
        else
        {
            // ConnectButton.Checked = false;
        }

    }

    private void LinkButton_Click(object sender, RoutedEventArgs e)
    {
        AppBarButton button = sender as AppBarButton;
        var barbutton = sender as AppBarButton;

        if (barbutton != null)
        {
            string is_connect = barbutton.Tag as string;
            if (mainPage_Singleton.Is_up == true)
            {
                FontIcon icon = new FontIcon();
                icon.Glyph = "\uE896";
                barbutton.Icon = icon;
                mainPage_Singleton.Is_up = false;
            }
            else
            {
                FontIcon icon = new FontIcon();
                icon.Glyph = "\uE898";
                barbutton.Icon = icon;
                mainPage_Singleton.Is_up = true;
            }
        }
    }
}

