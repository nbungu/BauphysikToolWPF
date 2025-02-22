﻿using BauphysikToolWPF.Services;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BauphysikToolWPF.UI.Models;

namespace BauphysikToolWPF.UI.CustomControls
{
    /// <summary>
    /// Interaktionslogik für PropertyDataGrid.xaml
    /// </summary>
    public partial class PropertyDataGrid : UserControl
    {
        public PropertyDataGrid()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register(nameof(Properties), typeof(object), typeof(PropertyDataGrid), new PropertyMetadata(null));

        public object Properties
        {
            get => GetValue(PropertiesProperty);
            set => SetValue(PropertiesProperty, value);
        }

        private void numericData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var propertyItem = textBox.DataContext as IPropertyItem ?? null;
                var type = propertyItem?.Value.GetType();
                if (type is null) return;
                if (type == typeof(double)) e.Handled = TextInputValidation.NumericCurrentCulture.IsMatch(e.Text);
                else if (type == typeof(int)) e.Handled = TextInputValidation.IntegerCurrentCulture.IsMatch(e.Text);
            }
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.IsReadOnly) return;

                var propertyItem = textBox.DataContext as IPropertyItem ?? null;
                if (textBox.Text != "" && propertyItem != null)
                {
                    // Change Value in property item, which reflects to the corresponding SelectedLayer Property
                    var type = propertyItem.Value.GetType();

                    if (type == typeof(double)) propertyItem.Value = Convert.ToDouble(textBox.Text, CultureInfo.CurrentCulture);
                    else if (type == typeof(int)) propertyItem.Value = int.Parse(textBox.Text);
                    else propertyItem.Value = textBox.Text.ToString(CultureInfo.CurrentCulture);
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox.IsReadOnly) return;

                var propertyItem = comboBox.DataContext as IPropertyItem ?? null;
                if (comboBox.SelectedItem != null && propertyItem != null)
                {
                    propertyItem.Value = comboBox.SelectedItem;
                }
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (!checkBox.IsEnabled) return;

                var propertyItem = checkBox.DataContext as IPropertyItem ?? null;
                if (propertyItem != null)
                {
                    propertyItem.Value = checkBox.IsChecked ?? false;
                }
            }
        }
    }
}
