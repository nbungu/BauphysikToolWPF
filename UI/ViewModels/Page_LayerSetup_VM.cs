using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using BauphysikToolWPF.Services.UI.OpenGL;
using BauphysikToolWPF.UI.CustomControls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using BauphysikToolWPF.Services.UI.Converter;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for Page_LayerSetup.xaml: Used in xaml as "DataContext"
    public partial class Page_LayerSetup_VM : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly Element _element;
        private readonly OglController _oglController;

        // Called by 'InitializeComponent()' from Page_LayerSetup.cs due to Class-Binding in xaml via DataContext
        public Page_LayerSetup_VM(OglController scene)
        {
            if (Session.SelectedElement is null) return;

            _oglController = scene;
            _element = Session.SelectedElement;
            _dialogService = new DialogService();

            // Allow child Windows to trigger UpdateAll of this Window
            Session.SelectedProjectChanged += ProjectDataChanged;
            Session.SelectedLayerChanged += LayerChanged;
            Session.SelectedElementChanged += ElementChanged;
            Session.EnvVarsChanged += EnvVarsChanged;

            _oglController.ShapeClicked += OnShapeClicked;
            _oglController.ShapeDoubleClicked += OnShapeDoubleClicked;

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
        public void EditLayer() => _dialogService.ShowEditLayerDialog(SelectedLayer?.InternalId ?? -1);

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
        public void DeleteSubConstructionLayer(int targetLayerInternalId = -1) 
        {
            if (targetLayerInternalId == -1) targetLayerInternalId = SelectedLayer?.InternalId ?? -1;
            var targetLayer = _element.Layers.FirstOrDefault(l => l?.InternalId == targetLayerInternalId, null);
            targetLayer?.RemoveSubConstruction();
            Session.OnSelectedLayerChanged();
        }

        [RelayCommand]
        public void DeleteLayer()
        {
            if (SelectedLayer is null) return;
            var newIndex = SelectedLayerIndex - 1;
            _element.RemoveLayerById(SelectedLayer.InternalId);
            Session.OnSelectedElementChanged();
            SelectedLayerIndex = newIndex;
        }

        [RelayCommand]
        public void DeleteAllLayer()
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

        [RelayCommand]
        private void ToggleDecorationVisibility()
        {
           _oglController.ShowSceneDecoration = !_oglController.ShowSceneDecoration;
           OnPropertyChanged(nameof(ShowDecoration));
        }

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
        [NotifyPropertyChangedFor(nameof(PropertyHeader1))]
        [NotifyPropertyChangedFor(nameof(PropertyHeader2))]
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
        public bool ShowDecoration => _oglController.ShowSceneDecoration;
        public string PropertyHeader1 => $"Schicht: {SelectedLayer?.Material.Name}";
        public string PropertyHeader2 => $"Balkenlage: {SelectedLayer?.SubConstruction?.Material.Name}";
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
        public string TiValue
        {
            get
            {
                double? value = (_tiIndex == -1) ? _element.ThermalCalcConfig.Ti : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TemperatureInterior).Find(e => e.Name == TiKeys[_tiIndex])?.Value;
                _element.ThermalCalcConfig.Ti = value ?? 0.0;
                return NumberConverter.ConvertToString(_element.ThermalCalcConfig.Ti, 1);
            }
            set
            {
                // Save custom user input
                _element.ThermalCalcConfig.Ti = NumberConverter.ConvertToDouble(value);
                // Changing ti_Index Triggers TiValue getter due to NotifyProperty
                TiIndex = -1;
                if (Math.Abs(_element.ThermalCalcConfig.Ti) > 40) MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Warning);
            }
        }
        public string TeValue
        {
            get
            {
                double? value = (_teIndex == -1) ? _element.ThermalCalcConfig.Te : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TemperatureExterior).Find(e => e.Name == TeKeys[_teIndex])?.Value;
                _element.ThermalCalcConfig.Te = value ?? 0.0;
                return NumberConverter.ConvertToString(_element.ThermalCalcConfig.Te, 1);
            }
            set
            {
                _element.ThermalCalcConfig.Te = NumberConverter.ConvertToDouble(value);
                TeIndex = -1;
                if (Math.Abs(_element.ThermalCalcConfig.Te) > 50) MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Warning);
            }
        }
        public string RsiValue
        {
            get
            {
                double? value = (_rsiIndex == -1) ? _element.ThermalCalcConfig.Rsi : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TransferResistanceSurfaceInterior).Find(e => e.Name == RsiKeys[_rsiIndex])?.Value;
                _element.ThermalCalcConfig.Rsi = value ?? 0.0;
                return NumberConverter.ConvertToString(_element.ThermalCalcConfig.Rsi);
            }
            set
            {
                if (NumberConverter.ConvertToDouble(value) < 0)
                {
                    MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Error);
                    return;
                }
                _element.ThermalCalcConfig.Rsi = NumberConverter.ConvertToDouble(value);
                RsiIndex = -1;
                if (_element.ThermalCalcConfig.Rsi > 1) MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Warning);
            }
        }
        public string RseValue
        {
            get
            {
                double? value = (_rseIndex == -1) ? _element.ThermalCalcConfig.Rse : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.TransferResistanceSurfaceExterior).Find(e => e.Name == RseKeys[_rseIndex])?.Value;
                _element.ThermalCalcConfig.Rse = value ?? 0.0;
                return NumberConverter.ConvertToString(_element.ThermalCalcConfig.Rse);
            }
            set
            {
                if (NumberConverter.ConvertToDouble(value) < 0)
                {
                    MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Error);
                    return;
                }
                _element.ThermalCalcConfig.Rse = NumberConverter.ConvertToDouble(value);
                RseIndex = -1;
                if (_element.ThermalCalcConfig.Rse > 1) MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Warning);
            }
        }
        public string RelFiValue
        {
            get
            {
                double? value = (_relFiIndex == -1) ? _element.ThermalCalcConfig.RelFi : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.RelativeHumidityInterior).Find(e => e.Name == RelFiKeys[_relFiIndex])?.Value;
                _element.ThermalCalcConfig.RelFi = value ?? 0.0;
                return NumberConverter.ConvertToString(_element.ThermalCalcConfig.RelFi, 1);
            }
            set
            {
                if (NumberConverter.ConvertToDouble(value) < 0 || NumberConverter.ConvertToDouble(value) > 100)
                {
                    MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Error);
                    return;
                }
                _element.ThermalCalcConfig.RelFi = NumberConverter.ConvertToDouble(value);
                RelFiIndex = -1;
            }
        }
        public string RelFeValue
        {
            get
            {
                double? value = (_relFeIndex == -1) ? _element.ThermalCalcConfig.RelFe : DatabaseAccess.QueryDocumentParameterBySymbol(Symbol.RelativeHumidityExterior).Find(e => e.Name == RelFeKeys[_relFeIndex])?.Value;
                _element.ThermalCalcConfig.RelFe = value ?? 0.0;
                return NumberConverter.ConvertToString(_element.ThermalCalcConfig.RelFe, 1);
            }
            set
            {
                if (NumberConverter.ConvertToDouble(value) < 0 || NumberConverter.ConvertToDouble(value) > 100)
                {
                    MainWindow.ShowToast("Unrealistischer Wert!", ToastType.Error);
                    return;
                }
                _element.ThermalCalcConfig.RelFe = NumberConverter.ConvertToDouble(value);
                RelFeIndex = -1;
            }
        }

        #region Who triggers: Event Handlers for UI Events

        private void OnShapeClicked(ShapeId shape)
        {
            var targetLayer = LayerList.FirstOrDefault(l => l?.InternalId == shape.Index, null);
            if (targetLayer != null)
            {
                var index = LayerList.IndexOf(targetLayer);
                SelectedLayerIndex = index;
            }
            Console.WriteLine($"VM Shape clicked: {shape}");
        }

        private void OnShapeDoubleClicked(ShapeId shape)
        {
            var layerShapeTarget = LayerList.FirstOrDefault(l => l?.InternalId == shape.Index, null);
            
            if (layerShapeTarget != null)
            {
                var index = LayerList.IndexOf(layerShapeTarget);
                SelectedLayerIndex = index;

                if (shape.Type == ShapeType.SubConstructionLayer) EditSubConstructionLayer(SelectedLayer?.InternalId ?? -1);
                else if (shape.Type == ShapeType.DimensionalChain) LayerDoubleClick(); // TODO: Put focus on thickness TextBox;
                else LayerDoubleClick();
            }
            Console.WriteLine($"VM Shape double clicked: {shape}");
        }

        private void ProjectDataChanged()
        {
            RefreshGauges();
        }

        private void ElementChanged()
        {
            RefreshGauges();
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
            Session.OnSelectedLayerIndexChanged();
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
