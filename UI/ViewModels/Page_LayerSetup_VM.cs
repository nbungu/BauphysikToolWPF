using BauphysikToolWPF.Calculation;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using BT.Geometry;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static BauphysikToolWPF.Models.UI.Enums;
using Point = BT.Geometry.Point;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for Page_LayerSetup.xaml: Used in xaml as "DataContext"
    public partial class Page_LayerSetup_VM : ObservableObject
    {
        private readonly CrossSectionDrawing _crossSection = new CrossSectionDrawing(Session.SelectedElement, new Rectangle(new Point(0, 0), 880, 400), DrawingType.CrossSection);

        // Called by 'InitializeComponent()' from Page_LayerSetup.cs due to Class-Binding in xaml via DataContext
        public Page_LayerSetup_VM()
        {
            if (Session.SelectedProject is null) return;
            if (Session.SelectedElement is null) return;

            Session.SelectedLayerId = -1;
            Session.SelectedElement.SortLayers();
            Session.SelectedElement.AssignEffectiveLayers();
            Session.SelectedElement.AssignInternalIdsToLayers();

            // Allow child Windows to trigger UpdateAll of this Window
            Session.SelectedLayerChanged += UpdateAll;
            Session.EnvVarsChanged += UpdateRecalculate;

            // For values changed in PropertyDataGrid TextBox
            PropertyItem<double>.PropertyChanged += TriggerSelectedLayerChanged;
            PropertyItem<int>.PropertyChanged += TriggerSelectedLayerChanged;
            PropertyItem<bool>.PropertyChanged += TriggerSelectedLayerChanged;
        }

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationPage desiredPage)
        {
            MainWindow.SetPage(desiredPage);
        }

        [RelayCommand]
        private void AddLayer()
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new AddLayerWindow().ShowDialog();
        }
        [RelayCommand]
        private void EditLayer()
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            new AddLayerWindow(SelectedListViewItem?.InternalId ?? -1).ShowDialog();
        }

        [RelayCommand]
        private void AddSubConstructionLayer(int targetLayerInternalId = -1)
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            if (targetLayerInternalId == -1) targetLayerInternalId = SelectedListViewItem?.InternalId ?? -1;
            new AddLayerSubConstructionWindow(targetLayerInternalId).ShowDialog();
        }
        [RelayCommand]
        private void EditSubConstructionLayer(int targetLayerInternalId = -1)
        {
            // Once a window is closed, the same object instance can't be used to reopen the window.
            // Open as modal (Parent window pauses, waiting for the window to be closed)
            if (targetLayerInternalId == -1) targetLayerInternalId = SelectedListViewItem?.InternalId ?? -1;
            new AddLayerSubConstructionWindow(targetLayerInternalId).ShowDialog();
        }

        [RelayCommand]
        private void DeleteSubConstructionLayer(int targetLayerInternalId = -1) 
        {
            if (targetLayerInternalId == -1) targetLayerInternalId = SelectedListViewItem?.InternalId ?? -1;
            var targetLayer = Session.SelectedElement?.Layers.FirstOrDefault(l => l?.InternalId == targetLayerInternalId, null);
            targetLayer?.RemoveSubConstruction();
            Session.OnSelectedLayerChanged();
        }

        [RelayCommand]
        private void DeleteLayer()
        {
            if (Session.SelectedElement is null) return;
            if (SelectedListViewItem is null) return;

            Session.SelectedElement.RemoveLayer(SelectedListViewItem.InternalId);
            Session.OnSelectedLayerChanged();
            SelectedListViewItem = null;
        }

        [RelayCommand]
        private void DuplicateLayer()
        {
            if (Session.SelectedElement is null) return;
            if (SelectedListViewItem is null) return;

            Session.SelectedElement.DuplicateLayer(SelectedListViewItem.InternalId);
            Session.OnSelectedLayerChanged();
        }

        [RelayCommand]
        private void MoveLayerDown()
        {
            if (Session.SelectedElement is null) return;
            if (SelectedListViewItem is null) return;

            Session.SelectedElement.MoveLayerPositionToOutside(SelectedListViewItem.InternalId);
            Session.OnSelectedLayerChanged();
        }

        [RelayCommand]
        private void MoveLayerUp()
        {
            if (Session.SelectedElement is null) return;
            if (SelectedListViewItem is null) return;

            Session.SelectedElement.MoveLayerPositionToInside(SelectedListViewItem.InternalId);
            Session.OnSelectedLayerChanged();
        }

        [RelayCommand]
        private void LayerDoubleClick()
        {
            EditLayer();
        }

        // This method will be called whenever SelectedListViewItem changes
        partial void OnSelectedListViewItemChanged(Layer? value)
        {
            // Unselect all
            if (Session.SelectedElement is null) return;
            Session.SelectedElement.Layers.ForEach(l => l.IsSelected = false);

            if (value != null)
            {
                Session.SelectedLayerId = value.InternalId;
                Session.SelectedLayer.IsSelected = true;
            }
            _crossSection.UpdateDrawings();
        }
        
        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Everything the user can edit or change: All objects affected by user interaction.
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLayerSelected))]
        [NotifyPropertyChangedFor(nameof(SubConstructionExpanderVisibility))]
        [NotifyPropertyChangedFor(nameof(LayerPropertiesExpanderVisibility))]
        [NotifyPropertyChangedFor(nameof(LayerProperties))]
        [NotifyPropertyChangedFor(nameof(SubConstructionProperties))]
        [NotifyPropertyChangedFor(nameof(CrossSectionDrawing))]
        private Layer? _selectedListViewItem;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LayerMeasurement))]
        [NotifyPropertyChangedFor(nameof(LayerMeasurementFull))]
        [NotifyPropertyChangedFor(nameof(SubConstructionMeasurement))]
        [NotifyPropertyChangedFor(nameof(CrossSectionDrawing))]
        [NotifyPropertyChangedFor(nameof(CanvasSize))]
        private List<Layer>? _layerList = Session.SelectedElement?.Layers;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TiValue))]
        private static int _tiIndex; // As Static Class Variable to Save the Selection after Switching Pages!

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TeValue))]
        private static int _teIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RsiValue))]
        private static int _rsiIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RseValue))]
        private static int _rseIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RelFiValue))]
        private static int _relFiIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RelFeValue))]
        private static int _relFeIndex;

        /*
        * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
        * 
        * Not Observable, not directly mutated by user input
        */
       
        public string Title => Session.SelectedElement != null ? $"'{Session.SelectedElement.Name}' - Schichtaufbau " : "";
        public Element? SelectedElement => Session.SelectedElement;
        public bool IsLayerSelected => SelectedListViewItem != null;
        public Visibility SubConstructionExpanderVisibility => IsLayerSelected && SelectedListViewItem.HasSubConstructions ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LayerPropertiesExpanderVisibility => IsLayerSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility NoLayersVisibility => LayerList?.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        public List<IDrawingGeometry> CrossSectionDrawing => _crossSection.DrawingGeometries;
        public Rectangle CanvasSize => _crossSection.CanvasSize;
        public List<DrawingGeometry> LayerMeasurement => MeasurementDrawing.GetLayerMeasurementChain(_crossSection);
        public List<DrawingGeometry> SubConstructionMeasurement => MeasurementDrawing.GetSubConstructionMeasurementChain(_crossSection);
        public List<DrawingGeometry> LayerMeasurementFull => MeasurementDrawing.GetFullLayerMeasurementChain(_crossSection);
        public IEnumerable<IPropertyItem> LayerProperties => SelectedListViewItem?.PropertyBag ?? new List<IPropertyItem>(0);
        public IEnumerable<IPropertyItem> SubConstructionProperties => SelectedListViewItem?.SubConstruction?.PropertyBag ?? new List<IPropertyItem>(0);
        public List<string> TiKeys { get; } = DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TemperatureInterior).Select(e => e.Name).ToList();
        public List<string> TeKeys { get; } = DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TemperatureExterior).Select(e => e.Name).ToList();
        public List<string> RsiKeys { get; } = DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TransferResistanceSurfaceInterior).Select(e => e.Name).ToList();
        public List<string> RseKeys { get; } = DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TransferResistanceSurfaceExterior).Select(e => e.Name).ToList();
        public List<string> RelFiKeys { get; } = DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.RelativeHumidityInterior).Select(e => e.Name).ToList();
        public List<string> RelFeKeys { get; } = DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.RelativeHumidityExterior).Select(e => e.Name).ToList();
        

        public CheckRequirements RequirementValues { get; } = new CheckRequirements(Session.SelectedElement, Session.CheckRequirementsConfig);


        private GaugeItem? _uValueGauge;
        /// <summary>
        /// Gets the gauge configuration, lazily initializing it on first access.
        /// 
        /// This lazy pattern ensures that the GaugeItem is only constructed when needed, 
        /// avoiding repeated allocations from XAML bindings that may call this getter multiple times. 
        /// It improves performance and keeps initialization logic encapsulated.
        /// </summary>
        public GaugeItem UValueGauge
        {
            get
            {
                if (_uValueGauge == null)
                {
                    double? uMax = RequirementValues.UMax;
                    double elementUValue = RequirementValues.Element.UValue;
                    _uValueGauge = new GaugeItem(Symbol.UValue, elementUValue, uMax, RequirementValues.UMaxComparisonRequirement)
                    {
                        Caption = RequirementValues.UMaxCaption,
                        ScaleMin = 0.0,
                        ScaleMax = uMax.HasValue ? Math.Max(2 * uMax.Value, elementUValue + 0.1) : Math.Max(1.0, elementUValue + 0.1)
                    };
                }
                return _uValueGauge;
            }
        }

        private GaugeItem? _rValueGauge;
        /// <summary>
        /// Gets the gauge configuration, lazily initializing it on first access.
        /// 
        /// This lazy pattern ensures that the GaugeItem is only constructed when needed, 
        /// avoiding repeated allocations from XAML bindings that may call this getter multiple times. 
        /// It improves performance and keeps initialization logic encapsulated.
        /// </summary>
        public GaugeItem RValueGauge
        {
            get
            {
                if (_rValueGauge == null)
                {
                    var uValueGauge = UValueGauge; // ensure initialized once
                    double uValueNormalized = (uValueGauge.Value - uValueGauge.ScaleMin) / (uValueGauge.ScaleMax - uValueGauge.ScaleMin);
                    double targetRValueNormalized = 1.0 - uValueNormalized;

                    _rValueGauge = new GaugeItem(Symbol.RValueElement, RequirementValues.Element.RGesValue, RequirementValues.RMin, RequirementValues.RMinComparisonRequirement)
                    {
                        Caption = RequirementValues.RMinCaption,
                        ScaleMin = 0.0,
                        ScaleMax = RequirementValues.Element.RGesValue / targetRValueNormalized,
                    };
                }
                return _rValueGauge;
            }
        }

        // Index is 0:
        // On Initial Startup (default value for not assigned int)
        // Index is -1:
        // On custom user input
        public double TiValue
        {
            get
            {
                double? value = (_tiIndex == -1) ? Session.Ti : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TemperatureInterior).Find(e => e.Name == TiKeys[_tiIndex])?.Value;
                Session.Ti = value ?? 0.0;
                return Session.Ti;
            }
            set
            {
                // Save custom user input
                Session.Ti = value;
                // Changing ti_Index Triggers TiValue getter due to NotifyProperty
                TiIndex = -1;
            }
        }
        public double TeValue
        {
            get
            {
                double? value = (_teIndex == -1) ? Session.Te : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TemperatureExterior).Find(e => e.Name == TeKeys[_teIndex])?.Value;
                Session.Te = value ?? 0.0;
                return Session.Te;
            }
            set
            {
                Session.Te = value;
                TeIndex = -1;
            }
        }
        public double RsiValue
        {
            get
            {
                double? value = (_rsiIndex == -1) ? Session.Rsi : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TransferResistanceSurfaceInterior).Find(e => e.Name == RsiKeys[_rsiIndex])?.Value;
                Session.Rsi = value ?? 0.0;
                return Session.Rsi;
            }
            set
            {
                Session.Rsi = value;
                RsiIndex = -1;
            }
        }
        public double RseValue
        {
            get
            {
                double? value = (_rseIndex == -1) ? Session.Rse : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TransferResistanceSurfaceExterior).Find(e => e.Name == RseKeys[_rseIndex])?.Value;
                Session.Rse = value ?? 0.0;
                return Session.Rse;
            }
            set
            {
                Session.Rse = value;
                RseIndex = -1;
            }
        }
        public double RelFiValue
        {
            get
            {
                double? value = (_relFiIndex == -1) ? Session.RelFi : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.RelativeHumidityInterior).Find(e => e.Name == RelFiKeys[_relFiIndex])?.Value;
                Session.RelFi = value ?? 0.0;
                return Session.RelFi;
            }
            set
            {
                Session.RelFi = value;
                RelFiIndex = -1;
            }
        }
        public double RelFeValue
        {
            get
            {
                double? value = (_relFeIndex == -1) ? Session.RelFe : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.RelativeHumidityExterior).Find(e => e.Name == RelFeKeys[_relFeIndex])?.Value;
                Session.RelFe = value ?? 0.0;
                return Session.RelFe;
            }
            set
            {
                Session.RelFe = value;
                RelFeIndex = -1;
            }
        }

        /*
         * Trigger Hooks
         */

        /// <summary>
        /// Updates XAML Bindings and the Reset Calculation Flag
        /// </summary>
        private void UpdateAll()
        {
            UpdateRecalculate();

            RefreshDrawings();

            LayerList = null;
            LayerList = SelectedElement?.Layers;
            SelectedListViewItem = null;
            SelectedListViewItem = Session.SelectedLayer;

            // For updating MVVM Capsulated Properties
            OnPropertyChanged(nameof(SelectedElement));
            OnPropertyChanged(nameof(LayerProperties));
            OnPropertyChanged(nameof(NoLayersVisibility));
        }

        private void UpdateRecalculate()
        {
            Session.SelectedElement.RefreshResults();
            
            RefreshGauges();
        }

        // To also trigger the SelectedLayerChanged event for other subscribers
        private void TriggerSelectedLayerChanged()
        {
            Session.OnSelectedLayerChanged();
        }

        private void RefreshGauges()
        {
            RequirementValues.Update();
            // Forces the re-creation of the GaugeItems with updated values
            _uValueGauge = null;
            _rValueGauge = null;
            OnPropertyChanged(nameof(UValueGauge));
            OnPropertyChanged(nameof(RValueGauge));
        }

        private void RefreshDrawings()
        {
            _crossSection.UpdateDrawings();
            OnPropertyChanged(nameof(LayerMeasurement));
            OnPropertyChanged(nameof(LayerMeasurementFull));
            OnPropertyChanged(nameof(SubConstructionMeasurement));
            OnPropertyChanged(nameof(CrossSectionDrawing));
            OnPropertyChanged(nameof(CanvasSize));
        }
    }
}
