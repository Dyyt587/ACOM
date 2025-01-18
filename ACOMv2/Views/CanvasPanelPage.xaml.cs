using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using CommunityToolkit.Labs.WinUI;
using Windows.UI.Core;
using Microsoft.UI.Input;
using System.Reflection;
using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.Controls;
using ShadowPluginLoader.WinUI;
using Windows.Devices.Enumeration;
using ACOMPlugin.Core;
using DryIoc;
using ACOM.Models;
using ACOMPlug;
using ACOMv2.Models.Processers;
using static ACOMPlugin.Core.ACOMPluginBase;
 // To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ACOMv2.Views
{
    public static class UIElementExtensions
    {
        //private static CoreCursor defaultCursor;

        public static void ChangeCursor(this UIElement uiElement, InputCursor cursor)
        {
            Type type = typeof(UIElement);
            type.InvokeMember("ProtectedCursor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, uiElement, new object[] { cursor });
        }
    }
    public sealed partial class CanvasPanelPage : Page
    {

        IO_Manage iomanage = IO_Manage.Instance;
        public FrameworkElement InitializeElementGrid(FrameworkElement element,double initWidth=180,double initHeight=50,double offset=30)
        {

            // 创建Grid
            Grid elementGrid = new Grid()
            {
                Name = element.Name + "Grid",
                MinHeight = initHeight,
                MinWidth = initWidth,
                Style = (Style)Application.Current.Resources["GridCardPanel"]
            };
            //创建组件
            Widget part = new(ref element);

            double newX = 30;
            double newY = 30;
             // 设置Canvas位置（在C#中，我们通常不设置Canvas.Left和Canvas.Top，因为它们是Canvas特有的属性，这里假设你想要将Grid放置在Canvas中）
            // 你需要一个Canvas作为父容器，然后添加Grid到其中，并设置位置
            AGAIN:
            // 遍历Canvas中的所有子控件
            foreach (var child in CanvasView1.Items)
            {
                // 获取子控件的位置
                double childLeft = Canvas.GetLeft((UIElement)child);
                double childTop = Canvas.GetTop((UIElement)child);

                // 检查新控件的位置是否与现有控件重叠
                if (childLeft == newX && childTop == newY)
                {
                    // 如果重叠，调整新控件的位置
                    newX += offset; // 向下偏移
                    newY += offset; // 向下偏移
                    goto AGAIN; // 退出循环，因为我们只需要检查第一个重叠的控件
                }
            }

            // 设置新控件的位置
            Canvas.SetLeft(elementGrid, newX);
            Canvas.SetTop(elementGrid, newY);

 

            // 定义列
            ColumnDefinition columnDef1 = new ColumnDefinition();
            ColumnDefinition columnDef2 = new ColumnDefinition() { Width = GridLength.Auto };
            elementGrid.ColumnDefinitions.Add(columnDef1);
            elementGrid.ColumnDefinitions.Add(columnDef2);

            // 定义行
            RowDefinition rowDef1 = new RowDefinition();
            RowDefinition rowDef2 = new RowDefinition() { Height = GridLength.Auto };
            elementGrid.RowDefinitions.Add(rowDef1);
            elementGrid.RowDefinitions.Add(rowDef2);

            Grid.SetColumn(element, 0);
            Grid.SetRow(element, 0);
            elementGrid.Children.Add(element);

            // 创建水平ContentSizer
            ContentSizer horizontalContentSizer = new ContentSizer()
            {
                Height = 2,
                VerticalAlignment = VerticalAlignment.Bottom,
                Orientation = Orientation.Horizontal,
                TargetControl = elementGrid,
                Visibility = Visibility.Visible
            };
            horizontalContentSizer.ManipulationDelta += ContentSizer_ManipulationDelta;

            Grid.SetRow(horizontalContentSizer, 1);

            elementGrid.Children.Add(horizontalContentSizer);

            // 创建垂直ContentSizer
            ContentSizer verticalContentSizer = new ContentSizer()
            {
                Width = 2,
                HorizontalAlignment = HorizontalAlignment.Right,
                TargetControl = elementGrid,
                Visibility = Visibility.Visible
            };

            Grid.SetColumn(verticalContentSizer, 1);

            verticalContentSizer.ManipulationDelta += ContentSizer_ManipulationDelta;

            elementGrid.Children.Add(verticalContentSizer);
            element.KeyDown += KeyControl;
             elementGrid.KeyDown += KeyControl;
            // 将Grid添加到页面或其他父容器中
            return elementGrid; // 假设你是在Page中，并且将Grid设置为页面的内容
        }

        public static void RemoveControlAndParentsFromCanvas(FrameworkElement control)
        {
            // 找到控件所在的Canvas
            Canvas canvas = FindParent<Canvas>(control);
            if (canvas == null) return;

            // 创建一个栈来存储需要移除的控件及其父控件
            Stack<FrameworkElement> controlsToRemove = new ();
            controlsToRemove.Push(control);

            // 递归地将控件及其所有父控件加入栈中
            while ((control = (FrameworkElement)VisualTreeHelper.GetParent(control)) != null && control != canvas)
            {
                controlsToRemove.Push(control);
            }

            // 从栈中弹出控件并从Canvas中移除
            while (controlsToRemove.Count > 0)
            {
                control = controlsToRemove.Pop();
                if (canvas.Children.Contains(control))
                {
                    canvas.Children.Remove(control);
                }
            }
        }

        public static T FindParent<T>(FrameworkElement child) where T : FrameworkElement
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            while (parentObject != null)
            {
                if (parentObject is T parent)
                {
                    return parent;
                }
                parentObject = VisualTreeHelper.GetParent(parentObject);
            }
            return null;
        }

        private void KeyControl(object sender, KeyRoutedEventArgs e)
        {
            // 检查按下的键是否是Delete键
            if (e.Key == Windows.System.VirtualKey.Delete)
            {
                RemoveControlAndParentsFromCanvas(sender as FrameworkElement);
                //CanvasView1.Items.Remove(sender);
            }
            e.Handled = true;
        }

        public void CreateWidget(ACOMPluginBase WidgetPlug)
        {
            if (WidgetPlug == null) return;
            WidgetPlug.UpdateCommand += UpdateWidgetCommend;
            WidgetPlug.SystemCommand += SystemCommand;
            iomanage.updateCannelViewMsg += WidgetPlug.UpdateData;
            CanvasView1.Items.Add(InitializeElementGrid(WidgetPlug.Create()));
        }

        private void SystemCommand(object sender, List<SystemCommandEnum> cmd)
        {
            foreach (var command in cmd)
            {
                switch (command)
                {
                    case SystemCommandEnum.Delete:
                        // 执行删除操作
                        DeleteOperation(sender);
                        break;
                    case SystemCommandEnum.Copy:
                        // 执行复制操作
                        CopyOperation(sender);
                        break;
                    case SystemCommandEnum.Paste:
                        // 执行粘贴操作
                        PasteOperation(sender);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void DeleteOperation(object sender)
        {
            if (sender is FrameworkElement cp)
            {
                RemoveControlAndParentsFromCanvas(cp);
            }
        }

        private void CopyOperation(object sender)
        {
            // 复制操作的具体实现
        }

        private void PasteOperation(object sender)
        {
            // 粘贴操作的具体实现
        }

        private void UpdateWidgetCommend(object sender, List<string[]> cmd)
        {
            throw new NotImplementedException();
        }

        public CanvasPanelPage()
        {
            ViewModel = App.GetService<HomeLandingViewModel>();

            this.Loaded += (sender, e) =>
            {
                ViewModel.CanvasPages.Add(this);
            };
            this.Unloaded += (sender, e) =>
            {
                ViewModel.CanvasPages.Remove(this);
            };

            this.InitializeComponent();
            ACOMPluginBase var;
            Plugs.WidgetPlugins.TryGetValue("ACOMPlug.Widget.Slide",out var);
            if(var !=null)
            {
                //CreateWidget(var);
                //CreateWidget(Plugs.WidgetPlugins["ACOMPlug.Widget.Slide"]);
            }

             


            //FrameworkElement element = InitializeElementGrid( (Activator.CreateInstance(Models.Processers.Plugs.WidgetsPlugins[0]) as IPlugWidgetBase).Create());
            //CanvasView1.Items.Add( new Button() { Content = "Button1" });
        }

        private void Border_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            //Debug.WriteLine("cesfec");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Button button1 = sender as Button;
            //button1.ChangeCursor(InputSystemCursor.Create(InputSystemCursorShape.Wait));

            Button button = new Button();
            button.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            //CanvasView1.Items.Add(button);
        }

        private void ContentSizer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
        }



        private readonly Random rnd = new();
        private HomeLandingViewModel ViewModel;

        
        private void rangeSelector_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void OptionsAllCheckBox_Indeterminate(object sender, RoutedEventArgs e)
        {

        }




    }
}
