﻿using BauphysikToolWPF.SQLiteRepo;
using BauphysikToolWPF.ComponentCalculations;
using BauphysikToolWPF.EnvironmentData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Reflection.Emit;
using SkiaSharp;
using Xceed.Wpf.Toolkit.Converters;

namespace BauphysikToolWPF.UI
{
    public class DrawLayerCanvas
    {
        public DrawLayerCanvas(List<Layer> layers, Canvas canvas)
        {
            if (layers == null || layers.Count == 0)
                return;
            DrawRectanglesFromLayers(layers, canvas);
        }               
       
        public void DrawRectanglesFromLayers(List<Layer> layers, Canvas canvas)
        {
            canvas.Children.Clear();
            double right = canvas.Width;

            double elementWidth = 0;
            foreach (Layer layer in layers)
            {
                elementWidth += layer.LayerThickness;
            }
            // Drawing from right to left
            foreach (Layer layer in layers)
            {
                double layerWidthScale = layer.LayerThickness / elementWidth; // from  0 ... 1
                double layerWidth = canvas.Width * layerWidthScale;
                double left = right - layerWidth; // start drawing from right canvas side (beginning with INSIDE Layer, which is first list element) -> We want Inside layer position on right/inner side. 

                // Draw layer rectangle
                Rectangle baseRect = new Rectangle()
                {
                    Width = layerWidth,
                    Height = canvas.Height,
                    Stroke = layer.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black,
                    StrokeThickness = layer.IsSelected ? 2 : 0.2,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(layer.correspondingMaterial().ColorCode)),
                };
                canvas.Children.Add(baseRect);
                Canvas.SetTop(baseRect, 0);
                Canvas.SetLeft(baseRect, left);

                // Draw hatch pattern rectangle
                Rectangle hatchPatternRect = new Rectangle()
                {
                    Width = layerWidth - 1,
                    Height = canvas.Height,
                    Fill = HatchPattern.GetHatchPattern(layer.Material.Category, layerWidth, canvas.Height),
                    Opacity = 0.7
                };
                canvas.Children.Add(hatchPatternRect);
                Canvas.SetTop(hatchPatternRect, 0);
                Canvas.SetLeft(hatchPatternRect, left+0.5);

                // Add Label with layer position
                System.Windows.Controls.Label label = new System.Windows.Controls.Label()
                {
                    Content = layer.LayerPosition,
                    FontSize = 14
                };
                canvas.Children.Add(label);
                Canvas.SetTop(label, 0);
                Canvas.SetLeft(label, left);

                right -= layerWidth; // Add new layer at left edge of previous layer
            }
        }
    }
}
