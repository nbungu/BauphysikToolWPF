using BauphysikToolWPF.Services;
using BauphysikToolWPF.UI.Drawing;
using Geometry;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Windows.Media;

namespace BauphysikToolWPF.Models
{
    /// <summary>
    /// Business logic of a Layer
    /// </summary>
    public partial class Layer : IDrawingGeometry
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; }

        [NotNull]
        public int LayerPosition { get; set; } // Inside = 1

        [NotNull, ForeignKey(typeof(Element))] // FK for the n:1 relationship with Element
        public int ElementId { get; set; }

        [NotNull, ForeignKey(typeof(Material))] // FK for the 1:1 relationship with Material
        public int MaterialId { get; set; }

        [ForeignKey(typeof(LayerSubConstruction))] // FK for the 1:1 relationship with LayerSubConstruction
        public int? SubConstructionId { get; set; } = null;

        [NotNull]
        public double Thickness { get; set; } // Layer thickness in cm

        [NotNull]
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        [NotNull]
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();


        //------Not part of the Database-----//

        [Ignore]
        public int InternalId { get; set; }

        // n:1 relationship with Element
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Element Element { get; set; } = new Element();

        // 1:1 relationship with Material
        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Material Material { get; set; } = new Material();

        // 1:1 relationship with LayerSubConstruction
        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public LayerSubConstruction? SubConstruction { get; set; }
        
        [Ignore]
        public bool IsSelected { get; set; } // For UI Purposes 

        [Ignore]
        public bool IsEffective { get; set; } = true;

            // Readonly Properties

        [Ignore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [Ignore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);
        [Ignore]
        public bool HasSubConstruction => SubConstruction != null && SubConstruction.IsValid;
        [Ignore]
        public bool IsValid => LayerPosition >= 0 && Thickness > 0 && Material.IsValid;

        [Ignore]
        public double R_Value
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                return Math.Round((this.Thickness / 100) / Material.ThermalConductivity, 3);
            }
        }

        [Ignore]
        public double Sd_Thickness // sd thickness in m
        {
            get
            {
                if (!Material.IsValid) return 0;
                return Math.Round((this.Thickness / 100) * Material.DiffusionResistance, 3);
            }
        }

        [Ignore]
        public double AreaMassDensity // m' in kg/m²
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                return Math.Round(this.Thickness / 100 * Material.BulkDensity, 3);
            }
        }

        // Temperaturleitfähigkeit a gibt das Mass für die Geschwindigkeit an,
        // mit der sich eine Temperaturänderung im Material ausbreitet:
        [Ignore]
        public double TemperatureConductivity // a in m²/s
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                return Math.Round(Material.ThermalConductivity / System.Convert.ToDouble(Material.BulkDensity / Material.SpecificHeatCapacity), 2);
            }
        }

        //------Konstruktor-----//


        //------Methoden-----//
        public Layer Copy()
        {
            var copy = new Layer();
            copy.Id = this.Id;
            copy.LayerPosition = this.LayerPosition;
            copy.Element = this.Element;
            copy.MaterialId = this.MaterialId;
            copy.Element = this.Element;
            copy.Material = this.Material;
            copy.SubConstructionId = this.SubConstructionId;
            copy.SubConstruction = this.SubConstruction;
            copy.Thickness = this.Thickness;
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
            return Thickness + " cm, " + Material.Name + " (Pos.: " + LayerPosition + ")";
        }

        public void RemoveSubConstruction()
        {
            this.SubConstructionId = null;
            this.SubConstruction = null;
        }
    }

    /// <summary>
    /// Presentation logic of a Layer which can be drawn on a XAML Canvas
    /// </summary>
    public partial class Layer : IDrawingGeometry
    {
        #region IDrawingGeometry
        [Ignore]
        public Rectangle Rectangle { get; set; } = Rectangle.Empty;
        [Ignore]
        public Brush RectangleBorderColor { get; set; } = Brushes.Black;
        [Ignore]
        public double RectangleBorderThickness { get; set; } = 0.2;
        [Ignore]
        public DoubleCollection RectangleStrokeDashArray { get; set; } = new DoubleCollection();
        [Ignore]
        public Brush BackgroundColor { get; set; } = Brushes.Transparent;
        [Ignore]
        public Brush DrawingBrush { get; set; } = new DrawingBrush();
        [Ignore]
        public double Opacity { get; set; } = 1;
        [Ignore]
        public int ZIndex { get; set; } = 0;
        [Ignore]
        public object Tag { get; set; }

        public IDrawingGeometry Convert()
        {
            return new DrawingGeometry(this);
        }

        #endregion
    }
}
