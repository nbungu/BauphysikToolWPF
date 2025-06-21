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
        public ConstructionType ConstructionType { get; set; } = ConstructionType.NotDefined;

        [NotNull]
        public string TypeName { get; set; } = string.Empty;

        [NotNull]
        public ConstructionDirection ConstructionDirection { get; set; } = ConstructionDirection.Vertical;

        [NotNull]
        public ConstructionGroup ConstructionGroup { get; set; } = ConstructionGroup.NotDefined;

        [NotNull]
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        [NotNull]
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        //------Not part of the Database-----//

        // m:n relationship with Requirement
        [Ignore, ManyToMany(typeof(ConstructionDocumentParameter), CascadeOperations = CascadeOperation.CascadeRead)]
        public List<DocumentParameter> Requirements { get; set; } = new List<DocumentParameter>();

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
            copy.ConstructionGroup = this.ConstructionGroup;
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
