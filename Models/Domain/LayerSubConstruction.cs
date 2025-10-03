using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.Services.UI;
using BauphysikToolWPF.Services.UI.OpenGL;
using BT.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Media;
using static BauphysikToolWPF.Models.Database.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Models.Domain
{
    /// <summary>
    /// Business logic of a LayerSubConstruction
    /// </summary>
    public partial class LayerSubConstruction : IDomainObject<LayerSubConstruction>, IDrawingGeometry
    {
        #region Serialization Objects

        public double Width { get; set; } // cm
        public double Thickness { get; set; } // cm
        public double Spacing { get; set; } // cm (innerer Abstand)
        public ConstructionDirection Direction { get; set; }
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        private int _materialId = -1;
        // 1:1 relationship with Material
        public int MaterialId
        {
            get => _materialId;
            set
            {
                _materialId = value;
                ReloadMaterial = true;
            }
        }
        public int LayerNumber { get; set; } = -1;

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
                        new PropertyItem<string>("Material", () => Material.Name),
                        new PropertyItem<string>("Kategorie", () => Material.CategoryName),
                        new PropertyItem<int>("Ausrichtung", () => (int)Direction, value => Direction = (ConstructionDirection)value)
                        {
                            PropertyValues = SubConstructionDirectionMapping.Values.Cast<object>().ToArray()
                        },
                        new PropertyItem<double>(Symbol.Thickness, () => Thickness, value => Thickness = value),
                        new PropertyItem<double>(Symbol.Width, () => Width, value => Width = value),
                        new PropertyItem<double>(Symbol.Distance, () => Spacing, value => Spacing = value),
                        new PropertyItem<double>("Achsenabstand", Symbol.Distance, () => AxisSpacing, value => AxisSpacing = value),
                        new PropertyItem<double>(Symbol.RValueLayer, () => R_Value)
                        {
                            SymbolSubscriptText = $"{LayerNumber}b"
                        },
                        new PropertyItem<double>(Symbol.AreaMassDensity, () => AreaMassDensity),
                        new PropertyItem<double>(Symbol.SdThickness, () => Sd_Thickness) { DecimalPlaces = 1 },
                        new PropertyItem<double>(Symbol.ArealHeatCapacity, () => ArealHeatCapacity)
                        {
                            SymbolSubscriptText = $"{LayerNumber}b"
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
                    _material = DatabaseManager.QueryMaterialById(MaterialId);
                    ReloadMaterial = false;
                }
                return _material;
            }
        }

        [JsonIgnore]
        public bool IsValid => Width > 0 && Thickness > 0 && Spacing > 0 && !Material.IsNewEmptyMaterial;
        
        [JsonIgnore]
        public double AxisSpacing
        {
            get => Spacing + Width;
            set => Spacing = value - Width;
        }

        [JsonIgnore]
        public bool IsSelected { get; set; } // For UI Purposes
        [JsonIgnore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [JsonIgnore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);
        
        [JsonIgnore]
        public bool IsEffective { get; set; } = true;

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
                if (!Material.IsValid) return 0;
                return Math.Round((this.Thickness / 100) * Material.DiffusionResistance, 3);
            }
        }

        [JsonIgnore]
        public double AreaMassDensity // m' in kg/m²
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                var partialAreaOfSubConstr = this.Width / (this.Width + this.Spacing);
                return Math.Round((this.Thickness / 100) * Material.BulkDensity * partialAreaOfSubConstr, 3);
            }
        }

        [JsonIgnore]
        public double ArealHeatCapacity // C_i in kJ/m²K 
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                var partialAreaOfSubConstr = this.Width / (this.Width + this.Spacing);
                return (this.Thickness / 100) * Material.VolumetricHeatCapacity * partialAreaOfSubConstr;
            }
        }

        #endregion

        #region Public Methods

        public LayerSubConstruction Copy()
        {
            var copy = new LayerSubConstruction();
            copy.Width = this.Width;
            copy.Thickness = this.Thickness;
            copy.Spacing = this.Spacing;
            copy.Direction = this.Direction;
            copy.MaterialId = this.MaterialId;
            copy.IsEffective = this.IsEffective;
            copy.IsSelected = false;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.LayerNumber = this.LayerNumber;
            return copy;
        }
        public void CopyToNewLayer(Layer layer)
        {
            layer.SubConstruction = Copy();
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }

        public override string ToString() // Überlagert vererbte standard ToString() Methode 
        {
            return Width.ToString(CultureInfo.CurrentCulture) + " x " + Thickness.ToString(CultureInfo.CurrentCulture) + " cm " + Material.Name + ", Abstand: " + Spacing.ToString(CultureInfo.CurrentCulture) + " cm";
        }

        public void RefreshMaterial()
        {
            ReloadMaterial = true;
        }

        public void RefreshPropertyBag()
        {
            ReloadPropertyBag = true;
        }


        #endregion
    }

    /// <summary>
    /// Presentation logic of a LayerSubConstruction which can be drawn on a XAML Canvas
    /// </summary>
    public partial class LayerSubConstruction : IDrawingGeometry
    {
        #region IDrawingGeometry
        [JsonIgnore]
        public Rectangle Rectangle { get; set; } = Rectangle.Empty;
        [JsonIgnore]
        public Pen BorderPen { get; set; } = Pens.GetSolidPen(Brushes.Black, 1.0);
        [JsonIgnore]
        public Brush BackgroundColor { get; set; } = Brushes.Transparent;
        [JsonIgnore]
        public Brush? TextureBrush { get; set; } = new DrawingBrush();
        [JsonIgnore]
        public double Opacity { get; set; } = 1;
        [JsonIgnore]
        public int ZIndex { get; set; } = 1;
        [JsonIgnore]
        public object Tag { get; set; } = new object();
        [JsonIgnore]
        public int? TextureId { get; set; }
        [JsonIgnore]
        public ShapeId ShapeId { get; set; }
        [JsonIgnore]
        public HatchFitMode HatchFitMode { get; set; } = HatchFitMode.OriginalPixelSize;

        public IDrawingGeometry CopyGeometry()
        {
            return new DrawingGeometry(this);
        }

        #endregion
    }
}

