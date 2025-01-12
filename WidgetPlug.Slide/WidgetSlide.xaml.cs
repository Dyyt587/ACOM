using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using CustomExtensions.WinUI;
using Microsoft.UI;
using Windows.Devices.Input;
using ACOMPlugin.Core;
using Windows.Globalization.NumberFormatting;
using ACOMCommmon;
using DryIoc.ImTools;
using System.Diagnostics;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WidgetPlug.Slide;
public sealed partial class WidgetSlide : UserControl
{
    public Slide slide;

 
    public ObservableCollection<CannelData> cannel_data = new(); //数据颜色
    private List<int> cannel_index = new();
    IncrementNumberRounder rounder = new IncrementNumberRounder();


    public DecimalFormatter formatter = new DecimalFormatter();

    private void ShowMenu(UIElement sender, bool isTransient)
    {
        FlyoutShowOptions myOption = new FlyoutShowOptions();
        //myOption.ShowMode = isTransient ? FlyoutShowMode.Transient : FlyoutShowMode.Standard;
        PartsCommandBarFlyout.ShowAt(sender, myOption);
        

    }
    private void Parts_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
    {
        //Debug.WriteLine("test Parts_ContextRequested");
        ShowMenu(sender, true);

    }
    public WidgetSlide()
    {
        rounder.Increment = 0.000001;
        rounder.RoundingAlgorithm = RoundingAlgorithm.RoundDown;

        formatter.IntegerDigits = 1;
        formatter.FractionDigits = 7;
        formatter.NumberRounder = rounder;

        this.LoadComponent(ref _contentLoaded);
    }
    public void updateData(List<CannelData> newData)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            foreach (var newDataItem in newData)
            {
                var existingDataItem = cannel_data.FirstOrDefault(cd => cd.DataName == newDataItem.DataName);
                if (existingDataItem != null)
                {
                    existingDataItem.Data = newDataItem.Data;
                    existingDataItem.DateTime = newDataItem.DateTime;
                    //existingDataItem.DataColor = newDataItem.DataColor;
                    existingDataItem.is_View = newDataItem.is_View;
                }
                else
                {
                    cannel_data.Add(newDataItem);
                }
            }
            if(cannel_index.Count !=0)
            MainSlider.Value =  Convert.ToDouble(cannel_data[cannel_index[cannel_index.Count-1]].Data);

        });
    }
    private void Slider_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void OptionsAllCheckBox1_Indeterminate(object sender, RoutedEventArgs e)
    {

    }

    private void OptionsAllCheckBox_Indeterminate(object sender, RoutedEventArgs e)
    {

    }

    private void WidgetDeleteButton_Click(object sender, RoutedEventArgs e)
    {
        List<SystemCommandEnum> cmd = new List<SystemCommandEnum>();
        cmd.Add(SystemCommandEnum.Delete);
        slide.OnSystemCommand(this, cmd);
    }

    private void OptionCheckBoxHandle(object sender, RoutedEventArgs e)
    {
        if(sender is CheckBox cp)
        {

            var existingDataItem = cannel_data.FirstOrDefault(cd => cd.DataName == (cp.Content as string));
            if (existingDataItem != null)
            {
                if ((bool)cp.IsChecked)
                {
                    cannel_index.Add(cannel_data.IndexOf(existingDataItem));
                }
                else
                {
                    cannel_index.Remove(cannel_data.IndexOf(existingDataItem));

                }
            }

        }
 
 
    }
}
