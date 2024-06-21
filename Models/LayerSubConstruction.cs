using BauphysikToolWPF.Models.Helper;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace BauphysikToolWPF.Models
{

    public enum SubConstructionDirection
    {
        Horizontal,
        Vertical
    }
    public class LayerSubConstruction : ISavefileElement<LayerSubConstruction>
    {
        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; }
        [NotNull]
        public double Width { get; set; } // cm
        [NotNull]
        public double Height { get; set; } // cm
        [NotNull]
        public double Spacing { get; set; } // cm von Achse zu Achse
        [NotNull]
        public SubConstructionDirection SubConstructionDirection { get; set; }

        [NotNull]
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        [NotNull]
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        [ForeignKey(typeof(Material))] // FK for the 1:1 relationship with Material
        public int MaterialId { get; set; }


        //------Not part of the Database-----//

        [Ignore]
        public int InternalId { get; set; }
        [Ignore]
        public bool IsValid => Width > 0 && Height > 0 && Spacing > 0;
        [Ignore]
        public double InnerSpacing => Spacing - Width;
        [Ignore]
        public bool IsSelected { get; set; } // For UI Purposes
        [Ignore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [Ignore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);

        // 1:1 relationship with Material
        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Material Material { get; set; } = new Material();


        //------Methods-----//

        public LayerSubConstruction Copy()
        {
            throw new NotImplementedException();
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }
    }
}
