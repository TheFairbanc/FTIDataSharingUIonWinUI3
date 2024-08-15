using FTIDataSharingUI.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace FTIDataSharingUI.Views;

public sealed partial class FilePreviewPage : Page
{
    public FilePreviewViewModel FilePreviewViewModel
    {
        get;
    }
    public FilePreviewViewModel ViewModel
    {
        get;
        private set;
    }

    public FilePreviewPage()
    {
        ViewModel = App.GetService<FilePreviewViewModel>();
        InitializeComponent();
    }
}