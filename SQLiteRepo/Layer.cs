using SQLite;
using SQLiteNetExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.SQLiteRepo
{
    //https://bitbucket.org/twincoders/sqlite-net-extensions/src/master/
    public class Layer 
    {
        //------Variablen-----//


        //------Eigenschaften-----//
        [NotNull, PrimaryKey, AutoIncrement, Unique] //SQL Attributes
        public int LayerPosition { get; set; } //Inside = 1 ....

        [SQLiteNetExtensions.Attributes.ForeignKey(typeof(Material))]
        public int MaterialId { get; set; } // Used Material specified by ID

        [NotNull]
        public double LayerThickness { get; set; }  // Layer thickness in cm

        
        [OneToOne]
        //relationships and the ForeignKey property will be discovered and updated automatically at runtime.
        //stellt die 1:1 relationship her und fügt das entsprechende Material beim Get/Read-Vorgang hier ein: vgl. zu "correspondingMaterial()" function
        public Material Material { get; set; } // the corresp. object/Type for the foreign-key

        // Not part of the Database: Ignore
        [Ignore]
        public bool IsSelected { get; set; } // For UI Purposes 

        [Ignore]
        public double LayerResistance
        {
            get { return Math.Round((this.LayerThickness/100)/correspondingMaterial().ThermalConductivity, 3); }
        }

        [Ignore]
        public string LayerName
        { 
            get { return correspondingMaterial().Name; }
        }

        [Ignore]
        public string LayerCategory
        {
            get { return correspondingMaterial().Category; }
        }

        [Ignore]
        public string LayerColor
        {
            get { return correspondingMaterial().ColorCode; }
        }


        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public Material correspondingMaterial() {
            return DatabaseAccess.GetMaterials().Find(m => m.MaterialId == this.MaterialId);
        }
    }
}
