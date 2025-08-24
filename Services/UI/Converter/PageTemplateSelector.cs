using System;
using System.Windows;
using System.Windows.Controls;
using BauphysikToolWPF.Services.Application;

namespace BauphysikToolWPF.Services.UI.Converter
{
    public class PageTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(item is NavigationPage page))
                return null;

            // See App.xaml e.g.: <DataTemplate x:Key="0">
            string key = Convert.ToString((int)page);

            return NavigationManager.GetPageFromAppResources(key);
        }
    }
}
