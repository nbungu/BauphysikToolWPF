using SQLite;
using System.Windows.Media;

namespace BauphysikToolWPF.SQLiteRepo
{
    public enum MaterialCategory
    {
        None,
        Insulation,
        Concrete,
        Wood,
        Masonry,
        Plasters,
        Sealant,
        Air,
        UserDefined
    }
    public class Material
    {
        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int MaterialId { get; set; }
        [NotNull, Unique]
        public string Name { get; set; } = string.Empty;
        [NotNull]
        public string CategoryName { get; set; } = string.Empty;
        [NotNull]
        public int BulkDensity { get; set; }
        [NotNull]
        public double ThermalConductivity { get; set; }
        [NotNull]
        public double DiffusionResistance { get; set; }
        [NotNull]
        public string ColorCode { get; set; } = "#00FFFFFF";
        [NotNull]
        public int SpecificHeatCapacity { get; set; }

        //------Not part of the Database-----//

        [Ignore]
        public bool IsValid => BulkDensity > 0 && ThermalConductivity > 0;

        [Ignore]
        public Color Color // HEX ColorCode (e.g. #dddddd) to 'Color' Type
        {
            get
            {
                if (ColorCode == "#00FFFFFF") return Colors.Transparent;
                return (Color)ColorConverter.ConvertFromString(ColorCode);
            }
        }

        [Ignore]
        public MaterialCategory Category
        {
            get
            {
                switch (CategoryName)
                {
                    case "Wärmedämmung":
                        return MaterialCategory.Insulation;
                    case "Beton":
                        return MaterialCategory.Concrete;
                    case "Holz":
                        return MaterialCategory.Wood;
                    case "Mörtel und Putze":
                        return MaterialCategory.Plasters;
                    case "Dichtbahnen, Folien":
                        return MaterialCategory.Sealant;
                    case "Luftschicht":
                        return MaterialCategory.Air;
                    case "Mauerwerk":
                        return MaterialCategory.Masonry;
                    case "Benutzerdefiniert":
                        return MaterialCategory.UserDefined;
                    default:
                        return MaterialCategory.None;
                }
            }
        }

        //------Methoden-----//

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return this.Name + " (" + this.CategoryName + ")";
        }
    }
}
