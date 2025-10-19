using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public static class DynamicContextMenuService
{
    public static void ShowContextMenu(
        UIElement placementTarget,
        BT.Geometry.Point position,
        IEnumerable<ContextMenuItemDefinition> items)
    {
        var menu = new ContextMenu
        {
            PlacementTarget = placementTarget,
            Placement = System.Windows.Controls.Primitives.PlacementMode.Relative,
            HorizontalOffset = position.X,
            VerticalOffset = position.Y
        };

        foreach (var def in items)
        {
            if (def.IsSeparator)
            {
                menu.Items.Add(new Separator());
                continue;
            }

            if (!(def.IsVisible ?? false))
                continue;

            var item = new MenuItem
            {
                Header = def.Header
            };
            if (def.IconSource != null)
            {
                item.Icon = new Image
                {
                    Source = def.IconSource,
                    Width = 16,
                    Height = 16
                };
            }

            item.Click += (_, _) => def.Action?.Invoke();

            menu.Items.Add(item);
        }

        // Open the menu
        menu.IsOpen = true;
    }
}

public class ContextMenuItemDefinition
{
    public string Header { get; set; } = string.Empty;
    public ImageSource? IconSource { get; set; }
    public Action? Action { get; set; } // invoked method
    public bool IsSeparator { get; set; }
    public bool? IsVisible { get; set; } = true;

    // Optional - you can add CanExecute or dynamic visibility later if needed
}

