using System.Diagnostics;
using ACOM.Models;
using ACOMPlugin.Core;
using ACOMv2.Persistence;
using ACOMv2.Views.DeviceConnect;
using CustomExtensions.WinUI;
using DryIoc;
using DiFactory = ShadowPluginLoader.WinUI.DiFactory;

namespace ACOMv2;

public partial class App : Application
{
    public static Window MainWindow = Window.Current;
    public IServiceProvider Services { get; }
    public new static App Current => (App)Application.Current;
    public IJsonNavigationViewService GetJsonNavigationViewService => GetService<IJsonNavigationViewService>();
    public IThemeService GetThemeService => GetService<IThemeService>();

    public static T GetService<T>() where T : class
    {
        if ((App.Current as App)!.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    
    public App()
    {
        LoggerSetup.ConfigureLogger();
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NMaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWX5eeHRXRGlcVEB2W0c=");
        Services = ConfigureServices();
        this.InitializeComponent();
        ApplicationExtensionHost.Initialize(this);
        DiFactory.Services.Register<ACOMPluginLoader>(reuse: Reuse.Singleton);
        Debug.WriteLine("LogDirectoryPath is " + Constants.LogDirectoryPath);
        Debug.WriteLine("RootDirectoryPath is " + Constants.RootDirectoryPath);
        Debug.WriteLine("AppConfigPath is " + Constants.AppConfigPath);
        Debug.WriteLine("LogFilePath is " + Constants.LogFilePath);

        LoggerSetup.Logger.Warning("App Start");
        
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IJsonNavigationViewService>(factory =>
        {
            var json = new JsonNavigationViewService();
            //json.ConfigDefaultPage(typeof(HomeLandingPage));
            json.ConfigDefaultPage(typeof(SerialConnect));
            //json.ConfigDefaultPage(typeof(UDPConnect));
            //json.ConfigDefaultPage(typeof(TCPConnect));
            json.ConfigSettingsPage(typeof(SettingsPage));
            return json;
        });

        services.AddTransient<MainViewModel>();
        services.AddSingleton<ContextMenuService>();
        services.AddSingleton<IO_Manage>();
        services.AddTransient<GeneralSettingViewModel>();
        services.AddTransient<AppUpdateSettingViewModel>();
        services.AddTransient<AboutUsSettingViewModel>();
        //services.AddTransient<BreadCrumbBarViewModel>();
        services.AddSingleton<HomeLandingViewModel>();

        return services.BuildServiceProvider();
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new Window();

        if (MainWindow.Content is not Frame rootFrame)
        {
            MainWindow.Content = rootFrame = new Frame();
        }

        if (GetThemeService != null)
        {
            GetThemeService.AutoInitialize(MainWindow);
        }

        var menuService = GetService<ContextMenuService>();
        if (menuService != null)
        {
            ContextMenuItem menu = new ContextMenuItem
            {
                Title = "Open ACOMv2 Here",
                Param = @"""{path}""",
                AcceptFileFlag = (int)FileMatchFlagEnum.All,
                AcceptDirectoryFlag = (int)(DirectoryMatchFlagEnum.Directory | DirectoryMatchFlagEnum.Background | DirectoryMatchFlagEnum.Desktop),
                AcceptMultipleFilesFlag = (int)FilesMatchFlagEnum.Each,
                Index = 0,
                Enabled = true,
                Icon = ProcessInfoHelper.GetFileVersionInfo().FileName,
                Exe = "ACOMv2.exe"
            };

            await menuService.SaveAsync(menu);
        }

        rootFrame.Navigate(typeof(MainPage));

        MainWindow.Title = MainWindow.AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;
        MainWindow.AppWindow.SetIcon("Assets/icon.ico");

        if (Settings.UseDeveloperMode)
        {
            ConfigureLogger();
        }

        MainWindow.Activate();

        UnhandledException += (s, e) => Logger?.Error(e.Exception, "UnhandledException");
    }
}

