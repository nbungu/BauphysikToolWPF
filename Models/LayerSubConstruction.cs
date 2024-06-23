using BauphysikToolWPF.Models.Helper;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using BauphysikToolWPF.Services;

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

        /*public LayerGeometry ToGeometry()
        {
            var initWidth = Width; // cm
            var initHeight = Height; // cm
            var geometry = new LayerGeometry()
            {
                Rectangle = new Rectangle(new Point(0, 0), initWidth, initHeight),
                LayerThickness = initWidth,
                LayerPosition = LayerPosition.ToString(),
                BackgroundColor = new SolidColorBrush(Material.Color),
                DrawingBrush = DrawingBrush.GetHatchPattern(Material.Category, 0.5, 0, 0),
                StrokeColor = IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black,
                StrokeThickness = IsSelected ? 1 : 0.2,
                Opacity = IsEffective ? 1 : 0.2,
            };
            return geometry;
        }*/
    }
}
