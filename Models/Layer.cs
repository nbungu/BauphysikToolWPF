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

        [ForeignKey(typeof(Element))] // FK for the n:1 relationship with Element
        public int ElementId { get; set; }

        [ForeignKey(typeof(Material))] // FK for the 1:1 relationship with Material
        public int MaterialId { get; set; }

        [ForeignKey(typeof(LayerSubConstruction))] // FK for the 1:1 relationship with LayerSubConstruction
        public int SubConstructionId { get; set; }

        [NotNull]
        public double Thickness { get; set; } // Layer thickness in cm

        [NotNull]
        public int Effective { get; set; } // For Calculation Purposes - Whether a Layer is considered in the Calculations or not

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

        // Encapsulate/Hide 'Effective' to convert to bool
        [Ignore]
        public bool IsEffective // true = 1
        {
            get => Effective != 0;
            set => Effective = (value) ? 1 : 0;
        }

        // Readonly Properties

        [Ignore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [Ignore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);
        [Ignore]
        public bool HasSubConstruction => SubConstruction != null && SubConstruction.IsValid;
        [Ignore]
        public bool IsValid => LayerPosition >= 0 && Thickness > 0;

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
            copy.Thickness = this.Thickness;
            copy.Effective = this.Effective;
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
            this.SubConstructionId = -1;
            this.SubConstruction = null;
        }
    }

    /// <summary>
    /// Presentation logic of a Layer which can be drawn on a XAML Canvas
    /// </summary>
    public partial class Layer : IDrawingGeometry
    {
        #region IDrawingGeometry

        public Rectangle Rectangle { get; set; } = Rectangle.Empty;
        public Brush RectangleBorderColor { get; set; } = Brushes.Black;
        public double RectangleBorderThickness { get; set; } = 0.2;
        public Brush BackgroundColor { get; set; } = Brushes.Transparent;
        public Brush DrawingBrush { get; set; } = new DrawingBrush();
        public double Opacity { get; set; } = 1;
        public int ZIndex { get; set; } = 0;
        public object Tag { get; set; }

        public IDrawingGeometry Convert()
        {
            return new DrawingGeometry(this);
        }
        public void UpdateGeometry()
        {
            var initWidth = 100; // cm
            var initHeight = this.Thickness; // cm

            Rectangle = new Rectangle(new Point(0, 0), initWidth, initHeight);
            BackgroundColor = new SolidColorBrush(this.Material.Color);
            DrawingBrush = HatchPattern.GetHatchPattern(this.Material.Category, 0.5, initWidth, initHeight);
            RectangleBorderColor = this.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
            RectangleBorderThickness = this.IsSelected ? 2 : 0.2;
            Opacity = this.IsEffective ? 1 : 0.2;
        }

        #endregion
    }



}
