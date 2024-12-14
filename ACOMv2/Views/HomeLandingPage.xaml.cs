namespace ACOMv2.Views;

using System.Diagnostics;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ICommand = System.Windows.Input.ICommand;
using ACOM.Models;
using Microsoft.UI.Xaml.Controls;
using TextControlBoxNS;
using Windows.System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WinUICommunity;
using System.Management;
using static ACOMv2.ViewModels.HomeLandingViewModel;
using CommunityToolkit.WinUI.Collections;
using ACOMPlug;
using static ACOM.Models.IO_Manage;
using static ACOMPlug.RawDataMassage;






/// <summary>
/// 单例模式的实现
/// </summary>
public class MainPage_Singleton : ObservableObject
{
    // 定义一个静态变量来保存类的实例
    private static MainPage_Singleton? uniqueInstance;
    //public IO_Manage ioManage;
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



public class ConsolString : INotifyPropertyChanged
{
    private string _totalString;

    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    public ConsolString(string init_date)
    {
        this._totalString = init_date;
    }

    public string totalString
    {
        get { return this._totalString; }
        set
        {
            this._totalString = value;
            this.OnPropertyChanged();
        }
    }

    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        // Raise the PropertyChanged event, passing the name of the property whose value has changed.
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
public sealed partial class HomeLandingPage : Page
{
    public string AppInfo { get; set; }
    //public HomeLandingViewModel ViewModel { get; }

    ConsolString total_str = new("none");



    public MainPage_Singleton mainPage_Singleton = MainPage_Singleton.GetInstance();

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


    public void TextAddLine(string str)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            total_str.totalString += str;
            //dialogTextBox.AddLine(dialogTextBox.CurrentLineIndex, str);
            //dialogTextBox.SetLineText(dialogTextBox.CurrentLineIndex, str);
            //dialogTextBox.ScrollBottomIntoView();

            //dialogTextBox.Text = total_str.totalString;
            //dialogTextBox.LoadText(total_str.totalString);

            str = null;
            //scrollview1.ScrollTo(scrollview1.ActualHeight + scrollview1.VerticalOffset,0);

            //dialogTextBox.ScrollLineIntoView(dialogTextBox.NumberOfLines);
        });
        
        //GC.Collect();
    }

    public HomeLandingViewModel ViewModel { get; }

    public HomeLandingPage()
    {
        IO_Manage.Instance.page = this;
        List<SerialDevice> serialDevices = new();
        ViewModel = App.GetService<HomeLandingViewModel>();

        this.InitializeComponent();

        //注册回调函数
        IO_Manage.updateDevices += Instance_update;
        IO_Manage.updateCannelViewMsg += UpdateCannelViewMsg;


       // IO_Manage.Instance.updateSerialDevce();

        //ListView_LinkDevice.ItemsSource = ViewModel.advancedCollectionView;

        //ViewModel.advancedCollectionView.SortDescriptions.Add(new SortDescription("DataName", SortDirection.Descending));
        //ViewModel.advancedCollectionView.Add(new CannelDataView("data", -0, "9"));
        //ViewModel.dateSource.Add(new CannelDataView("data1", -0, "9"));
        //AppInfo = $"{App.Current.AppName} v{App.Current.AppVersion}";


        //Use a builtin language -> see list a bit higher
        //dialogTextBox.CodeLanguage = TextControlBox.GetCodeLanguageFromId(CodeLanguageId.CSharp);

        //Use a custom language:
        // 假设你的JSON文件名为"data.json"，位于与你的程序相同的目录下
        //string filePath = "C:\\Users\\80520\\source\\repos\\ACOM\\ACOM\\Properties\\HighLightSettinng.json";
        //string jsonContent = null;
        //// 检查文件是否存在
        //if (File.Exists(filePath))
        //{
        //    // 读取文件内容到字符串
        //    jsonContent = File.ReadAllText(filePath);

        //    // 打印读取的内容
        //    Console.WriteLine(jsonContent);
        //}
        //else
        //{
        //    while (true) ;
        //}



        //dialogTextBox.Text = File.ReadAllText("C:\\Users\\80520\\Downloads\\ttt.txt");
        //var result = TextControlBox.GetCodeLanguageFromJson(jsonContent);
        //if (result.Succeed)
        //    dialogTextBox.CodeLanguage = result.CodeLanguage;
        //else {
        //    while (true) ;
        //}

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
                frame.Navigate(typeof(CanvasPanelPage));
                break;
            case 1:
                frame.Navigate(typeof(CanvasPanelPage));
                break;
            case 2:
                frame.Navigate(typeof(CanvasPanelPage));
                break;
        }

        newItem.Content = frame;

        return newItem;
    }

    private void HoverButton_Click(object sender, RoutedEventArgs e)
    {
        AppBarButton? button = sender as AppBarButton;
        Grid parent = button.FindParent<Grid>();
        foreach (var item in ViewModel.dateSource)
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
            ViewModel.dateSource[Convert.ToInt32(str)].DataName = textBox.Text;
        }
    }

    private void myColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        var str = sender.Tag as string;
        ViewModel.dateSource[Convert.ToInt32(str)].DataColor.Color = sender.Color;
        //dateSource[Convert.ToInt32(str)].DataColor = new SolidColorBrush(sender.Color);
    }

    private void AppBarButton_Click_ADD(object sender, RoutedEventArgs e)
    {
    }

    private void ComboBox_DropDownOpened(object sender, object e)
    {
        Debug.WriteLine("start to scanf serial");
        //string[] _SerialPortsSource = System.IO.Ports.SerialPort.GetPortNames();
        //ViewModel.SerialPortsSource.Clear();
        //foreach (string port in _SerialPortsSource)
        //{
        //    Debug.WriteLine("serial com" + port);
        //    ViewModel.SerialPortsSource.Add(port);
        //}
    }

    private void Instance_update(List<SerialDevice> serialDevices)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            Debug.WriteLine("update serial1");
            Debug.WriteLine("update serial2");

            foreach (var device in serialDevices)
            {
                if (!ViewModel.SerialPortsSource.Contains(device.PortName))
                {
                    ViewModel.SerialPortsSource.Add(device.PortName);
                    ViewModel.linkDeviceSource.Add(new LinkDeviceDates(device.PortName, device.FriendlyName));
                }
            }

            // Remove ports that are no longer available
            for (int i = ViewModel.SerialPortsSource.Count - 1; i >= 0; i--)
            {
                if (!serialDevices.Any(d => d.PortName == ViewModel.SerialPortsSource[i]))
                {
                    ViewModel.SerialPortsSource.RemoveAt(i);
                    ViewModel.linkDeviceSource.RemoveAt(i);
                }
            }
        });
    }

    private void UpdateCannelViewMsg(List<ChannelViewData> globChannelViewData)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            foreach (ChannelViewData it in globChannelViewData)
            {
                var existingItem = ViewModel.dateSource.FirstOrDefault(itt => itt.DataName == it.Name);
                if (existingItem != null)
                {
                    existingItem.Data = it.ChannelData;
                    // Update other properties as needed
                }
                else
                {
                    ViewModel.dateSource.Add(new CannelDataView(it.Name, it.ChannelData, "9"));
                }
            }
        });
    }


    //private void combox_COM_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //{
    //    // 获取发送事件的 ComboBox
    //    var comboBox = sender as Microsoft.UI.Xaml.Controls.ComboBox;

    //    if (comboBox != null)
    //    {
    //        if (comboBox.SelectedItem != null)
    //        {
    //            foreach (LinkDeviceDates dev in ViewModel.linkDeviceSource)
    //            {
    //                if (dev.DeviceName.Equals(comboBox.SelectedItem.ToString()))
    //                {
    //                    LinkSerial_Boundrate.SelectedValue = dev.BoundRate.ToString();
    //                    LinkSerial_DataLength.SelectedValue = dev.DateBit;
    //                    LinkSerial_StopBit.SelectedValue = dev.StopBit;
    //                    LinkSerial_StreamCtrl.SelectedValue = dev.StreamCtrl;
    //                }
    //            }
    //        }
    //        // 获取当前选中项
    //        var selectedItem = comboBox.SelectedItem;
    //        foreach (var port in PortsDesc)
    //        {
    //            Debug.WriteLine(port);
    //            if (selectedItem != null)
    //            {
    //                if (port.Contains(selectedItem.ToString()))
    //                {
    //                    TextBlockCOM_Desc.Text = port;
    //                    Console.WriteLine($"Changed selection to: {selectedItem}");
    //                    return;
    //                }
    //            }
    //        }
    //    }
    //}

    private void combox_COM_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
    {
        // 获取发送事件的 ComboBox
        var comboBox = sender as Microsoft.UI.Xaml.Controls.ComboBox;

        if (comboBox != null)
        {
            // 获取当前选中项
            var selectedItem = comboBox.SelectedItem;
            //PortsDesc = SerialPortFindTool.GetSerialPort();


            //IO_Manage.Instance.GetFriendlyName(selectedItem.ToString());

            //foreach (var port in PortsDesc)
            //{
            //    Debug.WriteLine(port);
            //    if (selectedItem != null)
            //    {
            //        if (port.Contains(selectedItem.ToString()))
            //        {
            //            TextBlockCOM_Desc.Text = port;
            //            Console.WriteLine($"Changed selection to: {selectedItem}");
            //            return;
            //        }
            //    }
            //}
            // 执行相应操作,修改text,还有其他属性
            //TextBlockCOM_Desc.Text = IO_Manage.Instance.GetFriendlyName(selectedItem.ToString());
        }
    }

    //private void LinkSerial_COM_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //{
    //    Debug.Write("dei");
    //    if (LinkSerial_Boundrate != null && LinkSerial_DataLength != null &&
    //        LinkSerial_StopBit != null && LinkSerial_StreamCtrl != null &&
    //        LinkSerial_Boundrate.SelectedValue != null && LinkSerial_DataLength.SelectedValue != null &&
    //        LinkSerial_StopBit.SelectedValue != null && LinkSerial_StreamCtrl.SelectedValue != null && combox_COM.SelectedItem != null
    //        )
    //    {
    //        var portName = combox_COM.SelectedItem.ToString();
    //        //if()
    //        foreach (LinkDeviceDates dev in ViewModel.linkDeviceSource)
    //        {
    //            if (dev.DeviceName.Equals(portName))
    //            {
    //                Debug.WriteLine(LinkSerial_Boundrate.SelectedValue.ToString());

    //                TextBlockCOM_Desc.Text = dev.DeviceDesc;

    //                dev.BoundRate = Convert.ToInt32(LinkSerial_Boundrate.SelectedValue.ToString());
    //                dev.DateBit = (int)LinkSerial_DataLength.SelectedValue;
    //                dev.StopBit = (string)LinkSerial_StopBit.SelectedValue;
    //                dev.StreamCtrl = (string)LinkSerial_StreamCtrl.SelectedValue;
    //                if (LinkSerial_CheckBit.SelectedIndex == 0)
    //                {
    //                    dev.CheckBit = "N";
    //                }
    //                else if (LinkSerial_CheckBit.SelectedIndex == 1)
    //                {
    //                    dev.CheckBit = "O";
    //                }
    //                else if (LinkSerial_CheckBit.SelectedIndex == 2)
    //                {
    //                    dev.CheckBit = "D";
    //                }
    //                else
    //                {
    //                    dev.CheckBit = " ";
    //                }
    //            }
    //        }
    //    }

    //}

    //private void ConnectButton_Click(object sender, RoutedEventArgs e)
    //{
    //    if (LinkSerial_Boundrate != null && LinkSerial_DataLength != null &&
    //        LinkSerial_StopBit != null && LinkSerial_StreamCtrl != null &&
    //        LinkSerial_Boundrate.SelectedValue != null && LinkSerial_DataLength.SelectedValue != null &&
    //        LinkSerial_StopBit.SelectedValue != null && LinkSerial_StreamCtrl.SelectedValue != null
    //        )
    //    {
    //        if (combox_COM.SelectedItem != null)
    //        {
    //            foreach (LinkDeviceDates dev in ViewModel.linkDeviceSource)
    //            {
    //                if (dev.DeviceName.Equals(combox_COM.SelectedItem.ToString()))
    //                {
    //                    if (ConnectButton.IsChecked == false)
    //                    {
    //                        dev.DisConnect();
    //                        return;
    //                    }
    //                    else
    //                    {
    //                        //TODO BUG 应该有重复的项目导致会有异常
    //                        dev.Connect();
    //                        return;
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            // ConnectButton. = false;  
    //        }

    //    }
    //    else
    //    {
    //        // ConnectButton.Checked = false;
    //    }

    //}

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

