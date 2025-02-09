using System;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Windows.Media;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.UI.Models;
using BT.Geometry;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.Repository.Models
{
    public enum SubConstructionDirection
    {
        Vertical,
        Horizontal
    }

    /// <summary>
    /// Business logic of a LayerSubConstruction
    /// </summary>
    public partial class LayerSubConstruction : IDrawingGeometry
    {
        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; } = -1; // -1 means: Is not part of Database yet
        [NotNull]
        public double Width { get; set; } // cm
        [NotNull]
        public double Thickness { get; set; } // cm
        [NotNull]
        public double Spacing { get; set; } // cm (innerer Abstand)
        [NotNull]
        public SubConstructionDirection Direction { get; set; }
        [NotNull]
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        [NotNull]
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        [NotNull, ForeignKey(typeof(Material))] // FK for the 1:1 relationship with Material
        public int MaterialId { get; set; }
        [NotNull, ForeignKey(typeof(Layer))] // FK for the 1:n relationship with Layer
        public int LayerId { get; set; }


        //------Not part of the Database-----//

        [Ignore, JsonIgnore]
        public int InternalId { get; set; }
        [Ignore, JsonIgnore]
        public bool IsValid => Width > 0 && Thickness > 0 && Spacing > 0 && Material != null;


        [Ignore, JsonIgnore]
        public double AxisSpacing
        {
            get => Spacing + Width;
            set => Spacing = value - Width;
        }

        [Ignore, JsonIgnore]
        public bool IsSelected { get; set; } // For UI Purposes
        [Ignore, JsonIgnore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [Ignore, JsonIgnore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);

        // 1:1 relationship with Material
        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Material Material { get; set; } = new Material();
        // relationship with Layer
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead), JsonIgnore]
        public Layer Layer { get; set; } = new Layer();

        [Ignore, JsonIgnore]
        public bool IsEffective { get; set; } = true;

        [Ignore, JsonIgnore]
        public double R_Value
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                return Math.Round((this.Thickness / 100) / Material.ThermalConductivity, 3);
            }
        }
        [Ignore, JsonIgnore]
        public double Sd_Thickness // sd thickness in m
        {
            get
            {
                if (!Material.IsValid) return 0;
                return Math.Round((this.Thickness / 100) * Material.DiffusionResistance, 3);
            }
        }

        [Ignore, JsonIgnore]
        public double AreaMassDensity // m' in kg/m²
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                var partialAreaOfSubConstr = this.Width / (this.Width + this.Spacing);
                return Math.Round((this.Thickness / 100) * Material.BulkDensity * partialAreaOfSubConstr, 3);
            }
        }

        [Ignore, JsonIgnore]
        public double ArealHeatCapacity // C_i in kJ/m²K 
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                var partialAreaOfSubConstr = this.Width / (this.Width + this.Spacing);
                return (this.Thickness / 100) * Material.VolumetricHeatCapacity * partialAreaOfSubConstr;
            }
        }

        //------Methods-----//

        public LayerSubConstruction Copy()
        {
            var copy = new LayerSubConstruction();
            copy.Id = -1;
            copy.Width = this.Width;
            copy.Thickness = this.Thickness;
            copy.Spacing = this.Spacing;
            copy.Direction = this.Direction;
            copy.MaterialId = this.MaterialId;
            copy.Material = this.Material; // TODO Check: Keep Reference, No Deep Copy
            copy.LayerId = this.LayerId;
            copy.Layer = this.Layer.Copy(); // TODO Check: Deep Copy
            copy.IsEffective = this.IsEffective;
            copy.IsSelected = false;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.InternalId = this.InternalId;
            return copy;
        }
        public LayerSubConstruction CopyToNewLayer(Layer layer)
        {
            var copy = new LayerSubConstruction();
            copy.Id = -1;
            copy.Width = this.Width;
            copy.Thickness = this.Thickness;
            copy.Spacing = this.Spacing;
            copy.Direction = this.Direction;
            copy.MaterialId = this.MaterialId;
            copy.Material = this.Material; // TODO Check: Keep Reference, No Deep Copy
            copy.LayerId = layer.Id;
            copy.Layer = layer;
            copy.IsEffective = this.IsEffective;
            copy.IsSelected = false;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.InternalId = this.InternalId;
            return copy;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }

        public override string ToString() // Überlagert vererbte standard ToString() Methode 
        {
            return Width.ToString(CultureInfo.CurrentCulture) + " x " + Thickness.ToString(CultureInfo.CurrentCulture) + " cm " + Material.Name + ", Abstand: " + Spacing.ToString(CultureInfo.CurrentCulture) + " cm";
        }
    }

    /// <summary>
    /// Presentation logic of a LayerSubConstruction which can be drawn on a XAML Canvas
    /// </summary>
    public partial class LayerSubConstruction : IDrawingGeometry
    {
        #region IDrawingGeometry
        [Ignore, JsonIgnore]
        public Rectangle Rectangle { get; set; } = Rectangle.Empty;
        [Ignore, JsonIgnore]
        public Brush RectangleBorderColor { get; set; } = Brushes.Black;
        [Ignore, JsonIgnore]
        public double RectangleBorderThickness { get; set; } = 0.2;
        [Ignore, JsonIgnore]
        public DoubleCollection RectangleStrokeDashArray { get; set; } = new DoubleCollection();
        [Ignore, JsonIgnore]
        public Brush BackgroundColor { get; set; } = Brushes.Transparent;
        [Ignore, JsonIgnore]
        public Brush DrawingBrush { get; set; } = new DrawingBrush();
        [Ignore, JsonIgnore]
        public double Opacity { get; set; } = 1;
        [Ignore, JsonIgnore]
        public int ZIndex { get; set; } = 1;
        [Ignore, JsonIgnore]
        public object Tag { get; set; } = new object();

        public IDrawingGeometry Convert()
        {
            return new DrawingGeometry(this);
        }

        #endregion
    }
}

