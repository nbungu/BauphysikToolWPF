using BauphysikToolWPF.Services;
using BauphysikToolWPF.UI.Drawing;
using Geometry;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
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
        public int Id { get; set; } = -1; // -1 means: Is not part of Database yet

        [NotNull]
        public int LayerPosition { get; set; } // Inside = 1

        [NotNull, ForeignKey(typeof(Element))] // FK for the n:1 relationship with Element
        public int ElementId { get; set; }

        [NotNull, ForeignKey(typeof(Material))] // FK for the 1:1 relationship with Material
        public int MaterialId { get; set; }

        [NotNull]
        public double Thickness { get; set; } // Layer thickness in cm

        [NotNull]
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        [NotNull]
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        //------Not part of the Database-----//

        [Ignore, JsonIgnore]
        public int InternalId { get; set; }

        // n:1 relationship with Element
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead), JsonIgnore]
        public Element Element { get; set; } = new Element();

        // 1:1 relationship with Material
        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Material Material { get; set; } = new Material();

        // 1:n relationship with LayerSubConstruction
        [OneToMany(CascadeOperations = CascadeOperation.All)] // ON DELETE CASCADE (When parent Element is removed: Deletes all SubConstructions linked to this 'Layer')
        public List<LayerSubConstruction> SubConstructions { get; set; } = new List<LayerSubConstruction>(1);
        
        [Ignore, JsonIgnore]
        public bool IsSelected { get; set; } // For UI Purposes 

        [Ignore, JsonIgnore]
        public bool IsEffective { get; set; } = true;

        // Readonly Properties

        [Ignore, JsonIgnore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [Ignore, JsonIgnore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);
        [Ignore, JsonIgnore]
        public bool HasSubConstructions => SubConstructions.Count > 0;
        [Ignore, JsonIgnore]
        public bool IsValid => LayerPosition >= 0 && Thickness > 0 && Material.IsValid;

        // Workaround: Currently only 1 SubConstruction supported
        [Ignore, JsonIgnore]
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
                if (HasSubConstructions)
                {
                    var partialAreaOfLayer = 1 - (SubConstruction.Width / (SubConstruction.Width + SubConstruction.Spacing));
                    return Math.Round((this.Thickness / 100) * Material.BulkDensity * partialAreaOfLayer, 3);
                }
                return Math.Round((this.Thickness / 100) * Material.BulkDensity, 3);
            }
        }

        [Ignore, JsonIgnore]
        public double ArealHeatCapacity // C_i in kJ/m²K 
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                if (HasSubConstructions)
                {
                    var partialAreaOfLayer = 1 - (SubConstruction.Width / (SubConstruction.Width + SubConstruction.Spacing));
                    return (this.Thickness / 100) * Material.VolumetricHeatCapacity * partialAreaOfLayer;
                } 
                return (this.Thickness / 100) * Material.VolumetricHeatCapacity;
            }
        }

        // Temperaturleitfähigkeit a gibt das Mass für die Geschwindigkeit an,
        // mit der sich eine Temperaturänderung im Material ausbreitet:
        [Ignore, JsonIgnore]
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
            copy.Id = -1;
            copy.LayerPosition = this.LayerPosition;
            copy.ElementId = this.ElementId;
            copy.Element = this.Element;
            copy.MaterialId = this.MaterialId;
            copy.Material = this.Material;
            copy.SubConstructions = this.SubConstructions;
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
            this.SubConstructions.Clear();
        }
    }

    /// <summary>
    /// Presentation logic of a Layer which can be drawn on a XAML Canvas
    /// </summary>
    public partial class Layer : IDrawingGeometry
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
        public int ZIndex { get; set; } = 0;
        [Ignore, JsonIgnore]
        public object Tag { get; set; }

        public IDrawingGeometry Convert()
        {
            return new DrawingGeometry(this);
        }

        #endregion
    }
}
