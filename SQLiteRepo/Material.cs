﻿using SQLite;
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
        Air
    }
    public class Material
    {
        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int MaterialId { get; set; }
        [NotNull, Unique]
        public string Name { get; set; }
        [NotNull]
        public string CategoryName { get; set; }
        [NotNull]
        public int BulkDensity { get; set; }
        [NotNull]
        public double ThermalConductivity { get; set; }
        [NotNull]
        public double DiffusionResistance { get; set; }
        public string? ColorCode { get; set; }
        public double? Porosity { get; set; }
        public int? SpecificHeatCapacity { get; set; }

        //------Not part of the Database-----//

        [Ignore]
        public Color Color
        {
            get
            {
                if (ColorCode == null)
                    return Colors.Transparent;
                return (Color)ColorConverter.ConvertFromString(ColorCode);
            }
        }

        [Ignore]
        public MaterialCategory Category
        {
            get
            {
                if (CategoryName == null)
                    return MaterialCategory.None;

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
