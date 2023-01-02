using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Data.Common;

namespace BauphysikToolWPF.SQLiteRepo
{
    public class Layer //: Material //Felder und Methoden aus 'Material' werden vererbt und sind Abrufbar 
    {
        //------Variablen-----//


        //------Eigenschaften-----//
        [NotNull, PrimaryKey, AutoIncrement, Unique] //SQL Attributes
        public int LayerPosition { get; set; } //Inside = 1 ....
        [NotNull]
        public int MaterialId { get; set; }       // Used Material specified by ID
        [NotNull]
        public double LayerThickness { get; set; }  // Layer thickness in cm

        // Not part of the Database: Ignore
        [Ignore]
        public bool IsSelected { get; set; }  // For UI Purposes 
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

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public Material correspondingMaterial() {
            return DatabaseAccess.GetMaterials().Find(m => m.MaterialId == this.MaterialId);
        }
    }
}
