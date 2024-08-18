using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BauphysikToolWPF.UI.CustomControls
{
    public class ExpandableGridSplitter : GridSplitter
    {
        static ExpandableGridSplitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExpandableGridSplitter),
                new FrameworkPropertyMetadata(typeof(ExpandableGridSplitter))
            );
        }

        public ExpandableGridSplitter()
        {
            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var expanderButton = GetTemplateChild("PART_ExpanderToggleButton") as ToggleButton;
            if (expanderButton != null)
            {
                expanderButton.Click += ExpanderButton_Click;
            }
        }

        private void ExpanderButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Grid grid)
            {
                if (this.HorizontalAlignment == HorizontalAlignment.Stretch)
                {
                    // Handle horizontal splitter logic
                    int rowIndex = Grid.GetRow(this);
                    var rowDefinition = grid.RowDefinitions[rowIndex];
                    rowDefinition.Height = rowDefinition.Height.Value > 0 ? new GridLength(0) : new GridLength(rowDefinition.MaxHeight);//new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    // Handle vertical splitter logic
                    int columnIndex = Grid.GetColumn(this);
                    var columnDefinition = grid.ColumnDefinitions[columnIndex];
                    columnDefinition.Width = columnDefinition.Width.Value > 0 ? new GridLength(0) : new GridLength(columnDefinition.MaxWidth);
                }
            }
        }
    }
}
