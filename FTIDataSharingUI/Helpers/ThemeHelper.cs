using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DataSubmission.Views
{
    public static class ThemeHelper
    {
        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        public static extern bool ShouldSystemUseDarkMode();

        public static void ApplyTheme(FrameworkElement element)
        {
            //var originalBackgroundColor = uiSettings.GetColorValue(UIColorType.Accent);
            UpdateTheme(element);
        }

        private static void UpdateTheme(FrameworkElement rootElement)
        {
            UpdateTextBlocks(rootElement);
        }

        private static void UpdateTextBlocks(DependencyObject parent)
        {
            //bool isDarkBgr = originalBackgroundColor.R < 128 && originalBackgroundColor.G < 128 && originalBackgroundColor.B < 128;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is TextBlock textBlock)
                {
                    //var clrs = (SolidColorBrush)textBlock.Foreground;
                    if (textBlock.Name.StartsWith("UserGreetings"))
                    {
                        //Debug.WriteLine(textBlock.Name.ToString());
                    }
                    else
                    {
                        if ((GetTheme() == "Dark" ) && (GetAppTheme() == "Dark"))
                        {
                            textBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
                        }
                        else
                        {
                            textBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black);
                        }
                    }

                }
                else if (child is Button buttonBlock)
                {
                    //var clrs = (SolidColorBrush)buttonBlock.Foreground;
                    if (buttonBlock.Name.StartsWith("btnRemove"))
                    {
                        Debug.WriteLine(buttonBlock.Name.ToString());
                        //break;
                    }
                    else
                    {
                        if ((GetTheme() == "Dark") && (GetAppTheme() == "Dark"))
                        {
                            buttonBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
                        }
                        else
                        {
                            buttonBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black);
                        }
                    }
                }
                if (child is DependencyObject)
                {
                    UpdateTextBlocks(child);
                }
            }

        }

        private static string GetTheme() 
        {
            var uiSettings = new UISettings();
            var accentColor = uiSettings.GetColorValue(UIColorType.Accent);
            var backgroundColor = uiSettings.GetColorValue(UIColorType.Background);
            bool isDarkTheme = backgroundColor.R < 128 && backgroundColor.G < 128 && backgroundColor.B < 128;

            if (ShouldSystemUseDarkMode())
            {
                return "Dark";
            }
            else
            {
                return "Light";
            }
        }

        private static string GetAppTheme()
        {
            var DefaultTheme = new Windows.UI.ViewManagement.UISettings();
            var uiTheme = DefaultTheme.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString();
            if (uiTheme == "#FF000000")
            {
                return  "Dark";
            }
            else if (uiTheme == "#FFFFFFFF")
            {
                return  "Light";
            }
            return "";
        }
    }


}
