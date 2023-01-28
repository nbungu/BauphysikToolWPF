using SQLite;

namespace BauphysikToolWPF.SQLiteRepo
{
    public class Material
    {
        // Eigenschaften/Kapselung: Steuert Zugriff auf die Variablen von außerhalb der Klasse
        // Hier: Auto-implemented properties -> without any extra logic

        [NotNull, PrimaryKey, AutoIncrement, Unique] // Wenn eine DB eine Klasse als Basis für neue Tabelle nimmt
        public int MaterialId { get; set; }
        [NotNull, Unique]
        public string Name { get; set; }
        public string Category { get; set; }
        public string ColorCode { get; set; }
        public int BulkDensity { get; set; }
        public double ThermalConductivity { get; set; }
        public double Porosity { get; set; }
        public int SpecificHeatCapacity { get; set; }
        public double DiffusionResistance { get; set; }

        //------Methoden-----//

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return this.Name + " (" + this.Category + ")";
        }
    }
}
