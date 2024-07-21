using BauphysikToolWPF.Services;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BauphysikToolWPF.Models
{
    public enum BuildingUsageType
    {
        NonResidential, // 0, Nichtwohngebäude
        Residential     // 1, Wohngebäude
    }
    public enum BuildingAgeType
    {
        Existing,       // 0, Bestandsgebäude
        New             // 1, Neubau
    }
    public class Project
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public int Id { get; set; } = -1; // -1 means: Is not part of Database yet
        [NotNull]
        public string Name { get; set; } = string.Empty;
        [NotNull]
        public string UserName { get; set; } = string.Empty;

        [NotNull]
        public int BuildingUsage { get; set; }

        [NotNull]
        public int BuildingAge { get; set; }

        [NotNull]
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        [NotNull]
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();


        //------Not part of the Database-----//

        [Ignore, JsonIgnore]
        public int InternalId { get; set; }

        [Ignore, JsonIgnore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [Ignore, JsonIgnore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);

        // 1:n relationship with Element
        [Ignore, OneToMany(CascadeOperations = CascadeOperation.All)] // ON DELETE CASCADE (When a Project is removed: Deletes all Elements linked to this 'Project' aswell)
        public List<Element> Elements { get; set; } = new List<Element>();

        // Encapsulate/Hide BuildingUsage and BuildingAge to convert to bool
        [Ignore, JsonIgnore]
        public bool IsResidentialUsage // true = 1
        {
            get => BuildingUsage == (int)BuildingUsageType.Residential;
            set => BuildingUsage = (value) ? 1 : 0;
        }
        [Ignore, JsonIgnore]
        public bool IsNonResidentialUsage // = 0
        {
            get => BuildingUsage == (int)BuildingUsageType.NonResidential;
            set => BuildingUsage = (value) ? 0 : 1;
        }
        [Ignore, JsonIgnore]
        public bool IsNewConstruction // = 1
        {
            get => BuildingAge == (int)BuildingAgeType.New;
            set => BuildingAge = (value) ? 1 : 0;
        }
        [Ignore, JsonIgnore]
        public bool IsExistingConstruction // = 0
        {
            get => BuildingAge == (int)BuildingAgeType.Existing;
            set => BuildingAge = (value) ? 0 : 1;
        }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//

        public Project Copy()
        {
            var copy = new Project();
            copy.Id = -1;
            copy.InternalId = this.InternalId;
            copy.Name = this.Name;
            copy.UserName = this.UserName;
            copy.BuildingAge = this.BuildingAge;
            copy.BuildingUsage = this.BuildingUsage;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.Elements = this.Elements;

            return copy;
        }

        public IRepositoryEntity<Project> Convert()
        {
            throw new System.NotImplementedException();
        }

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return Name + " (Id: " + Id + ")";
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }
    }
}
