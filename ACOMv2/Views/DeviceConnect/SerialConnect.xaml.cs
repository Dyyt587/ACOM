using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Text;
using ACOM.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ACOMv2.Views.DeviceConnect
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SerialConnect : Page
    {
        private MainViewModel ViewModel;
        private HomeLandingViewModel homeLandingViewModel;

        private string ContentRequestItemName;
        string[] PortsDesc = { "NULL" };
        IO_Manage iomanage = IO_Manage.Instance;

        private void Instance_update(List<SerialDevice> serialDevices)
        {
            try
            {
                //防止退出时候出现异常
                if(DispatcherQueue !=null)
                DispatcherQueue.TryEnqueue(() =>
                {

                    foreach (var device in serialDevices)
                    {
                        if (!ViewModel.SerialPortsSource.Contains(device.PortName))
                        {
                            ViewModel.SerialPortsSource.Add(device.PortName);
                            ViewModel.serialDevices.Add(new SerialDevices(device.PortName, device.FriendlyName));
                        }
                    }

                    // Remove ports that are no longer available
                    for (int i = ViewModel.SerialPortsSource.Count - 1; i >= 0; i--)
                    {
                        if (!serialDevices.Any(d => d.PortName == ViewModel.SerialPortsSource[i]))
                        {
                            ViewModel.SerialPortsSource.RemoveAt(i);
                            ViewModel.serialDevices.RemoveAt(i);
                        }
                    }
                });

            }
            catch (Exception e)
            {
                Debug.WriteLine("Instance_update DispatcherQueue error " + e.Message);
            }

        }
        public SerialConnect()
        {
            ViewModel = App.GetService<MainViewModel>();
            homeLandingViewModel = App.GetService<HomeLandingViewModel>();

            this.InitializeComponent();


            iomanage.updateDevices += Instance_update;
            iomanage.updateSerialDevce();
        }
















        private void ShowMenu(UIElement sender, bool isTransient, object Tag)
        {
            FlyoutShowOptions myOption = new FlyoutShowOptions();
            myOption.ShowMode = isTransient ? FlyoutShowMode.Transient : FlyoutShowMode.Standard;
            //CommandBarFlyout1.ShowAt(sender, myOption);
        }
        private void DataList_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            Grid grid = sender as Grid;
            if (grid != null)
            {
                //mainPage_Singleton.ContentRequestItemName = grid.Tag as string;
                ContentRequestItemName = grid.Tag as string;
                Debug.WriteLine(ContentRequestItemName);
                ShowMenu(sender, true, grid.Tag);
            }
        }
 







        private void DataList_ContextCanceled(UIElement sender, RoutedEventArgs e)
        {
            // Show a context menu in transient mode
            // Focus will not move to the menu
            // ShowMenu(sender, true);
        }






        private void combox_COM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 获取发送事件的 ComboBox
            var comboBox = sender as Microsoft.UI.Xaml.Controls.ComboBox;

            if (comboBox != null)
            {
                if (comboBox.SelectedItem != null)
                {
                    foreach (SerialDevices dev in ViewModel.serialDevices)
                    {
                        if (dev.DeviceName.Equals(comboBox.SelectedItem.ToString()))
                        {
                            LinkSerial_Boundrate.SelectedValue = dev.BoundRate.ToString();
                            LinkSerial_DataLength.SelectedValue = dev.DateBit.ToString();
                            LinkSerial_StopBit.SelectedValue = dev.StopBit;
                            LinkSerial_StreamCtrl.SelectedValue = dev.StreamCtrl;


                            if (dev.ConnectState)
                            {
 
                                ConnectButton.IsChecked = true;

                            }
                            else
                            {
                                ConnectButton.IsChecked = false;

                            }
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
                TextBlockCOM_Desc.Text = IO_Manage.Instance.GetFriendlyName(selectedItem.ToString());
            }
        }

        private void ConfingSerialDeviceChanged(object sender, SelectionChangedEventArgs e)
        {

            if (LinkSerial_Boundrate != null && LinkSerial_DataLength != null &&
                LinkSerial_StopBit != null && LinkSerial_StreamCtrl != null &&
                LinkSerial_Boundrate.SelectedValue != null && LinkSerial_DataLength.SelectedValue != null &&
                LinkSerial_StopBit.SelectedValue != null && LinkSerial_StreamCtrl.SelectedValue != null && combox_COM.SelectedItem != null
                )
            {
                var portName = combox_COM.SelectedItem.ToString();
                foreach (SerialDevices dev in ViewModel.serialDevices)
                {
                    if (dev.DeviceName.Equals(portName))
                    {
                        Debug.WriteLine(LinkSerial_Boundrate.SelectedValue.ToString());

                        TextBlockCOM_Desc.Text = dev.DeviceDesc;

                        dev.BoundRate = Convert.ToInt32(LinkSerial_Boundrate.SelectedValue.ToString());
                        dev.DateBit = Convert.ToInt32(LinkSerial_DataLength.SelectedValue);
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








 
        private void ComboBox_DropDownOpened(object sender, object e)
        {
        }
 


        private void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton button = sender as AppBarButton;
            var barbutton = sender as AppBarButton;

            if (barbutton != null)
            {
                string DeviceName = barbutton.Tag as string;
                foreach (SerialDevices dev in ViewModel.serialDevices)
                {
                    if (dev.DeviceName.Equals(DeviceName))
                    {
                        if (dev.ConnectState)
                        {
                            if (dev.DisConnect())
                            {
                                combox_COM.SelectedValue = dev.DeviceName;
                                ConnectButton.IsChecked = false;
                                homeLandingViewModel.SerialPortsFriendlyLinkedSource.Remove(dev.DeviceName);

                            }
                            else
                            {
                                Logger.Error("disconnect" + DeviceName + " error ");
                            }

                            return;
                        }
                        else
                        {
                            //TODO BUG 应该有重复的项目导致会有异常
                            if (dev.Connect())
                            {
                                combox_COM.SelectedValue = dev.DeviceName;
                                ConnectButton.IsChecked = true;
                                homeLandingViewModel.SerialPortsFriendlyLinkedSource.Add(dev.DeviceName);

                            }
                            else
                            {
                                Logger.Error("connect" + DeviceName + " error ");
                            }
                            return;
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
                LinkSerial_StopBit.SelectedValue != null && LinkSerial_StreamCtrl.SelectedValue != null)
            {
                if (combox_COM.SelectedItem != null)
                {
                    foreach (SerialDevices dev in ViewModel.serialDevices)
                    {
                        if (dev.DeviceName.Equals(combox_COM.SelectedItem.ToString()))
                        {
                            if (dev.ConnectState)
                            {
                                if (dev.DisConnect())
                                {
                                    ConnectButton.IsChecked = false;
                                    homeLandingViewModel.SerialPortsFriendlyLinkedSource.Remove(dev.DeviceName);

                                }
                                else
                                {
                                    Logger.Error("disconnect" + dev.DeviceName + " error ");
                                }

                                return;
                            }
                            else
                            {
                                //TODO BUG 应该有重复的项目导致会有异常
                                if (dev.Connect())
                                {
                                    ConnectButton.IsChecked = true;
                                    homeLandingViewModel.SerialPortsFriendlyLinkedSource.Add(dev.DeviceName);
                                }
                                else
                                {
                                    Logger.Error("connect" + dev.DeviceName + " error ");
                                }
                                return;
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

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
        }


    }
}
