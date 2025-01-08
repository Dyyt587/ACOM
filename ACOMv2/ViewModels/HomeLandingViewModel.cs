using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Windows.Input;
using ACOM.Models;
using System.Diagnostics;
using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.Collections;
 using Microsoft.UI.Dispatching;
using Windows.Devices.SerialCommunication;
using SerialDevice = ACOM.Models.SerialDevice;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using static ACOMCommmon.RawDataMassage;
using Microsoft.VisualBasic;
using static ACOM.Models.IO_Manage;
using ACOMv2.Persistence;

namespace ACOMv2.ViewModels;


public class LoadStat : ObservableObject
{
    double _load;
    public double Load
    {
        get => _load;
        set => SetProperty(ref _load, value);
    }
    DateTime time;
    public DateTime Time
    {
        get => time;
        set => SetProperty(ref time, value);
    }
    string _StrTime;
    public string StrTime
    {
        get => _StrTime;
        set => SetProperty(ref _StrTime, value);
    }
}

public partial class HomeLandingViewModel : ObservableObject
{
    public IJsonNavigationViewService JsonNavigationViewService;
    public CannelData dataListDatas;

    public ObservableCollection<CannelData> dateSource = new(); //数据颜色

    public ObservableCollection<LoadStat> loadStatSource = new(); //数据负载率

    //public ObservableCollection<SerialDevices> serialDevices = new(); //可以连接的串口设备

    public ObservableCollection<string> SerialPortsSource = new(); //连接设备
    public ObservableCollection<string> SerialPortsFriendlyLinkedSource = new(); //连接设备

    public int SelectedLinkedSendSerialIndex = 0; 
    public int ConfigingSerialDeviceIndex = 0; //正在配置的串口设备


    public AdvancedCollectionView advancedCollectionView ;
    public List<ACOM.Models.SerialDevice> Devices= new();// = serialDevices;


    public List<Page> CanvasPages = new();

    public void GetPlugFromLable(string lable)
    {
        //var item = new DataItem(Guid.NewGuid().ToString(), lable, typeof(HomeLandingViewModel).FullName, new object());
        //JsonNavigationViewService.NavigateTo(item.UniqueId + item.Parameter?.ToString(), item);
    }

    public HomeLandingViewModel(IJsonNavigationViewService jsonNavigationViewService)
    {
        JsonNavigationViewService = jsonNavigationViewService;

        //for (int i = 0; i < 20; i++)
        //{
        //    dateSource.Add(new CannelData("data" + i.ToString(), -1.2198256, i.ToString()));
        //    dateSource[i].DataColor.Color = ACOMv2.Common.ColorHelper.GenerateDistinctColor((i * 78) % 128);
        //}
        dateSource.EnableSyncWithDictionary(x => ((CannelData)x).DataName);
        advancedCollectionView = new AdvancedCollectionView(dateSource, true);
        // Let's filter out the integers
        int nul;
        advancedCollectionView.Filter = x => !int.TryParse(((CannelData)x).DataName, out nul);

        

        // And sort ascending by the property "Name"
        advancedCollectionView.SortDescriptions.Add(new SortDescription("DataName", SortDirection.Descending));
        //serialDevices.Add(new SerialDevices("COM1"));



    }

    [RelayCommand]
    private void OnItemClick(RoutedEventArgs e)
    {
        var args = (ItemClickEventArgs)e;
        var item = (DataItem)args.ClickedItem;

        JsonNavigationViewService.NavigateTo(item.UniqueId + item.Parameter?.ToString(), item);
        //HomeLandingViewService.TextAddLine("wd");

    }
    public void Send(byte[] bytes)
    {
        SelectedLinkedSendSerialIndex = SelectedLinkedSendSerialIndex >= SerialPortsFriendlyLinkedSource.Count ? SerialPortsFriendlyLinkedSource.Count-1 : SelectedLinkedSendSerialIndex;
        IO_Manage.Instance.SerialSend(SerialPortsFriendlyLinkedSource[SelectedLinkedSendSerialIndex], bytes);
     }

}

public static class ObservableCollectionExtensions
{
    private static Dictionary<object, object> _syncedDictionaries = new();

    public static void EnableSyncWithDictionary<T>(
       this ObservableCollection<T> collection,
       Func<T, object> keySelector,
       bool autoSync = true)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));

        //Type type = typeof(T);

        //if (!_syncedDictionaries.ContainsKey(type))
        //{
        //    _syncedDictionaries[type] = new Dictionary<object, object>();
        //}

        foreach (var item in collection)
        {
            var key = keySelector(item);
            //_syncedDictionaries[type][key] = item;
            _syncedDictionaries.Add(key, item);
        }

        if (autoSync)
        {
            collection.CollectionChanged += (sender, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (T item in e.NewItems)
                        {
                            var key = keySelector(item);
                            //_syncedDictionaries.Add(key, item);
                            _syncedDictionaries.Add(key, item);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (T item in e.OldItems)
                        {
                            var key = keySelector(item);
                            _syncedDictionaries.Remove(key);
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        foreach (T oldItem in e.OldItems)
                        {
                            var oldKey = keySelector(oldItem);
                            _syncedDictionaries.Remove(oldKey);
                        }
                        foreach (T newItem in e.NewItems)
                        {
                            var newKey = keySelector(newItem);
                            _syncedDictionaries[newKey] = newItem;
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        // No action needed for move
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        _syncedDictionaries.Clear();
                        foreach (var item in collection)
                        {
                            var key = keySelector(item);
                            _syncedDictionaries[key] = item;
                        }
                        break;
                }
            };
        }
    }

    public static void DisableSyncWithDictionary<T>(
        this ObservableCollection<T> collection)
    {
        collection.CollectionChanged -= (sender, e) =>
        {
            // Handle the collection changed event
        };
        // Clear the dictionary for the type
        //_syncedDictionaries.Remove(typeof(T));
        _syncedDictionaries.Clear();
    }
    public static T FindByProperty<T>(
        this ObservableCollection<T> collection,
        object key,
        Func<T, object> keySelector)
    {

        if (_syncedDictionaries.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        return default(T);

    }
}

