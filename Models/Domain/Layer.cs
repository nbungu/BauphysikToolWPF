using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using BT.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Media;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Models.Domain
{
    /// <summary>
    /// Business logic of a Layer
    /// </summary>
    public partial class Layer : IDomainObject<Layer>, IDrawingGeometry
    {
        #region Serialization Objects

        public int LayerPosition { get; set; } // Inside = 0

        public double Thickness { get; set; } // Layer thickness in cm
        
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        // 1:1 relationship with Material
        public int MaterialId { get; set; } = -1;

        // 1:n relationship with LayerSubConstruction
        public List<LayerSubConstruction> SubConstructions { get; set; } = new List<LayerSubConstruction>(1);

        #endregion

        #region Non-serialized Properties

        [JsonIgnore]
        public int InternalId { get; set; } = -1;
        
        [JsonIgnore]
        public IEnumerable<IPropertyItem> PropertyBag => new List<IPropertyItem>()
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

        [JsonIgnore]
        public static Layer Empty => new Layer(); // Optional static default (for easy reference)

        // 1:1 relationship with Material
        [JsonIgnore]
        public Material Material => DatabaseAccess.QueryMaterialById(MaterialId);

        [JsonIgnore]
        public bool IsSelected { get; set; } // For UI Purposes 

        private bool _isEffective = true;
        [JsonIgnore]
        public bool IsEffective
        {
            get => _isEffective;
            set
            {
                _isEffective = value;
                if (HasSubConstructions && SubConstruction != null) SubConstruction.IsEffective = value;
            }
        }

        [JsonIgnore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [JsonIgnore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);
        [JsonIgnore]
        public bool HasSubConstructions => SubConstructions.Count > 0;
        [JsonIgnore]
        public bool IsValid => LayerPosition >= 0 && Thickness > 0 && Material.IsValid;
        [JsonIgnore]
        public int LayerNumber => LayerPosition + 1;

        // Workaround: Currently only 1 SubConstruction supported
        [JsonIgnore]
        public LayerSubConstruction? SubConstruction
        {
            get => SubConstructions.FirstOrDefault();
            set
            {
                if (value == null) return;
                SubConstructions.Clear();
                SubConstructions.Add(value);
            }
        }

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
                if (HasSubConstructions && SubConstruction != null)
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
                if (HasSubConstructions && SubConstruction != null)
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
            this.SubConstructions.ForEach(sc => sc.CopyToNewLayer(copy));
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
            this.SubConstructions.Clear();
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

        public IDrawingGeometry Convert()
        {
            return new DrawingGeometry(this);
        }

        #endregion
    }
}
