using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using BT.Geometry;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Media;
using static BauphysikToolWPF.Models.UI.Enums;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace BauphysikToolWPF.Models.Domain
{
    /// <summary>
    /// Business logic of a Layer
    /// </summary>
    public partial class Layer : IDomainObject<Layer>, IDrawingGeometry
    {
        #region Serialization Objects

        private int _layerPosition; // Inside = 0
        public int LayerPosition
        {
            get => _layerPosition;
            set
            {
                _layerPosition = value;
                if (SubConstruction != null) SubConstruction.LayerNumber = LayerNumber;
            }
        }

        public double Thickness { get; set; } // Layer thickness in cm
        
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        private int _materialId = -1;

        public int MaterialId
        {
            get => _materialId;
            set
            {
                _materialId = value;
                ReloadMaterial = true;
                ReloadPropertyBag = true;
            }
        }

        public LayerSubConstruction? SubConstruction { get; set; }

        private bool _isEffective = true;
        public bool IsEffective
        {
            get => _isEffective;
            set
            {
                _isEffective = value;
                if (SubConstruction != null) SubConstruction.IsEffective = value;
            }
        }

        #endregion

        #region Non-serialized Properties

        [JsonIgnore]
        public int InternalId { get; set; } = -1;

        [JsonIgnore]
        public bool ReloadPropertyBag { get; private set; } = true;

        private IEnumerable<IPropertyItem> _propertyBag = new List<IPropertyItem>();

        [JsonIgnore]
        public IEnumerable<IPropertyItem> PropertyBag
        {
            get
            {
                if (ReloadPropertyBag)
                {
                    _propertyBag = new List<IPropertyItem>()
                    {
                        new PropertyItem<string>("Kategorie", () => Material.CategoryName),
                        new PropertyItem<string>("Materialquelle", () => Material.IsUserDefined ? "Benutzerdefiniert" : "aus Materialdatenbank"),
                        new PropertyItem<double>(Symbol.Thickness, () => Thickness, value => Thickness = value),
                        new PropertyItem<double>(Symbol.ThermalConductivity, () => Material.ThermalConductivity) { DecimalPlaces = 3 },
                        new PropertyItem<double>(Symbol.RValueLayer, () => R_Value)
                        {
                            SymbolSubscriptText = $"{LayerNumber}"
                        },
                        new PropertyItem<int>(Symbol.RawDensity, () => Material.BulkDensity),
                        new PropertyItem<double>(Symbol.AreaMassDensity, () => AreaMassDensity),
                        new PropertyItem<double>(Symbol.SdThickness, () => Sd_Thickness) { DecimalPlaces = 1 },
                        new PropertyItem<double>(Symbol.VapourDiffusionResistance, () => Material.DiffusionResistance),
                        new PropertyItem<int>(Symbol.SpecificHeatCapacity, () => Material.SpecificHeatCapacity),
                        new PropertyItem<double>(Symbol.ArealHeatCapacity, () => ArealHeatCapacity)
                        {
                            SymbolSubscriptText = $"{LayerNumber}"
                        },
                        new PropertyItem<bool>("Wirksame Schicht", () => IsEffective, value => IsEffective = value)
                    };
                    ReloadPropertyBag = false;
                }
                return _propertyBag;
            }
        }

        [JsonIgnore]
        public bool ReloadMaterial { get; private set; } = true;

        // 1:1 relationship with Material
        private Material _material = Material.Empty;
        /// <summary>
        /// Lazy Initialization of Material object.
        /// </summary>
        [JsonIgnore]
        public Material Material
        {
            get
            {
                if (ReloadMaterial)
                {
                    _material = DatabaseAccess.QueryMaterialById(MaterialId);
                    ReloadMaterial = false;
                }
                return _material;
            }
        }

        [JsonIgnore]
        public bool IsSelected { get; set; } // For UI Purposes 

        [JsonIgnore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);

        [JsonIgnore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);

        [JsonIgnore]
        public bool IsValid => LayerPosition >= 0 && Thickness > 0 && Material.IsValid;

        [JsonIgnore]
        public int LayerNumber => LayerPosition + 1;
        
        [JsonIgnore]
        public double R_Value
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                return Math.Round((this.Thickness / 100) / Material.ThermalConductivity, 3);
            }
        }

        [JsonIgnore]
        public double Sd_Thickness // sd thickness in m
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                return Math.Round((this.Thickness / 100) * Material.DiffusionResistance, 3);
            }
        }

        [JsonIgnore]
        public double AreaMassDensity // m' in kg/m²
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                if (SubConstruction != null)
                {
                    var partialAreaOfLayer = 1 - (SubConstruction.Width / (SubConstruction.Width + SubConstruction.Spacing));
                    return Math.Round((this.Thickness / 100) * Material.BulkDensity * partialAreaOfLayer, 3);
                }
                return Math.Round((this.Thickness / 100) * Material.BulkDensity, 3);
            }
        }

        [JsonIgnore]
        public double ArealHeatCapacity // C_i in kJ/m²K 
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                if (SubConstruction != null)
                {
                    var partialAreaOfLayer = 1 - (SubConstruction.Width / (SubConstruction.Width + SubConstruction.Spacing));
                    return (this.Thickness / 100) * Material.VolumetricHeatCapacity * partialAreaOfLayer;
                } 
                return (this.Thickness / 100) * Material.VolumetricHeatCapacity;
            }
        }

        // Temperaturleitfähigkeit a gibt das Mass für die Geschwindigkeit an,
        // mit der sich eine Temperaturänderung im Material ausbreitet:
        [JsonIgnore]
        public double TemperatureConductivity // a in m²/s
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                return Math.Round(Material.ThermalConductivity / System.Convert.ToDouble(Material.BulkDensity / Material.SpecificHeatCapacity), 2);
            }
        }

        #endregion

        #region ctors

        #endregion

        #region Public Methods
        
        public Layer Copy()
        {
            var copy = new Layer();
            copy.LayerPosition = this.LayerPosition;
            copy.MaterialId = this.MaterialId;
            copy.Thickness = this.Thickness;
            copy.IsEffective = this.IsEffective;
            copy.IsSelected = false;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            // Deep copy of the SubConstructions list
            if (SubConstruction != null) this.SubConstruction.CopyToNewLayer(copy);
            return copy;
        }
        public void CopyToElement(Element element)
        {
            var copy = Copy();
            element.Layers.Add(copy);
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }
        
        public override string ToString() // Überlagert vererbte standard ToString() Methode 
        {
            return Thickness + " cm, " + Material.Name + " (Pos.: " + LayerPosition + ")";
        }

        public void RemoveSubConstruction()
        {
            this.SubConstruction = null;
        }

        /// <summary>
        /// Forces re-initialization of the Material by querying the database using the current MaterialId.
        /// </summary>
        public void RefreshMaterial()
        {
            ReloadMaterial = true;
            SubConstruction?.RefreshMaterial();
        }

        /// <summary>
        /// Forces re-initialization of the PropertyBag using current Material and Layer properties.
        /// </summary>
        public void RefreshPropertyBag()
        {
            ReloadPropertyBag = true;
            SubConstruction?.RefreshPropertyBag();
        }

        #endregion
    }

    /// <summary>
    /// Presentation logic of a Layer which can be drawn on a XAML Canvas
    /// </summary>
    public partial class Layer : IDrawingGeometry
    {
        #region IDrawingGeometry
        [JsonIgnore]
        public Rectangle Rectangle { get; set; } = Rectangle.Empty;
        [JsonIgnore]
        public Brush RectangleBorderColor { get; set; } = Brushes.Black;
        [JsonIgnore]
        public double RectangleBorderThickness { get; set; } = 0.2;
        [JsonIgnore]
        public DoubleCollection RectangleStrokeDashArray { get; set; } = new DoubleCollection();
        [JsonIgnore]
        public Brush BackgroundColor { get; set; } = Brushes.Transparent;
        [JsonIgnore]
        public Brush DrawingBrush { get; set; } = new DrawingBrush();
        [JsonIgnore]
        public double Opacity { get; set; } = 1;
        [JsonIgnore]
        public int ZIndex { get; set; } = 0;
        [JsonIgnore]
        public object Tag { get; set; } = new object();
        [JsonIgnore]
        public int? TextureId { get; set; }

        public IDrawingGeometry Convert()
        {
            return new DrawingGeometry(this);
        }
        
        [JsonIgnore]
        public System.Drawing.RectangleF RectangleF => new System.Drawing.RectangleF(
            (float)Rectangle.X,
            (float)Rectangle.Y,
            (float)Rectangle.Width,
            (float)Rectangle.Height
        );
        [JsonIgnore]
        public Vector4 BackgroundColorVector { get; private set; } = new Vector4(0, 0, 0, 0);

        // Call this from the UI thread before rendering. ensure UpdateBrushCache runs on UI thread
        public void UpdateBrushCache()
        {
            if (System.Windows.Application.Current?.Dispatcher?.CheckAccess() == false)
            {
                // If we're not on UI thread, invoke synchronously on UI thread
                System.Windows.Application.Current.Dispatcher.Invoke(UpdateBrushCache);
                return;
            }
            if (BackgroundColor is SolidColorBrush solidColor)
            {
                var c = solidColor.Color;
                BackgroundColorVector = new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
            }
            else
            {
                BackgroundColorVector = new Vector4(0, 0, 0, 0);
            }
        }

        #endregion
    }
}
