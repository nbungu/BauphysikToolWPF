using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using BauphysikToolWPF.UI.CustomControls;
using BT.Geometry;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using static BauphysikToolWPF.Models.UI.Enums;
using Point = BT.Geometry.Point;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for Page_LayerSetup.xaml: Used in xaml as "DataContext"
    public partial class Page_LayerSetup_VM : ObservableObject
    {
        private readonly CrossSectionDrawing _crossSection = new CrossSectionDrawing();
        private readonly IDialogService _dialogService;
        private readonly Element _element;

        // Called by 'InitializeComponent()' from Page_LayerSetup.cs due to Class-Binding in xaml via DataContext
        public Page_LayerSetup_VM()
        {
            if (Session.SelectedElement is null) return;

            _element = Session.SelectedElement;
            _element.SortLayers();
            _element.AssignEffectiveLayers();
            _element.AssignInternalIdsToLayers();

            _dialogService = new DialogService();
            _crossSection = new CrossSectionDrawing(_element, new Rectangle(new Point(0, 0), 880, 400), DrawingType.CrossSection);
            
            // Allow child Windows to trigger UpdateAll of this Window
            Session.SelectedProjectChanged += ProjectDataChanged;
            Session.SelectedLayerChanged += LayerChanged;
            Session.SelectedElementChanged += ElementChanged;
            Session.EnvVarsChanged += EnvVarsChanged;

            // For values changed in PropertyDataGrid TextBox
            PropertyItem<double>.PropertyChanged += PropertyItemChanged;
            PropertyItem<int>.PropertyChanged += PropertyItemChanged;
            PropertyItem<bool>.PropertyChanged += PropertyItemChanged;
        }

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void SwitchPage(NavigationPage desiredPage) => MainWindow.SetPage(desiredPage);

        [RelayCommand]
        private void AddLayer() => _dialogService.ShowAddNewLayerDialog();

        [RelayCommand]
        private void EditLayer() => _dialogService.ShowEditLayerDialog(SelectedLayer?.InternalId ?? -1);

        [RelayCommand]
        private void AddSubConstructionLayer(int targetLayerInternalId = -1)
        {
            if (targetLayerInternalId == -1) targetLayerInternalId = SelectedLayer?.InternalId ?? -1;
            _dialogService.ShowAddNewSubconstructionDialog(targetLayerInternalId);
        }

        [RelayCommand]
        private void EditSubConstructionLayer(int targetLayerInternalId = -1)
        {
            if (targetLayerInternalId == -1) targetLayerInternalId = SelectedLayer?.InternalId ?? -1;
            _dialogService.ShowEditSubconstructionDialog(targetLayerInternalId);
        }

        [RelayCommand]
        private void DeleteSubConstructionLayer(int targetLayerInternalId = -1) 
        {
            if (targetLayerInternalId == -1) targetLayerInternalId = SelectedLayer?.InternalId ?? -1;
            var targetLayer = _element.Layers.FirstOrDefault(l => l?.InternalId == targetLayerInternalId, null);
            targetLayer?.RemoveSubConstruction();
            Session.OnSelectedLayerChanged();
        }

        [RelayCommand]
        private void DeleteLayer()
        {
            if (SelectedLayer is null) return;
            var newIndex = SelectedLayerIndex - 1;
            _element.RemoveLayerById(SelectedLayer.InternalId);
            Session.OnSelectedElementChanged();
            SelectedLayerIndex = newIndex;
        }

        [RelayCommand]
        private void DeleteAllLayer()
        {
            MessageBoxResult result = _dialogService.ShowDeleteConfirmationDialog();

            switch (result)
            {
                case MessageBoxResult.Yes:
                    _element.Layers.Clear();
                    Session.OnSelectedElementChanged();
                    SelectedLayerIndex = -1;
                    break;
                case MessageBoxResult.Cancel:
                    // Do nothing, user cancelled the action
                    break;
            }
        }

        [RelayCommand]
        private void DuplicateLayer()
        {
            if (SelectedLayer is null) return;

            _element.DuplicateLayerById(SelectedLayer.InternalId);
            Session.OnSelectedElementChanged();
        }

        [RelayCommand]
        private void MoveLayerDown()
        {
            if (SelectedLayer is null) return;

            _element.MoveLayerPositionToOutside(SelectedLayer.InternalId);
            Session.OnSelectedElementChanged();
        }

        [RelayCommand]
        private void MoveLayerUp()
        {
            if (SelectedLayer is null) return;

            _element.MoveLayerPositionToInside(SelectedLayer.InternalId);
            Session.OnSelectedElementChanged();
        }

        [RelayCommand]
        private void LayerDoubleClick() => EditLayer();

        // This method will be called whenever SelectedLayer changes
        //partial void OnSelectedListViewItemChanged(Layer? value) => SelectedLayerIndexChanged(value);
        partial void OnSelectedLayerIndexChanged(int value) => SelectedLayerIndexChanged(value);

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Everything the user can edit or change: All objects affected by user interaction.
         */

        [ObservableProperty]
        private int _selectedLayerIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLayerSelected))]
        [NotifyPropertyChangedFor(nameof(LayerPropertyBag))]
        [NotifyPropertyChangedFor(nameof(LayerSubConstrPropertyBag))]
        [NotifyPropertyChangedFor(nameof(SubConstructionExpanderVisibility))]
        [NotifyPropertyChangedFor(nameof(LayerPropertiesExpanderVisibility))]
        private Layer? _selectedLayer;

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
       
        public string Title => $"'{_element.Name}' - Schichtaufbau ";
        public string SelectedElementColorCode => _element.ColorCode;
        public string SelectedElementConstructionName => _element.Construction.TypeName;
        public ObservableCollection<Layer> LayerList => new ObservableCollection<Layer>(_element.Layers);
        public IEnumerable<IPropertyItem>? LayerPropertyBag => SelectedLayer?.PropertyBag;
        public IEnumerable<IPropertyItem>? LayerSubConstrPropertyBag => SelectedLayer?.SubConstruction?.PropertyBag;

        public bool IsLayerSelected => SelectedLayer != null;
        public bool HasItems => LayerList.Count > 0;
        public Visibility SubConstructionExpanderVisibility => IsLayerSelected && SelectedLayer?.SubConstruction != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LayerPropertiesExpanderVisibility => IsLayerSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility NoLayersVisibility => HasItems ? Visibility.Collapsed : Visibility.Visible;
        public List<IDrawingGeometry> CrossSectionDrawing => _crossSection.DrawingGeometries;
        public Rectangle CanvasSize => _crossSection.CanvasSize;
        public List<DrawingGeometry> LayerMeasurement => MeasurementDrawing.GetLayerMeasurementChain(_crossSection);
        public List<DrawingGeometry> LayerMeasurementFull => MeasurementDrawing.GetFullLayerMeasurementChain(_crossSection);
        public List<DrawingGeometry> SubConstructionMeasurement => MeasurementDrawing.GetSubConstructionMeasurementChain(_crossSection);
        
        public List<string> TiKeys { get; } = DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TemperatureInterior).Select(e => e.Name).ToList();
        public List<string> TeKeys { get; } = DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TemperatureExterior).Select(e => e.Name).ToList();
        public List<string> RsiKeys { get; } = DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TransferResistanceSurfaceInterior).Select(e => e.Name).ToList();
        public List<string> RseKeys { get; } = DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TransferResistanceSurfaceExterior).Select(e => e.Name).ToList();
        public List<string> RelFiKeys { get; } = DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.RelativeHumidityInterior).Select(e => e.Name).ToList();
        public List<string> RelFeKeys { get; } = DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.RelativeHumidityExterior).Select(e => e.Name).ToList();


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
                    double? uMax = _element.Requirements.UMax;
                    double? elementUValue = _element.ThermalResults.IsValid ? _element.UValue : null;
                    double scaleMax = uMax.HasValue
                        ? Math.Max(2 * uMax.Value, (elementUValue ?? 0.0) + 0.1)
                        : Math.Max(1.0, (elementUValue ?? 0.0) + 0.1);

                    _uValueGauge = new GaugeItem(Symbol.UValue, elementUValue, uMax, _element.Requirements.UMaxComparisonRequirement)
                    {
                        Caption = _element.Requirements.UMaxCaption,
                        ScaleMin = 0.0,
                        ScaleMax = scaleMax,
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
                    double? uValueNormalized = (uValueGauge.Value - uValueGauge.ScaleMin) / (uValueGauge.ScaleMax - uValueGauge.ScaleMin);
                    double? targetRValueNormalized = 1.0 - uValueNormalized;
                    double? elementRValue = _element.ThermalResults.IsValid ? _element.RGesValue : null;
                    _rValueGauge = new GaugeItem(Symbol.RValueElement, elementRValue, _element.Requirements.RMin, _element.Requirements.RMinComparisonRequirement)
                    {
                        Caption = _element.Requirements.RMinCaption,
                        ScaleMin = 0.0,
                        ScaleMax = _element.RGesValue / targetRValueNormalized ?? 1.0,
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
                double? value = (_tiIndex == -1) ? _element.ThermalCalcConfig.Ti : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TemperatureInterior).Find(e => e.Name == TiKeys[_tiIndex])?.Value;
                _element.ThermalCalcConfig.Ti = value ?? 0.0;
                return _element.ThermalCalcConfig.Ti;
            }
            set
            {
                // Save custom user input
                _element.ThermalCalcConfig.Ti = value;
                // Changing ti_Index Triggers TiValue getter due to NotifyProperty
                TiIndex = -1;
                if (Math.Abs(value) > 40) MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Warning);
            }
        }
        public double TeValue
        {
            get
            {
                double? value = (_teIndex == -1) ? _element.ThermalCalcConfig.Te : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TemperatureExterior).Find(e => e.Name == TeKeys[_teIndex])?.Value;
                _element.ThermalCalcConfig.Te = value ?? 0.0;
                return _element.ThermalCalcConfig.Te;
            }
            set
            {
                _element.ThermalCalcConfig.Te = value;
                TeIndex = -1;
                if (Math.Abs(value) > 50) MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Warning);
            }
        }
        public double RsiValue
        {
            get
            {
                double? value = (_rsiIndex == -1) ? _element.ThermalCalcConfig.Rsi : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TransferResistanceSurfaceInterior).Find(e => e.Name == RsiKeys[_rsiIndex])?.Value;
                _element.ThermalCalcConfig.Rsi = value ?? 0.0;
                return _element.ThermalCalcConfig.Rsi;
            }
            set
            {
                if (value < 0)
                {
                    MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Error);
                    return;
                }
                _element.ThermalCalcConfig.Rsi = value;
                RsiIndex = -1;
                if (value > 1) MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Warning);
            }
        }
        public double RseValue
        {
            get
            {
                double? value = (_rseIndex == -1) ? _element.ThermalCalcConfig.Rse : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TransferResistanceSurfaceExterior).Find(e => e.Name == RseKeys[_rseIndex])?.Value;
                _element.ThermalCalcConfig.Rse = value ?? 0.0;
                return _element.ThermalCalcConfig.Rse;
            }
            set
            {
                if (value < 0)
                {
                    MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Error);
                    return;
                }
                _element.ThermalCalcConfig.Rse = value;
                RseIndex = -1;
                if (value > 1) MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Warning);
            }
        }
        public double RelFiValue
        {
            get
            {
                double? value = (_relFiIndex == -1) ? _element.ThermalCalcConfig.RelFi : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.RelativeHumidityInterior).Find(e => e.Name == RelFiKeys[_relFiIndex])?.Value;
                _element.ThermalCalcConfig.RelFi = value ?? 0.0;
                return _element.ThermalCalcConfig.RelFi;
            }
            set
            {
                if (value < 0 || value > 100)
                {
                    MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Error);
                    return;
                }
                _element.ThermalCalcConfig.RelFi = value;
                RelFiIndex = -1;
            }
        }
        public double RelFeValue
        {
            get
            {
                double? value = (_relFeIndex == -1) ? _element.ThermalCalcConfig.RelFe : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.RelativeHumidityExterior).Find(e => e.Name == RelFeKeys[_relFeIndex])?.Value;
                _element.ThermalCalcConfig.RelFe = value ?? 0.0;
                return _element.ThermalCalcConfig.RelFe;
            }
            set
            {
                if (value < 0 || value > 100)
                {
                    MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Error);
                    return;
                }
                _element.ThermalCalcConfig.RelFe = value;
                RelFeIndex = -1;
            }
        }

        #region Who triggers: Event Handlers for UI Events

        private void ProjectDataChanged()
        {
            RefreshGauges();
        }

        private void ElementChanged()
        {
            RefreshGauges();
            RefreshDrawingsFull();
            RefreshPropertyGrid();
            RefreshLayerCollection();
            RefreshElementTimeStamp();
        }

        /// <summary>
        /// Updates XAML Bindings and the Reset Calculation Flag
        /// </summary>
        private void LayerChanged()
        {
            RefreshGauges();
            RefreshDrawingsFull();
            RefreshPropertyGrid();
            RefreshLayerCollection();
            RefreshLayerTimeStamp();
        }

        // To also trigger the SelectedLayerChanged event for other subscribers
        private void PropertyItemChanged()
        {
            // To notify all other subscribers (e.g. MainWindow) of this Event aswell
            Session.OnSelectedLayerChanged();
        }

        private void EnvVarsChanged(Symbol changed)
        {
            // Only run logic for Rsi or Rse change
            if (changed == Symbol.TransferResistanceSurfaceInterior || changed == Symbol.TransferResistanceSurfaceExterior)
                RefreshGauges();
        }

        private void SelectedLayerIndexChanged(int value)
        {
            for (int i = 0; i < LayerList.Count; i++)
            {
                LayerList[i].IsSelected = (i == value);
            }
            RefreshDrawingsLayerSelected();
        }

        #endregion

        #region What is refreshed: Refresh Methods for selected XAML Elements

        private void RefreshPropertyGrid()
        {
            if (SelectedLayer is null) return;
            SelectedLayer.RefreshPropertyBag();
            OnPropertyChanged(nameof(LayerPropertyBag));
            OnPropertyChanged(nameof(LayerSubConstrPropertyBag));
        }

        private void RefreshDrawingsFull()
        {
            _crossSection.UpdateDrawings();
            OnPropertyChanged(nameof(CrossSectionDrawing));
            OnPropertyChanged(nameof(CanvasSize));
            OnPropertyChanged(nameof(LayerMeasurement));
            OnPropertyChanged(nameof(LayerMeasurementFull));
            OnPropertyChanged(nameof(SubConstructionMeasurement));
        }
        private void RefreshDrawingsLayerSelected()
        {
            _crossSection.UpdateDrawings();
            OnPropertyChanged(nameof(CrossSectionDrawing));
        }

        private void RefreshGauges()
        {
            // Recalulates the Element
            _element.Requirements.Update();
            // Forces the re-creation of the GaugeItems with updated values
            _uValueGauge = null;
            _rValueGauge = null;
            OnPropertyChanged(nameof(UValueGauge));
            OnPropertyChanged(nameof(RValueGauge));
        }

        private void RefreshLayerCollection()
        {
            OnPropertyChanged(nameof(LayerList));
            OnPropertyChanged(nameof(NoLayersVisibility));
            OnPropertyChanged(nameof(HasItems));
        }

        private void RefreshLayerTimeStamp()
        {
            if (SelectedLayer is null) return;
            SelectedLayer.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();

            RefreshElementTimeStamp();
        }
        private void RefreshElementTimeStamp()
        {
            _element.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }

        #endregion
    }
}
