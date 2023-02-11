using SQLite;

namespace BauphysikToolWPF.SQLiteRepo
{
    public class Material
    {
        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int MaterialId { get; set; }

        [NotNull, Unique]
        public string Name { get; set; }

        [NotNull]
        public string Category { get; set; }

        [NotNull]
        public int BulkDensity { get; set; }

        [NotNull]
        public double ThermalConductivity { get; set; }

        [NotNull]
        public double DiffusionResistance { get; set; }

        public string? ColorCode { get; set; }
        public double? Porosity { get; set; }
        public int? SpecificHeatCapacity { get; set; }

        //------Methoden-----//

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return this.Name + " (" + this.Category + ")";
        }
    }
}
