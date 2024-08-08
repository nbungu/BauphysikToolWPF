using BauphysikToolWPF.Services;
using BauphysikToolWPF.SessionData;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BauphysikToolWPF.Models.Helper;

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
        public int Id { get; set; } = -1; // -1 means: Is not part of Database yet

        [NotNull, ForeignKey(typeof(Construction))] // FK for the 1:1 relationship with Construction
        public int ConstructionId { get; set; }

        [NotNull, ForeignKey(typeof(Project))] // FK for the n:1 relationship with Project
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

        [Ignore, JsonIgnore]
        public int InternalId { get; set; }
        
        // n:1 relationship with Project
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead), JsonIgnore]
        public Project Project { get; set; } = new Project();

        // 1:n relationship with Layer
        [OneToMany(CascadeOperations = CascadeOperation.All)] // ON DELETE CASCADE (When parent Element is removed: Deletes all Layers linked to this 'Element')
        public List<Layer> Layers { get; set; } = new List<Layer>(0);

        // 1:1 relationship with Construction
        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Construction Construction { get; set; } = new Construction();

        [Ignore, JsonIgnore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [Ignore, JsonIgnore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);

        [Ignore, JsonIgnore]
        public bool IsValid => Name != string.Empty && Layers.Count > 0;

        [Ignore, JsonIgnore]
        public bool IsInhomogeneous => Layers.Any(l => l.HasSubConstructions);

        [Ignore, JsonIgnore]
        public Color Color // HEX 'ColorCode' Property to 'Color' Type
        {
            get
            {
                if (ColorCode == "#00FFFFFF") return Colors.Transparent;
                return (Color)ColorConverter.ConvertFromString(ColorCode);
            }
        }

        [Ignore, JsonIgnore]
        public byte[] FullImage { get; set; } = Array.Empty<byte>();

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
        [Ignore, JsonIgnore]
        public BitmapImage ElementImage
        {
            get
            {
                if (Image == Array.Empty<byte>() || Image.Length == 0) return new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/placeholder_256px_light.png"));

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

        [Ignore, JsonIgnore]
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

        [Ignore, JsonIgnore]
        public double SdThickness // sd in m
        {
            get
            {
                double fullWidth = 0;
                if (Layers.Count == 0) return fullWidth;
                foreach (Layer layer in Layers)
                {
                    if (!layer.IsEffective) continue;
                    fullWidth += layer.Sd_Thickness;
                }
                return Math.Round(fullWidth, 2);
            }
        }

        [Ignore, JsonIgnore]
        public double AreaMassDens // m' in kg/m²
        {
            get
            {
                if (Layers.Count == 0) return 0;
                double val = 0;
                foreach (Layer layer in Layers)
                {
                    if (layer.HasSubConstructions)
                    {
                        val += layer.AreaMassDensity;
                        val += layer.SubConstruction.AreaMassDensity;
                    }
                    else val += layer.AreaMassDensity;
                }
                return Math.Round(val, 2);
            }
        }

        [Ignore, JsonIgnore]
        public double ArealHeatCapacity // C in kJ/m²K
        {
            get
            {
                if (Layers.Count == 0) return 0;
                double val = 0;
                foreach (Layer layer in Layers)
                {
                    if (layer.HasSubConstructions)
                    {
                        val += layer.ArealHeatCapacity;
                        val += layer.SubConstruction.ArealHeatCapacity;
                    }
                    else val += layer.ArealHeatCapacity;
                }
                return Math.Round(val, 2);

            }
        }

        #region Thermal Calculations

        [Ignore, JsonIgnore]
        public double RGesValue => UserSaved.CalcResults.RGes; // R_ges in m²K/W
        [Ignore, JsonIgnore]
        public double RTotValue => UserSaved.CalcResults.RTotal; // R_tot in m²K/W
        [Ignore, JsonIgnore]
        public double QValue => UserSaved.CalcResults.QValue; // q in W/m²
        [Ignore, JsonIgnore]
        public double UValue => UserSaved.CalcResults.UValue; // q in W/m²

        [Ignore, JsonIgnore]
        public List<EnvVars> UsedEnvVars => new List<EnvVars>()
        {
            new EnvVars(UserSaved.CalcResults.Rsi, Symbol.TransferResistanceSurfaceInterior, Unit.SquareMeterKelvinPerWatt),
            new EnvVars(UserSaved.CalcResults.Rse, Symbol.TransferResistanceSurfaceExterior, Unit.SquareMeterKelvinPerWatt),
            new EnvVars(UserSaved.CalcResults.Ti, Symbol.TemperatureInterior, Unit.Celsius),
            new EnvVars(UserSaved.CalcResults.Te, Symbol.TemperatureExterior, Unit.Celsius)
        };

        #endregion

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public Element Copy()
        {
            var copy = new Element();
            copy.Id = -1;
            copy.ConstructionId = this.ConstructionId;
            copy.Construction = this.Construction;
            copy.OrientationType = this.OrientationType;
            copy.ProjectId = this.ProjectId;
            copy.Project = this.Project;
            copy.Name = this.Name + "-Copy";
            copy.Image = this.Image;
            copy.ColorCode = this.ColorCode;
            copy.Tag = this.Tag;
            copy.Comment = this.Comment;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
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
