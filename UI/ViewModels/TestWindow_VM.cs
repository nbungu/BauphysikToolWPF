using System.Collections.Generic;
using System.Linq;
using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geometry;

namespace BauphysikToolWPF.UI.ViewModels
{
    public partial class TestWindow_VM : ObservableObject
    {
        

        /*
         * Regular Instance Variables
         * 
         * Not depending on UI changes. No Observable function.
         */

        
        //public LiveChartsCore.Measure.Margin ChartMargin_i { get; private set; } = new LiveChartsCore.Measure.Margin(64, 16, 0, 64);

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationContent desiredPage)
        {
            MainWindow.SetPage(desiredPage);
        }

        [RelayCommand]
        private void EditElement() // Binding in XAML via 'EditElementCommand'
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new EditElementWindow().ShowDialog();
        }

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private Element _selectedElement = UserSaved.SelectedElement;

        /*
         * MVVM Capsulated Properties + Triggered by other Properties
         * 
         * Not Observable, because Triggered and Changed by the Values above
         */
        
        //public Rectangle Bounds => UserSaved.SelectedElement.CalculationAreaBounds();
        public static Rectangle CanvasSize { get; set; } = new Rectangle(new Point(0, 0), 400, 400);

        //public List<IDrawingGeometry> DrawingGeometries => UserSaved.SelectedElement.GetCrossSectionDrawing(CanvasSize.Width, CanvasSize.Height);

        // Using a Single-Item Collection, since ItemsSource of XAML Element expects IEnumerable iface
        public List<DrawingGeometry> LayerMeasurement => MeasurementChain.GetMeasurementChain(UserSaved.SelectedElement.Layers).ToList();
        //public List<DrawingGeometry> SubConstructionMeasurement => MeasurementChain.GetMeasurementChain(MeasurementChain.GetGeometryIntervals(DrawingGeometries.Where(g => (int)g.Tag == 0 && g.ZIndex == 1), Axis.X), Axis.X).ToList();
        //public List<DrawingGeometry> SubConstructionMeasurement => MeasurementChain.GetMeasurementChain(DrawingGeometries.Where(g => g.ZIndex == 1), Axis.X).ToList();

        // Using a Single-Item Collection, since ItemsSource of XAML Element expects IEnumerable iface
        public List<DrawingGeometry> LayerMeasurementFull => UserSaved.SelectedElement.Layers.Count > 1 ? MeasurementChain.GetMeasurementChain(new[] { 0, 400.0 }).ToList() : new List<DrawingGeometry>();


    }
}
