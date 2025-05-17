using BauphysikToolWPF.Services.Application;
using SQLite;
using System;
using System.Windows.Media;
using static BauphysikToolWPF.Models.Database.Helper.Enums;

namespace BauphysikToolWPF.Models.Database
{
    public class Material : IDatabaseObject<Material>, IEquatable<Material>
    {
        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; } = -1; // -1 means: Is not part of Database yet
        [NotNull, Unique]
        public string Name { get; set; } = string.Empty;
        [NotNull]
        public MaterialCategory MaterialCategory { get; set; } = MaterialCategory.NotDefined;
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
        [NotNull]
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        [NotNull]
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        //------Not part of the Database-----//

        [Ignore]
        public static Material Empty => new Material(); // Optional static default (for easy reference)

        [Ignore]
        public bool IsNewEmptyMaterial => this.Equals(Material.Empty);

        [Ignore]
        public bool IsValid => BulkDensity > 0 && Name != "";

        [Ignore]
        public string CategoryName => MaterialCategoryMapping[MaterialCategory];

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
        public double VolumetricHeatCapacity // C in kJ/m³K
        {
            get
            {
                if (!IsValid) return 0;
                return (SpecificHeatCapacity * BulkDensity) / 1000;
            }
        }
        
        #region public methods

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return this.Name + " (" + this.CategoryName + ")";
        }

        public Material Copy()
        {
            var copy = new Material();
            copy.Id = -1;
            copy.Name = this.Name;
            copy.MaterialCategory = this.MaterialCategory;
            copy.IsUserDefined = this.IsUserDefined;
            copy.BulkDensity = this.BulkDensity;
            copy.ThermalConductivity = this.ThermalConductivity;
            copy.DiffusionResistance = this.DiffusionResistance;
            copy.ColorCode = this.ColorCode;
            copy.SpecificHeatCapacity = this.SpecificHeatCapacity;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            return copy;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }

        #endregion

        #region IEquatable<Material> Implementation

        public bool Equals(Material? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Name == other.Name && MaterialCategory == other.MaterialCategory && IsUserDefined == other.IsUserDefined && BulkDensity == other.BulkDensity && ThermalConductivity.Equals(other.ThermalConductivity) && DiffusionResistance.Equals(other.DiffusionResistance) && SpecificHeatCapacity == other.SpecificHeatCapacity;
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
            return HashCode.Combine(Name, (int)MaterialCategory, IsUserDefined, BulkDensity, ThermalConductivity, DiffusionResistance, ColorCode, SpecificHeatCapacity);
        }

        #endregion
    }
}
