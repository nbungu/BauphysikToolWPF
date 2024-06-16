using System;
using BauphysikToolWPF.Repository;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.Models
{
    public class Layer
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int LayerId { get; set; }

        [NotNull]
        public int LayerPosition { get; set; } // Inside = 1

        [ForeignKey(typeof(Element))] // FK for the n:1 relationship with Element
        public int ElementId { get; set; }

        [ForeignKey(typeof(Material))] // FK for the 1:1 relationship with Material
        public int MaterialId { get; set; }

        [NotNull]
        public double LayerThickness { get; set; } // Layer thickness in cm

        [NotNull]
        public int Effective { get; set; } // For Calculation Purposes - Whether a Layer is considered in the Calculations or not

        //------Not part of the Database-----//

        [Ignore]
        public int InternalId { get; set; }

        // n:1 relationship with Element
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Element Element { get; set; } = new Element();

        // 1:1 relationship with Material
        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Material Material { get; set; } = new Material();

        [Ignore]
        public bool IsSelected { get; set; } // For UI Purposes 

        // Encapsulate/Hide 'Effective' to convert to bool
        [Ignore]
        public bool IsEffective // true = 1
        {
            get => Effective != 0;
            set => Effective = (value) ? 1 : 0;
        }

        [Ignore]
        public double R_Value
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                return Math.Round((this.LayerThickness / 100) / Material.ThermalConductivity, 3);
            }
        }

        [Ignore]
        public double Sd_Thickness // sd thickness in m
        {
            get
            {
                if (!Material.IsValid) return 0;
                return Math.Round((this.LayerThickness / 100) * Material.DiffusionResistance, 3);
            }
        }

        [Ignore]
        public double AreaMassDensity // m' in kg/m²
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                return Math.Round(this.LayerThickness / 100 * Material.BulkDensity, 3);
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
                return Math.Round(Material.ThermalConductivity / Convert.ToDouble(Material.BulkDensity / Material.SpecificHeatCapacity), 2);
            }
        }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public Material CorrespondingMaterial()
        {
            return DatabaseAccess.GetMaterials().Find(m => m.MaterialId == this.MaterialId) ?? new Material();
        }

        public Layer Copy()
        {
            var copy = new Layer();
            copy.LayerId = this.LayerId;
            copy.LayerPosition = this.LayerPosition;
            copy.Element = this.Element;
            copy.MaterialId = this.MaterialId;
            copy.Element = this.Element;
            copy.Material = this.Material;
            copy.LayerThickness = this.LayerThickness;
            copy.Effective = this.Effective;
            copy.IsSelected = false;
            copy.InternalId = this.InternalId;
            return copy;
        }

        public override string ToString() // Überlagert vererbte standard ToString() Methode 
        {
            return LayerThickness + " cm, " + Material.Name + " (Pos.: " + LayerPosition + ")";
        }
    }
}
