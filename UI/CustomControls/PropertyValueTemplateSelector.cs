using BauphysikToolWPF.Models.Helper;
using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class PropertyValueTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? TextBoxTemplate { get; set; }
        public DataTemplate? ComboBoxTemplate { get; set; }
        public DataTemplate? CheckBoxTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IPropertyItem propertyItem)
            {
                if (propertyItem.PropertyValues != null && propertyItem.PropertyValues.Length > 1)
                {
                    return ComboBoxTemplate ?? new DataTemplate();
                }
                if (propertyItem.Value is bool)
                {
                    return CheckBoxTemplate ?? new DataTemplate();
                }
                return TextBoxTemplate ?? new DataTemplate();
            }
            return base.SelectTemplate(item, container) ?? new DataTemplate();
        }
    }
}
