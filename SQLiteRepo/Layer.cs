using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace BauphysikToolWPF.SQLiteRepo
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

        //------Not part of the Database-----//

        // n:1 relationship with Element
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Element Element { get; set; }

        // 1:1 relationship with Material
        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Material Material { get; set; }

        [Ignore]
        public bool IsSelected { get; set; } // For UI Purposes 

        [Ignore]
        public double R_Value
        {
            get
            {
                if (Material == null)
                    return 0;
                return Math.Round((this.LayerThickness / 100) / Material.ThermalConductivity, 3);
            }
        }

        [Ignore]
        public double Sd_Thickness // sd thickness in m
        {
            get
            {
                if (Material == null)
                    return 0;
                return Math.Round((this.LayerThickness / 100) * Material.DiffusionResistance, 3);
            }
        }

        [Ignore]
        public double AreaMassDensity // m' in kg/m²
        {
            get
            {
                if (Material == null)
                    return 0;
                return Math.Round(this.LayerThickness / 100 * Material.BulkDensity, 3);
            }
        }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public Material correspondingMaterial()
        {
            return DatabaseAccess.GetMaterials().Find(m => m.MaterialId == this.MaterialId);
        }

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return LayerThickness + " cm, "+ Material.Name + " (Pos.: " + LayerPosition + ")";
        }
    }
}
