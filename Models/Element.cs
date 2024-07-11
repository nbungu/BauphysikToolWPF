using BauphysikToolWPF.Services;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

/* 
 * https://bitbucket.org/twincoders/sqlite-net-extensions/src/master/
 * https://social.msdn.microsoft.com/Forums/en-US/85b1141b-2144-40c2-b9b3-e1e6cdb0ea02/announcement-cascade-operations-in-sqlitenet-extensions?forum=xamarinlibraries
 */

namespace BauphysikToolWPF.Models
{
    public enum OrientationType
    {
        Norden,
        NordOsten,
        Osten,
        SuedOsten,
        Sueden,
        SuedWesten,
        Westen,
        NordWesten
    }

    public class Element
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique] // SQL Attributes
        public int Id { get; set; }

        [ForeignKey(typeof(Construction))] // FK for the 1:1 relationship with Construction
        public int ConstructionId { get; set; }

        [ForeignKey(typeof(Project))] // FK for the n:1 relationship with Project
        public int ProjectId { get; set; }
        [NotNull]
        public string Name { get; set; } = string.Empty;
        [NotNull]
        public byte[] Image { get; set; } = Array.Empty<byte>();
        [NotNull]
        public string ColorCode { get; set; } = "#00FFFFFF";
        [NotNull]
        public string Tag { get; set; } = string.Empty;
        [NotNull]
        public string Comment { get; set; } = string.Empty;
        [NotNull]
        public OrientationType OrientationType { get; set; } = OrientationType.Norden;
        [NotNull]
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        [NotNull]
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        //------Not part of the Database-----//

        [Ignore]
        public int InternalId { get; set; }
        
        // n:1 relationship with Project
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Project Project { get; set; } = new Project();

        // 1:n relationship with Layer
        [OneToMany(CascadeOperations = CascadeOperation.All)] // ON DELETE CASCADE (When parent Element is removed: Deletes all Layers linked to this 'Element')
        public List<Layer> Layers { get; set; } = new List<Layer>();

        // 1:1 relationship with Construction
        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Construction Construction { get; set; } = new Construction();

        // m:n relationship with EnvVars
        [ManyToMany(typeof(ElementEnvVars), CascadeOperations = CascadeOperation.All)] // ON DELETE CASCADE (When parent Element is removed: Deletes all EnvVars linked to this 'Element')
        public List<EnvVars> EnvVars { get; set; } = new List<EnvVars>();

        // Properties

        [Ignore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [Ignore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);

        [Ignore]
        public bool IsValid => Name != string.Empty && Layers.Count > 0;

        [Ignore]
        public Color Color // HEX 'ColorCode' Property to 'Color' Type
        {
            get
            {
                if (ColorCode == "#00FFFFFF") return Colors.Transparent;
                return (Color)ColorConverter.ConvertFromString(ColorCode);
            }
        }

        [Ignore]
        public List<string> TagList // Converts string of Tags, separated by Comma, to a List of Tags
        {
            get
            {
                if (Tag == string.Empty) return new List<string>();
                return Tag.Split(',').ToList(); // Splits elements of a string into a List
            }
            set => Tag = (value.Count == 0) ? "" : string.Join(",", value); // Joins elements of a list into a single string with the words separated by commas   
        }

        // Encapsulate 'Image' variable for use in frontend
        [Ignore]
        public BitmapImage ElementImage
        {
            get
            {
                if (Image == Array.Empty<byte>()) return new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/placeholder_256px_light.png"));

                BitmapImage image = new BitmapImage();
                // use using to call Dispose() after use of unmanaged resources. GC cannot manage this
                using (MemoryStream stream = new MemoryStream(Image))
                {
                    stream.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = stream;
                    image.EndInit();
                }
                image.Freeze();
                return image;
            }
        }

        [Ignore]
        public double Thickness // d in cm
        {
            get
            {
                double fullWidth = 0;
                if (Layers.Count == 0) return fullWidth;
                Layers.ForEach(l => fullWidth += l.Thickness);
                return Math.Round(fullWidth, 4);
            }
        }

        [Ignore]
        public double SdThickness // sd in m
        {
            get
            {
                double fullWidth = 0;
                if (Layers.Count == 0) return fullWidth;
                foreach (Layer layer in Layers)
                {
                    if (!layer.IsEffective)
                        continue;
                    fullWidth += layer.Sd_Thickness;
                }
                return Math.Round(fullWidth, 2);
            }
        }

        [Ignore]
        public double AreaMassDens // m' in kg/m²
        {
            get
            {
                if (Layers.Count == 0) return 0;
                double val = 0;
                foreach (Layer layer in Layers)
                {
                    val += layer.AreaMassDensity;
                }
                return Math.Round(val, 2);
            }
        }

        [Ignore]
        public double RValue // R_ges in m²K/W
        {
            get
            {
                if (Layers.Count == 0) return 0;
                double val = 0;
                foreach (Layer layer in Layers)
                {
                    if (!layer.IsEffective) // cut after Air Layer -> Remaining Layer don't add to RValue
                        continue;
                    val += layer.R_Value;
                }
                return Math.Round(val, 2);
            }
        }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public Element Copy()
        {
            var copy = new Element();
            copy.Id = this.Id;
            copy.ConstructionId = this.ConstructionId;
            copy.Construction = this.Construction;
            copy.OrientationType = this.OrientationType;
            copy.ProjectId = this.ProjectId;
            copy.Project = this.Project;
            copy.Name = this.Name + "-Copy";
            copy.Image = this.Image;
            copy.ColorCode = this.ColorCode;
            copy.Project = this.Project;
            copy.Tag = this.Tag;
            copy.Comment = this.Comment;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.EnvVars = this.EnvVars;
            copy.Layers = this.Layers;
            copy.InternalId = this.InternalId;
            return copy;
        }

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return Name + " - " + Construction.TypeName + " (Id: " + Id + ")";
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }
    }
}
