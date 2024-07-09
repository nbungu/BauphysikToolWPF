using SQLite;
using System.Windows.Media;

namespace BauphysikToolWPF.Models
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
        public int Id { get; set; }
        [NotNull, Unique]
        public string Name { get; set; } = string.Empty;
        [NotNull]
        public MaterialCategory Category { get; set; } = MaterialCategory.None;
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
        public int InternalId { get; set; }

        [Ignore]
        public bool IsValid => BulkDensity > 0 && ThermalConductivity > 0;

        [Ignore]
        public string CategoryName => TranslateToCategoryName();

        [Ignore]
        public Color Color // HEX ColorCode (e.g. #dddddd) to 'Color' Type
        {
            get
            {
                if (ColorCode == "#00FFFFFF") return Colors.Transparent;
                return (Color)ColorConverter.ConvertFromString(ColorCode);
            }
        }

        //------Methoden-----//

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return this.Name + " (" + this.CategoryName + ")";
        }

        private string TranslateToCategoryName()
        {
            switch (Category)
            {
                case MaterialCategory.Insulation:
                    return "Wärmedämmung";
                case MaterialCategory.Concrete:
                    return "Beton";
                case MaterialCategory.Wood:
                    return "Holz";
                case MaterialCategory.Plasters:
                    return "Mörtel und Putze";
                case MaterialCategory.Sealant:
                    return "Dichtbahnen, Folien";
                case MaterialCategory.Air:
                    return "Luftschicht";
                case MaterialCategory.Masonry:
                    return "Mauerwerk";
                case MaterialCategory.UserDefined:
                    return "Benutzerdefiniert";
                default:
                    return "Ohne";
            }
        }
    }
}
