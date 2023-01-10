using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace BauphysikToolWPF.SQLiteRepo
{
    //https://bitbucket.org/twincoders/sqlite-net-extensions/src/master/
    public class Layer 
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique] //SQL Attributes
        public int LayerId { get; set; }

        [NotNull] //SQL Attributes
        public int LayerPosition { get; set; } //Inside = 1 ....

        [SQLiteNetExtensions.Attributes.ForeignKey(typeof(Material))]
        public int MaterialId { get; set; } // Used Material specified by ID

        [NotNull]
        public double LayerThickness { get; set; }  // Layer thickness in cm

        //------Not part of the Database-----//

        [OneToOne] //relationships and the ForeignKey property will be discovered and updated automatically at runtime.
        public Material Material { get; set; } // the corresp. object/Type for the foreign-key. The 'Material' object itself is not stored in DB!

        [Ignore] 
        public bool IsSelected { get; set; } // For UI Purposes 

        [Ignore]
        public double R_Value
        {
            get { return Math.Round((this.LayerThickness/100) / Material.ThermalConductivity, 3); }
        }

        [Ignore]
        public double Sd_Thickness // sd thickness in m
        {
            get { return Math.Round((this.LayerThickness/100) * Material.DiffusionResistance, 3); }
        }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public Material correspondingMaterial() {
            return DatabaseAccess.GetMaterials().Find(m => m.MaterialId == this.MaterialId);
        }
    }
}
