using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using static BauphysikToolWPF.Models.Database.Helper.Enums;

namespace BauphysikToolWPF.Models.Database
{
    public class Construction : IDatabaseObject<Construction>
    {
        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public int Id { get; set; }
        [NotNull]
        public ConstructionType ConstructionType { get; set; }
        [NotNull]
        public string TypeName { get; set; } = string.Empty;
        [NotNull]
        public ConstructionDirection ConstructionDirection { get; set; }
        [NotNull, ForeignKey(typeof(DocumentSource))] // FK for the n:1 relationship with DocumentSource
        public int DocumentSourceId { get; set; }
        [NotNull]
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        [NotNull]
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        //------Not part of the Database-----//

        // n:1 relationship with DocumentSource
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public DocumentSource DocumentSource => DatabaseAccess.QueryDocumentSourceById(DocumentSourceId);

        // m:n relationship with Requirement
        [ManyToMany(typeof(ConstructionDocumentParameter), CascadeOperations = CascadeOperation.CascadeRead)]
        public List<DocumentParameter> Requirements => DatabaseAccess.QueryDocumentParameterByDocumentId(DocumentSourceId);

        [Ignore]
        public bool IsLayoutVertical => (int)ConstructionDirection == 1;


        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return TypeName + " (Id: " + Id + ")";
        }

        public Construction Copy()
        {
            var copy = new Construction();
            copy.Id = -1;
            copy.ConstructionType = this.ConstructionType;
            copy.TypeName = this.TypeName;
            copy.ConstructionDirection = this.ConstructionDirection;
            copy.DocumentSourceId = this.DocumentSourceId;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            return copy;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }
    }
}
