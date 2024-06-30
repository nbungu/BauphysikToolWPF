using System.Windows;
using System.Windows.Controls;
using BauphysikToolWPF.Models;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class PropertyValueTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextBoxTemplate { get; set; }
        public DataTemplate ComboBoxTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is PropertyItem propertyItem)
            {
                if (propertyItem.PropertyValues != null && propertyItem.PropertyValues.Length > 0)
                {
                    return ComboBoxTemplate;
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
