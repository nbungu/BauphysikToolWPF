using BauphysikToolWPF.Services.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;

namespace BauphysikToolWPF.Models.Domain
{
    public class Project : IDomainObject<Project>, IEquatable<Project>
    {
        #region Serialization Objects

        public Guid Guid { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public BuildingUsageType BuildingUsage { get; set; }
        public BuildingAgeType BuildingAge { get; set; }
        public BuildingTypeResidatial BuildingTypeResidatial { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string LinkedFilePaths { get; set; } = string.Empty;
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        public List<Element> Elements { get; set; } = new List<Element>(0);
        public List<EnvelopeItem> EnvelopeItems { get; set; } = new List<EnvelopeItem>(0);

        #endregion

        #region Non-serialized Properties

        [JsonIgnore]
        public int InternalId { get; set; } = -1;

        [JsonIgnore]
        public static Project Empty => new Project(); // Optional static default (for easy reference)

        [JsonIgnore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        
        [JsonIgnore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);
        
        [JsonIgnore]
        public bool IsValid => true; // TODO: Implement validation logic

        [JsonIgnore]
        public bool IsNewEmptyProject => this.Equals(Project.Empty);

        [JsonIgnore]
        public bool IsModified { get; set; } = false;

        [JsonIgnore]
        public bool CreatedByUser { get; set; } = false;

        [JsonIgnore]
        public List<string> LinkedFilesList // Converts string of LinkedFiles, separated by Comma, to a List of LinkedFiles
        {
            get
            {
                if (LinkedFilePaths == string.Empty) return new List<string>();
                return LinkedFilePaths.Split(',').ToList(); // Splits elements of a string into a List
            }
            set
            {
                if (value != null) LinkedFilePaths = (value.Count == 0) ? "" : string.Join(",", value); // Joins elements of a list into a single string with the words separated by commas   
                else LinkedFilePaths = "";
            }
        }

        /// <summary>
        /// Recalculate Flag only gets set by LayerSetup Page: All Changes to the Layers and EnvVars,
        /// which would require a re-calculation, are made there.
        /// </summary>
        //[JsonIgnore]
        //public bool RecalculateEnvelope { get; set; } = true;

        //private EnvelopeCalculation _envelopeResults = new EnvelopeCalculation();
        
        //[JsonIgnore]
        //public EnvelopeCalculation EnvelopeResults
        //{
        //    get
        //    {
        //        if (RecalculateEnvelope)
        //        {
        //            var newResults = new EnvelopeCalculation(EnvelopeItems, Session.EnvelopeCalcConfig);
        //            _envelopeResults = newResults;
        //            RecalculateEnvelope = false;
        //        }
        //        return _envelopeResults;
        //    }
        //}

        //[JsonIgnore]
        //public List<DocumentSourceType> ProjectRelatedDocumentSources => this.GetProjectRelatedDocumentSources();

        #endregion

        #region ctors

        #endregion

        #region Public Methods

        public Project Copy()
        {
            var copy = new Project();
            copy.Name = this.Name;
            copy.UserName = this.UserName;
            copy.BuildingAge = this.BuildingAge;
            copy.BuildingUsage = this.BuildingUsage;
            copy.LinkedFilePaths = this.LinkedFilePaths;
            copy.Comment = this.Comment;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.IsModified = this.IsModified;
            copy.CreatedByUser = this.CreatedByUser;
            // Deep copies of the Element and EnvelopItem list
            this.Elements.ForEach(l => l.CopyToProject(copy));
            this.EnvelopeItems.ForEach(l => l.CopyToProject(copy));
            return copy;
        }

        public override string ToString() => $"{Name} (Guid: {Guid})";

        public void UpdateTimestamp() => UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();

        #endregion

        #region IEquatable<Project> Implementation

        public bool Equals(Project? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && UserName == other.UserName && BuildingUsage == other.BuildingUsage && BuildingAge == other.BuildingAge && Comment == other.Comment && LinkedFilePaths == other.LinkedFilePaths && IsModified == other.IsModified && CreatedByUser == other.CreatedByUser;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Project)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, UserName, (int)BuildingUsage, (int)BuildingAge, Comment, LinkedFilePaths, IsModified, CreatedByUser);
        }

        // NOTE:
        // We do NOT use Guid-only comparison for equality because:
        // - Elements are compared by their property values for calculations and serialization.
        // - Default instances created with the parameterless constructor will each have a unique Guid,
        //   making Guid-only Equals return false even if the objects are functionally identical.
        // - Value-based equality ensures proper behavior when checking against default instances
        //   and comparing serialized/deserialized objects.
        // - Same goes for the Timestamp properties.

        #endregion
    }
}
