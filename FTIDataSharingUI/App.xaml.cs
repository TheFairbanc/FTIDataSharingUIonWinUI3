using FTIDataSharingUI.Activation;
using FTIDataSharingUI.Contracts.Services;
using FTIDataSharingUI.Core.Contracts.Services;
using FTIDataSharingUI.Core.Services;
using FTIDataSharingUI.Helpers;
using FTIDataSharingUI.Services;
using FTIDataSharingUI.ViewModels;
using FTIDataSharingUI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using OfficeOpenXml.FormulaParsing.Logging;
using Microsoft.UI.Xaml.Controls;

namespace FTIDataSharingUI;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();



            // Services
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<DialogContentViewModel>();
            services.AddTransient<DialogContentPage>();
            services.AddTransient<LogScreenViewModel>();
            services.AddTransient<LogScreenPage>();
            services.AddTransient<HelpScreenViewModel>();
            services.AddTransient<HelpScreenPage>();
            services.AddTransient<ManualProcessViewModel>();
            services.AddTransient<ManualProcessPage>();
            services.AddTransient<AutoConfigViewModel>();
            services.AddTransient<AutoConfigPage>();
            services.AddTransient<AutoProcessViewModel>();
            services.AddTransient<AutoProcessPage>();
            services.AddTransient<MainMenuViewModel>();
            services.AddTransient<MainMenuPage>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<LoginPage>();
            services.AddTransient<LandingViewModel>();
            services.AddTransient<LandingPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<FilePreviewViewModel>();
            services.AddTransient<FilePreviewPage>();

            // Create your custom LogBroker
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

            // Configuration
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Done => Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
        if (sender is Page _page)
        {
            ;
            ContentDialog errorDialog = new ContentDialog
            {
                XamlRoot = _page.XamlRoot,
                Title = "Info Kesalahan",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                Content = $"Gagal melakukan pengkinian data pada {DateTimeOffset.Now}."
            };
            errorDialog.ShowAsync();
        }

    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        

        await App.GetService<IActivationService>().ActivateAsync(args);
        // Initialize logging
    }
}
