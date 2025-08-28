using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ACOMPlugin.Core;
using ACOMv2.Models.Processers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ACOMv2.Views.DeviceConnect
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Parts : Page
    {
        public ObservableCollection<FrameworkElement> PluginElements { get; } = new();

        public Parts()
        {
            this.InitializeComponent();
            LoadPluginElements();
        }

        private void LoadPluginElements()
        {
            PluginElements.Clear();
            foreach (var plugin in Plugs.WidgetPlugins.Values)
            {
                var element = plugin.Create();
                if (element != null)
                {
                    element.Tag = plugin.Id;
                    PluginElements.Add(element);

                }
            }
        }

        private void PluginButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id)
            {
                // 这里可以根据id找到插件并执行操作
                // 例如：显示消息、调用插件方法等
                var plugin = Plugs.WidgetPlugins.TryGetValue(id, out var p) ? p : null;
                if (plugin != null)
                {
                    // 示例：弹窗显示插件名
                    var dialog = new ContentDialog
                    {
                        Title = "插件信息",
                        Content = $"插件名: {plugin.GetName()}",
                        CloseButtonText = "关闭",
                        XamlRoot = this.XamlRoot
                    };
                    _ = dialog.ShowAsync();
                }
            }
        }
        public T ConvertTabViewItemToPage<T>(object obj)
        {
            var ttt = obj as TabViewItem;
            var tt = ttt.Content as Frame;
            return (T)tt.Content;
        }
        private void PluginItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is FrameworkElement element)
            {
                ACOMPluginBase plug;
                var ele = element as ContentPresenter;
               
                Debug.WriteLine("dd" + (ele.Content as FrameworkElement).Tag as string);
                Plugs.WidgetPlugins.TryGetValue((ele.Content as FrameworkElement).Tag as string, out plug);
                if (plug != null && ACOMv2.Views.HomeLandingPage.Instance != null)
                {
                    ACOMv2.Views.HomeLandingPage.Instance.CreateWidgetOnCurrentTab(plug);
                }
                else
                {
                    Debug.WriteLine("Widget not found or HomeLandingPage.Instance is null");
                }
            }
        }

        // 拖动开始
        private void PluginItem_DragStarting(object sender, DragStartingEventArgs e)
        {
            if (sender is Button btn)
            {
                Debug.WriteLine("start drag ");
                // 你可以放入自定义数据，这里用按钮内容
                e.Data.SetText(btn.Content?.ToString() ?? "插件");
                // 可选：设置拖动时的视觉反馈
                e.Data.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
            }
        }

        // 拖动经过
        private void PluginItem_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
        }

        // 放下
        private void PluginItem_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Text))
            {
                // 获取拖放的数据
                var def = e.GetDeferral();
             //   _ = HandleDropAsync(sender, e, def);
            }
        }

        private async System.Threading.Tasks.Task HandleDropAsync(object sender, DragEventArgs e, Deferral def)
        {
            string text = await e.DataView.GetTextAsync();
            // 这里你可以根据 text 或 sender 进行业务处理
            // 例如：交换两个插件的位置、添加到新位置等
            def.Complete();
        }

        private void Button_DragEnter(object sender, DragEventArgs e)
        {
            Debug.WriteLine("start drag ");

        }
    }

    public class PluginViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}

