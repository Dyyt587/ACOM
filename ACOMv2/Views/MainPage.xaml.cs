using ACOMv2.Views.DeviceConnect;
using DryIoc;
using Microsoft.UI.Xaml.Media.Animation;

namespace ACOMv2.Views;

public sealed partial class MainPage : Page
{
    private int previousSelectedIndex;
    private int flag = 1;

    public MainViewModel ViewModel { get; }
    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        this.InitializeComponent();
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);

        var jsonNavigationViewService = App.GetService<IJsonNavigationViewService>() as JsonNavigationViewService;
        if (jsonNavigationViewService != null)
        {
            jsonNavigationViewService.Initialize(NavView, NavFrame, NavigationPageMappings.PageDictionary);
            jsonNavigationViewService.ConfigJson("Assets/NavViewMenu/AppData.json");
            //jsonNavigationViewService.ConfigBreadcrumbBar(JsonBreadCrumbNavigator, BreadcrumbPageMappings.PageDictionary);
        }




        MainFrame.Navigate(typeof(HomeLandingPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });

    }

    private void AppTitleBar_BackRequested(TitleBar sender, object args)
    {
        if (NavFrame.CanGoBack)
        {
            NavFrame.GoBack();
        }
    }

    private void AppTitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    private void NavFrame_Navigated(object sender, NavigationEventArgs e)
    {
        AppTitleBar.IsBackButtonVisible = NavFrame.CanGoBack;
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        ThemeService.ChangeThemeWithoutSave(App.MainWindow);
    }

    private void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxTextChangedEvent(sender, args, NavFrame);
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxQuerySubmittedEvent(sender, args, NavFrame);
    }

 
    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        // 根据目标页面类型执行相应的操作
        if (args.IsSettingsSelected)
        {
            MainGrid.ColumnDefinitions[1].Width = new GridLength(1080);
        }
        else
        {

            MainGrid.ColumnDefinitions[1].Width = new GridLength(240);
        }
    }
}

