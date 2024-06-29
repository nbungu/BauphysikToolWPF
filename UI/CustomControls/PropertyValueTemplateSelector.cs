using System.Windows;
using System.Windows.Controls;

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
                if (propertyItem.PropertyValues != null)
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

    public class PropertyItem
    {
        private string propertyName;
        private object propertyValue;
        private object[] propertyValues;

        public string PropertyName { get; set; }

        public object PropertyValue { get; set; }

        public object[] PropertyValues { get; set; }
    }
}
