using System;
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
    public class Material : IEquatable<Material>
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
        public int BulkDensity { get; set; } // kg/m³
        [NotNull]
        public double ThermalConductivity { get; set; } // W/(mK)
        [NotNull]
        public double DiffusionResistance { get; set; } // -
        [NotNull]
        public int SpecificHeatCapacity { get; set; } // J/(kg·K)
        [NotNull]
        public string ColorCode { get; set; } = "#00FFFFFF";

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

        [Ignore, JsonIgnore]
        public double VolumetricHeatCapacity // C in kJ/m³K
        {
            get
            {
                if (!IsValid) return 0;
                return (SpecificHeatCapacity * BulkDensity) / 1000;
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
        public static double DefaultLayerWidthForCategory(MaterialCategory category)
        {
            switch (category)
            {
                case MaterialCategory.Insulation:
                    return 16.0;
                case MaterialCategory.Concrete:
                    return 24.0;
                case MaterialCategory.Wood:
                    return 4.8;
                case MaterialCategory.Plasters:
                    return 1.0;
                case MaterialCategory.Sealant:
                    return 0.01;
                case MaterialCategory.Air:
                    return 4.0;
                case MaterialCategory.Masonry:
                    return 24.0;
                default:
                    return 6.0;
            }
        }

        public bool Equals(Material? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Category == other.Category && IsUserDefined == other.IsUserDefined && BulkDensity == other.BulkDensity && ThermalConductivity.Equals(other.ThermalConductivity) && DiffusionResistance.Equals(other.DiffusionResistance) && ColorCode == other.ColorCode && SpecificHeatCapacity == other.SpecificHeatCapacity;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Material)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, (int)Category, IsUserDefined, BulkDensity, ThermalConductivity, DiffusionResistance, ColorCode, SpecificHeatCapacity);
        }
    }
}
