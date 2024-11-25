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

namespace DataSubmission.Views
{
    public static class ThemeHelper
    {
        public static void ApplyTheme(FrameworkElement element)
        {
            //var oriColor = (Color)element.Resources["SystemAccentColor"];
            //var textColor = GetContrastingColor((Color)element.Resources["SystemAccentColor"]);
            UpdateTheme(element);
        }

        private static void UpdateTheme(FrameworkElement rootElement)
        {
            UpdateTextBlocks(rootElement);
        }

        private static void UpdateTextBlocks(DependencyObject parent)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is TextBlock textBlock)
                {
                    var clrs = (SolidColorBrush)textBlock.Foreground;
                    if (textBlock.Name.StartsWith("UserGreetings"))
                    {
                        //Debug.WriteLine(textBlock.Name.ToString());
                        break;  
                    }
                    if (GetTheme() == "Dark")
                    {
                        textBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
                    }else
                    {
                        textBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black);
                    }

                }
                else if (child is DependencyObject)
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

            if (isDarkTheme)
            {
                return "Dark";
            }
            else
            {
                return "Light";
            }

        }
    }


}
