using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.Repository.Models
{
    public enum RoomUsageType
    {
        NotDefined,
        Einzelbuero,
        GrpBueroBis6Ap,
        GrpBueroAb7Ap,
        Besprechung,
        Schalterhalle,
        Einzelhandel,
        EinzelhandelMitKühlprodukten,
        Klassenzimmer,
        Hoersaal,
        Bettenzimmer,
        Hotelzimmer,
        Kantine,
        Restaurant,
        KuecheInNWG,
        KuecheLager,
        SanitaerWCInNWG,
        SonstigeAufenthalt,
        NebenflaechenOhneAufenthalt,
        Verkehrsflaechen,
        Lager,
        Rechenzentrum,
        GewerbeUndIndustrieHalleSA,
        GewerbeUndIndustrieHalleMSA,
        GewerbeUndIndustrieHalleLA,
        Zuschauerbereich,
        Theater,
        Buehne,
        Messe,
        AusstellungMuseum,
        BibLesesaal,
        BibFreihandbereich,
        BibMagazinDepot,
        Turnhalle,
        ParkhausBueroPrivatnutzung,
        ParkhausOeffentlicheNutzung,
        Saunabereich,
        Fitnessraum,
        Labor,
        UntersuchungsBehandlungsraeume,
        Spezialpflegebereiche,
        FlurAllgemeinerPflegebereich,
        ArztpraxenTherapeutischePraxen,
        LagerLogistikhalle,
    }
    
    public class RoomUsageProfile
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; }

        [NotNull]
        public RoomUsageType Usage { get; set; }

        [NotNull]
        public string UsageName { get; set; } = string.Empty;

        public string UsageDescription { get; set; } = string.Empty;

        [NotNull, ForeignKey(typeof(DocumentSource))] // FK for the n:1 relationship with DocumentSource
        public int DocumentSourceId { get; set; }

        //------Not part of the Database-----//

        // n:1 relationship with DocumentSource
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public DocumentSource DocumentSource { get; set; } = new DocumentSource();

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return UsageDescription + " (Id: " + Id + ")";
        }
    }
}
