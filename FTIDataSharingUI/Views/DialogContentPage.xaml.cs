﻿using DataSubmission.ViewModels;
using DataSubmission.Views;
using Microsoft.UI.Xaml.Controls;

namespace FTIDataSharingUI.Views;

public sealed partial class DialogContentPage : Page
{
    public DialogContentViewModel ViewModel
    {
        get;
    }

    public DialogContentPage()
    {
        ViewModel = App.GetService<DialogContentViewModel>();
        InitializeComponent();
        ThemeHelper.ApplyTheme(this);
    }
}
