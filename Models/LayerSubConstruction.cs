using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.UI.Helper;
using Geometry;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Windows.Media;

namespace BauphysikToolWPF.Models
{

    public enum SubConstructionDirection
    {
        Horizontal,
        Vertical
    }
    public class LayerSubConstruction : IDrawingGeometry //, ISavefileElement<LayerSubConstruction>
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

        public void UpdateGeometry()
        {
            Rectangle = new Rectangle(new Point(0, 0), this.Width, this.Height);
            BackgroundColor = new SolidColorBrush(this.Material.Color);
            DrawingBrush = HatchPattern.GetHatchPattern(this.Material.Category, 0.5, this.Width, this.Height);
            RectangleBorderColor = this.IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1473e6")) : Brushes.Black;
            RectangleBorderThickness = this.IsSelected ? 1 : 0.2;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }

        #region IDrawingGeometry

        public Rectangle Rectangle { get; set; } = Rectangle.Empty;
        public Brush RectangleBorderColor { get; set; } = Brushes.Black;
        public double RectangleBorderThickness { get; set; } = 0.2;
        public Brush BackgroundColor { get; set; } = Brushes.Transparent;
        public Brush DrawingBrush { get; set; } = new DrawingBrush();
        public double Opacity { get; set; } = 1;
        public int ZIndex { get; set; } = 1;
        public IDrawingGeometry Convert()
        {
            return new DrawingGeometry(this);
        }

        #endregion
    }
}
