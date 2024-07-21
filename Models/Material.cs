using System.Text.Json.Serialization;
using SQLite;
using System.Windows.Media;
using BauphysikToolWPF.Services;

namespace BauphysikToolWPF.Models
{
    public enum MaterialCategory
    {
        NotDefined,
        Insulation,
        Concrete,
        Wood,
        Masonry,
        Plasters,
        Sealant,
        Air
    }
    public class Material
    {
        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; } = -1; // -1 means: Is not part of Database yet
        [NotNull, Unique]
        public string Name { get; set; } = string.Empty;
        [NotNull]
        public MaterialCategory Category { get; set; } = MaterialCategory.NotDefined;
        [NotNull]
        public bool IsUserDefined { get; set; }
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

        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        //------Not part of the Database-----//

        [Ignore, JsonIgnore]
        public int InternalId { get; set; }

        [Ignore, JsonIgnore]
        public bool IsValid => BulkDensity > 0 && ThermalConductivity > 0;

        [Ignore, JsonIgnore]
        public string CategoryName => TranslateToCategoryName();

        [Ignore, JsonIgnore]
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

        public Material Copy()
        {
            var copy = new Material();
            copy.Id = -1;
            copy.Name = this.Name;
            copy.Category = this.Category;
            copy.IsUserDefined = this.IsUserDefined;
            copy.BulkDensity = this.BulkDensity;
            copy.ThermalConductivity = this.ThermalConductivity;
            copy.DiffusionResistance = this.DiffusionResistance;
            copy.ColorCode = this.ColorCode;
            copy.SpecificHeatCapacity = this.SpecificHeatCapacity;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.InternalId = this.InternalId;
            return copy;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
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
                default:
                    return "Ohne";
            }
        }
    }
}
