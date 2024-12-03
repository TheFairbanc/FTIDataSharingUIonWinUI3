using CommunityToolkit.Mvvm.ComponentModel;
using DataSubmission.Contracts.Services;
using DataSubmission.ViewModels;
using FTIDataSharingUI.Views;

using Microsoft.UI.Xaml.Controls;

namespace DataSubmission.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    public PageService()
    {
        Configure<LandingViewModel, LandingPage>();
        Configure<LoginViewModel, LoginPage>();
        Configure<MainMenuViewModel, MainMenuPage>();
        Configure<AutoProcessViewModel, AutoProcessPage>();
        Configure<AutoConfigViewModel, AutoConfigPage>();
        Configure<ManualProcessViewModel, ManualProcessPage>();
        Configure<HelpScreenViewModel, HelpScreenPage>();
        Configure<LogScreenViewModel, LogScreenPage>();
        Configure<DialogContentViewModel, DialogContentPage>();
        Configure<FilePreviewViewModel, FilePreviewPage>();
        Configure<FilePrevViewModel, FilePrevPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(V);
            if (_pages.ContainsValue(type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }
}
