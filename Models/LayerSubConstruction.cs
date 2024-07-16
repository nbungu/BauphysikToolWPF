﻿using System.Globalization;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.UI.Drawing;
using Geometry;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Windows.Media;
using System;

namespace BauphysikToolWPF.Models
{
    public enum SubConstructionDirection
    {
        Vertical,
        Horizontal
    }

    /// <summary>
    /// Business logic of a LayerSubConstruction
    /// </summary>
    public partial class LayerSubConstruction : IDrawingGeometry
    {
        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; }

        [NotNull]
        public double Width { get; set; } // cm
        [NotNull]
        public double Thickness { get; set; } // cm
        [NotNull]
        public double Spacing { get; set; } // cm (innerer Abstand)
        [NotNull]
        public SubConstructionDirection SubConstructionDirection { get; set; }

        [NotNull]
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        [NotNull]
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        [NotNull, ForeignKey(typeof(Material))] // FK for the 1:1 relationship with Material
        public int MaterialId { get; set; }


        //------Not part of the Database-----//

        [Ignore]
        public int InternalId { get; set; }
        [Ignore]
        public bool IsValid => Width > 0 && Thickness > 0 && Spacing > 0 && Material != null;


        [Ignore]
        public double AxisSpacing
        {
            get => Spacing + Width;
            set => Spacing = value - Width;
        }

        [Ignore]
        public bool IsSelected { get; set; } // For UI Purposes
        [Ignore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [Ignore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);

        // 1:1 relationship with Material
        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Material Material { get; set; } = new Material();

        [Ignore]
        public bool IsEffective { get; set; } = true;

        [Ignore]
        public double R_Value
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                return Math.Round((this.Thickness / 100) / Material.ThermalConductivity, 3);
            }
        }

        //------Methods-----//

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }

        public override string ToString() // Überlagert vererbte standard ToString() Methode 
        {
            return Width.ToString(CultureInfo.CurrentCulture) + " x " + Thickness.ToString(CultureInfo.CurrentCulture) + " cm " + Material.Name + ", Abstand: " + Spacing.ToString(CultureInfo.CurrentCulture) + " cm";
        }
    }

    /// <summary>
    /// Presentation logic of a LayerSubConstruction which can be drawn on a XAML Canvas
    /// </summary>
    public partial class LayerSubConstruction : IDrawingGeometry
    {
        #region IDrawingGeometry
        [Ignore]
        public Rectangle Rectangle { get; set; } = Rectangle.Empty;
        [Ignore]
        public Brush RectangleBorderColor { get; set; } = Brushes.Black;
        [Ignore]
        public double RectangleBorderThickness { get; set; } = 0.2;
        [Ignore]
        public DoubleCollection RectangleStrokeDashArray { get; set; } = new DoubleCollection();
        [Ignore]
        public Brush BackgroundColor { get; set; } = Brushes.Transparent;
        [Ignore]
        public Brush DrawingBrush { get; set; } = new DrawingBrush();
        [Ignore]
        public double Opacity { get; set; } = 1;
        [Ignore]
        public int ZIndex { get; set; } = 1;
        [Ignore]
        public object Tag { get; set; }

        public IDrawingGeometry Convert()
        {
            return new DrawingGeometry(this);
        }

        #endregion
    }
}

