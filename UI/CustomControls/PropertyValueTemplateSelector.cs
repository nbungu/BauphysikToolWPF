using BauphysikToolWPF.Models;
using System.Windows;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class PropertyValueTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextBoxTemplate { get; set; }
        public DataTemplate ComboBoxTemplate { get; set; }
        public DataTemplate CheckBoxTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is PropertyItem propertyItem)
            {
                if (propertyItem.PropertyValues != null && propertyItem.PropertyValues.Length > 1)
                {
                    return ComboBoxTemplate;
                }
                else if (propertyItem.Value is bool)
                {
                    return CheckBoxTemplate;
                }
                else
                {
                    return TextBoxTemplate;
                }

                
            }
            return base.SelectTemplate(item, container);
        }
    }
}
