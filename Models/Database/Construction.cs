using BauphysikToolWPF.Services.Application;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using static BauphysikToolWPF.Models.Database.Helper.Enums;

namespace BauphysikToolWPF.Models.Database
{
    public class Construction : IDatabaseObject<Construction>, IEquatable<Construction>
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public int Id { get; set; }
        [NotNull]
        public ConstructionType Type { get; set; }
        [NotNull]
        public string TypeName { get; set; } = string.Empty;

        public string TypeDescription { get; set; } = string.Empty;

        [NotNull]
        public int IsVertical { get; set; }

        [NotNull, ForeignKey(typeof(DocumentSource))] // FK for the n:1 relationship with DocumentSource
        public int DocumentSourceId { get; set; }
        [NotNull]
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        [NotNull]
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        //------Not part of the Database-----//

        // n:1 relationship with DocumentSource
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public DocumentSource DocumentSource { get; set; } = new DocumentSource();

        // m:n relationship with Requirement
        [ManyToMany(typeof(ConstructionRequirement), CascadeOperations = CascadeOperation.CascadeRead)]
        public List<Requirement> Requirements { get; set; } = new List<Requirement>();

        [Ignore]
        public bool IsLayoutVertical // true = 1
        {
            get => IsVertical == 1;
            set => IsVertical = (value) ? 1 : 0;
        }
        [Ignore]
        public static Construction Empty => new Construction(); // Optional static default (for easy reference)

        [Ignore]
        public bool IsNewEmptyConstruction => this.Equals(Construction.Empty);

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
            copy.Type = this.Type;
            copy.TypeName = this.TypeName;
            copy.TypeDescription = this.TypeDescription;
            copy.IsVertical = this.IsVertical;
            copy.DocumentSourceId = this.DocumentSourceId;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            return copy;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }

        #region IEquatable<Construction> Implementation

        public bool Equals(Construction? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Type == other.Type && TypeName == other.TypeName && TypeDescription == other.TypeDescription && IsVertical == other.IsVertical && DocumentSourceId == other.DocumentSourceId;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Construction)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, (int)Type, TypeName, TypeDescription, IsVertical, DocumentSourceId);
        }

        #endregion
    }
}
