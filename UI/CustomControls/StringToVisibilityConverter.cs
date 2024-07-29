﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BauphysikToolWPF.UI.CustomControls
{
    // In order to use the Visibility binding,
    // you need a value converter that converts an empty string
    // to Collapsed and any other string to Visible.
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
}
