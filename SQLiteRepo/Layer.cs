using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace BauphysikToolWPF.SQLiteRepo
{
    //https://bitbucket.org/twincoders/sqlite-net-extensions/src/master/
    public class Layer
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique, ForeignKey(typeof(Layer))] // FK for the n:1 relation
        public int LayerId { get; set; }

        [NotNull] //SQL Attributes
        public int LayerPosition { get; set; } //Inside = 1 ....

        [ForeignKey(typeof(Material))] // FK for the 1:1 relation
        public int MaterialId { get; set; } // This Layer is made out of Material X

        [ForeignKey(typeof(Element))] // FK for the 1:n relation
        public int ElementId { get; set; } // To which Parent Element this Layer belongs    

        [NotNull]
        public double LayerThickness { get; set; }  // Layer thickness in cm

        //------Not part of the Database-----//

        [OneToOne] // 1:1 relationship with Material
        public Material Material { get; set; } // Gets the corresp. object linked by the foreign-key. The 'Material' object itself is not stored in DB!

        [ManyToOne] // n:1 relationship with Element (the parent table)
        public Element Element { get; set; } // Gets the corresp. object linked by the foreign-key. The 'Element' object itself is not stored in DB!

        [Ignore]
        public bool IsSelected { get; set; } // For UI Purposes 

        [Ignore]
        public double R_Value
        {
            get { return Math.Round((this.LayerThickness / 100) / Material.ThermalConductivity, 3); }
        }

        [Ignore]
        public double Sd_Thickness // sd thickness in m
        {
            get { return Math.Round((this.LayerThickness / 100) * Material.DiffusionResistance, 3); }
        }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public Material correspondingMaterial()
        {
            return DatabaseAccess.GetMaterials().Find(m => m.MaterialId == this.MaterialId);
        }

        /*public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return LayerThickness + " cm, "+ Material.Name + " (Pos. " + this.LayerPosition + ")";
        }*/
    }
}
