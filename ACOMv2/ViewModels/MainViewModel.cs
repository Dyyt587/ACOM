using CommunityToolkit.WinUI.Collections;
using System.Collections.ObjectModel;

namespace ACOMv2.ViewModels;
public partial class MainViewModel : ObservableObject, ITitleBarAutoSuggestBoxAware
{
    public CannelData dataListDatas;

    public ObservableCollection<CannelData> dateSource = new(); //数据颜色

    public ObservableCollection<LoadStat> loadStatSource = new(); //数据负载率

    public ObservableCollection<SerialDevices> serialDevices = new(); //可以连接的串口设备
    public ObservableCollection<string> SerialPortsSource = new(); //连接设备
    //public ObservableCollection<string> SerialPortsFriendlyLinkedSource = new(); //连接设备


    //public int SelectedLinkedSendSerialIndex = 0;
    //public int ConfigingSerialDeviceIndex = 0; //正在配置的串口设备

    public AdvancedCollectionView advancedCollectionView;
    public List<ACOM.Models.SerialDevice> Devices = new();// = serialDevices;


    public List<Page> CanvasPages = new();


    public MainViewModel()
    {
     }

    public void OnAutoSuggestBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {

    }

    public void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {

    }
}
