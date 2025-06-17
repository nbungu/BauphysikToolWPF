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
        private readonly CrossSectionDrawing _crossSection = new CrossSectionDrawing();
        private readonly CheckRequirements _requirementValues = new CheckRequirements();

        private readonly IDialogService _dialogService;

        // Called by 'InitializeComponent()' from Page_LayerSetup.cs due to Class-Binding in xaml via DataContext
        public Page_LayerSetup_VM()
        {
            if (Session.SelectedProject is null) return;
            if (Session.SelectedElement is null) return;

            _dialogService = new DialogService();

            _crossSection = new CrossSectionDrawing(Session.SelectedElement, new Rectangle(new Point(0, 0), 880, 400), DrawingType.CrossSection);
            // TODO: this could be 'Element' property and be fetched from the SelectedElement directly
            // -> just set Update flag to true
            _requirementValues = new CheckRequirements(Session.SelectedElement, Session.CheckRequirementsConfig);

            Session.SelectedLayerId = -1;
            Session.SelectedElement.SortLayers();
            Session.SelectedElement.AssignEffectiveLayers();
            Session.SelectedElement.AssignInternalIdsToLayers();

            // Allow child Windows to trigger UpdateAll of this Window
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
        private void EditLayer() => _dialogService.ShowEditLayerDialog(SelectedListViewItem?.InternalId ?? -1);

        [RelayCommand]
        private void AddSubConstructionLayer(int targetLayerInternalId = -1)
        {
            if (targetLayerInternalId == -1) targetLayerInternalId = SelectedListViewItem?.InternalId ?? -1;
            _dialogService.ShowAddNewSubconstructionDialog(targetLayerInternalId);
        }

        [RelayCommand]
        private void EditSubConstructionLayer(int targetLayerInternalId = -1)
        {
            if (targetLayerInternalId == -1) targetLayerInternalId = SelectedListViewItem?.InternalId ?? -1;
            _dialogService.ShowEditSubconstructionDialog(targetLayerInternalId);
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

            Session.SelectedElement.RemoveLayerById(SelectedListViewItem.InternalId);
            Session.OnSelectedElementChanged();
            SelectedListViewItem = null;
        }
        [RelayCommand]
        private void DeleteAllLayer()
        {
            if (Session.SelectedElement is null) return;

            MessageBoxResult result = _dialogService.ShowDeleteConfirmationDialog();

            switch (result)
            {
                case MessageBoxResult.Yes:
                    Session.SelectedElement.Layers.Clear();
                    Session.OnSelectedElementChanged();
                    SelectedListViewItem = null;
                    break;
                case MessageBoxResult.Cancel:
                    // Do nothing, user cancelled the action
                    break;
            }
        }

        [RelayCommand]
        private void DuplicateLayer()
        {
            if (Session.SelectedElement is null) return;
            if (SelectedListViewItem is null) return;

            Session.SelectedElement.DuplicateLayerById(SelectedListViewItem.InternalId);
            Session.OnSelectedElementChanged();
        }

        [RelayCommand]
        private void MoveLayerDown()
        {
            if (Session.SelectedElement is null) return;
            if (SelectedListViewItem is null) return;

            Session.SelectedElement.MoveLayerPositionToOutside(SelectedListViewItem.InternalId);
            Session.OnSelectedElementChanged();
        }

        [RelayCommand]
        private void MoveLayerUp()
        {
            if (Session.SelectedElement is null) return;
            if (SelectedListViewItem is null) return;

            Session.SelectedElement.MoveLayerPositionToInside(SelectedListViewItem.InternalId);
            Session.OnSelectedElementChanged();
        }

        [RelayCommand]
        private void LayerDoubleClick() => EditLayer();

        // This method will be called whenever SelectedListViewItem changes
        partial void OnSelectedListViewItemChanged(Layer? value) => SelectedLayerChanged(value);

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Everything the user can edit or change: All objects affected by user interaction.
         */

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLayerSelected))]
        [NotifyPropertyChangedFor(nameof(SubConstructionExpanderVisibility))]
        [NotifyPropertyChangedFor(nameof(LayerPropertiesExpanderVisibility))]
        private Layer? _selectedListViewItem;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NoLayersVisibility))]
        [NotifyPropertyChangedFor(nameof(HasItems))]
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
       
        public string Title { get; } = Session.SelectedElement != null ? $"'{Session.SelectedElement.Name}' - Schichtaufbau " : "";
        public string SelectedElementColorCode { get; } = Session.SelectedElement?.ColorCode ?? string.Empty;
        public string SelectedElementConstructionName { get; } = Session.SelectedElement?.Construction.TypeName ?? string.Empty;

        public bool IsLayerSelected => SelectedListViewItem != null;
        public bool HasItems => LayerList?.Count > 0;
        public Visibility SubConstructionExpanderVisibility => IsLayerSelected && SelectedListViewItem?.SubConstruction != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LayerPropertiesExpanderVisibility => IsLayerSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility NoLayersVisibility => HasItems ? Visibility.Collapsed : Visibility.Visible;
        public List<IDrawingGeometry> CrossSectionDrawing => _crossSection.DrawingGeometries;
        public Rectangle CanvasSize => _crossSection.CanvasSize;
        public List<DrawingGeometry> LayerMeasurement => MeasurementDrawing.GetLayerMeasurementChain(_crossSection);
        public List<DrawingGeometry> SubConstructionMeasurement => MeasurementDrawing.GetSubConstructionMeasurementChain(_crossSection);
        public List<DrawingGeometry> LayerMeasurementFull => MeasurementDrawing.GetFullLayerMeasurementChain(_crossSection);
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
                if (_requirementValues.Element is null) return new GaugeItem(Symbol.UValue);

                if (_uValueGauge == null)
                {
                    double? uMax = _requirementValues.UMax;
                    double? elementUValue = _requirementValues.Element.ThermalResults.IsValid ? _requirementValues.Element.UValue : null;
                    double scaleMax = uMax.HasValue
                        ? Math.Max(2 * uMax.Value, (elementUValue ?? 0.0) + 0.1)
                        : Math.Max(1.0, (elementUValue ?? 0.0) + 0.1);

                    _uValueGauge = new GaugeItem(Symbol.UValue, elementUValue, uMax, _requirementValues.UMaxComparisonRequirement)
                    {
                        Caption = _requirementValues.UMaxCaption,
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
                if (_requirementValues.Element is null) return new GaugeItem(Symbol.RValueElement);

                if (_rValueGauge == null)
                {
                    var uValueGauge = UValueGauge; // ensure initialized once
                    double? uValueNormalized = (uValueGauge.Value - uValueGauge.ScaleMin) / (uValueGauge.ScaleMax - uValueGauge.ScaleMin);
                    double? targetRValueNormalized = 1.0 - uValueNormalized;
                    double? elementRValue = _requirementValues.Element.ThermalResults.IsValid ? _requirementValues.Element.RGesValue : null;
                    _rValueGauge = new GaugeItem(Symbol.RValueElement, elementRValue, _requirementValues.RMin, _requirementValues.RMinComparisonRequirement)
                    {
                        Caption = _requirementValues.RMinCaption,
                        ScaleMin = 0.0,
                        ScaleMax = _requirementValues.Element.RGesValue / targetRValueNormalized ?? 1.0,
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

        #region Who triggers: Event Handlers for UI Events

        private void ElementChanged()
        {
            RefreshGauges();
            RefreshDrawingsFull();
            RefreshPropertyGrid();

            LayerList = null;
            LayerList = Session.SelectedElement?.Layers;
            SelectedListViewItem = null;
            SelectedListViewItem = Session.SelectedLayer;
        }

        /// <summary>
        /// Updates XAML Bindings and the Reset Calculation Flag
        /// </summary>
        private void LayerChanged()
        {
            RefreshGauges();
            RefreshDrawingsFull();
            RefreshPropertyGrid();

            // when sub construction changes: To reflect the new buttons in the listView
            LayerList = null;
            LayerList = Session.SelectedElement?.Layers;
            SelectedListViewItem = null;
            SelectedListViewItem = Session.SelectedLayer;
        }

        // To also trigger the SelectedLayerChanged event for other subscribers
        private void PropertyItemChanged()
        {
            // To notify all other subscribers (e.g. MainWindow) of this Event aswell
            Session.OnSelectedLayerChanged();
        }

        private void EnvVarsChanged()
        {
            RefreshGauges();
        }

        private void SelectedLayerChanged(Layer? value)
        {
            // Unselect all
            if (Session.SelectedElement is null) return;
            Session.SelectedElement.Layers.ForEach(l => l.IsSelected = false);

            if (value != null)
            {
                Session.SelectedLayerId = value.InternalId;
                Session.SelectedLayer.IsSelected = true;
            }
            RefreshDrawingsLayerSelected();
        }

        #endregion

        #region What is refreshed: Refresh Methods for selected XAML Elements

        private void RefreshPropertyGrid()
        {
            if (SelectedListViewItem is null) return;
            SelectedListViewItem.RefreshPropertyBag();
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
            _requirementValues.Update();
            // Forces the re-creation of the GaugeItems with updated values
            _uValueGauge = null;
            _rValueGauge = null;
            OnPropertyChanged(nameof(UValueGauge));
            OnPropertyChanged(nameof(RValueGauge));
        }

        #endregion
    }
}
