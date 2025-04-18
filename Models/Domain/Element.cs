using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Models.UI;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;


/* 
 * https://bitbucket.org/twincoders/sqlite-net-extensions/src/master/
 * https://social.msdn.microsoft.com/Forums/en-US/85b1141b-2144-40c2-b9b3-e1e6cdb0ea02/announcement-cascade-operations-in-sqlitenet-extensions?forum=xamarinlibraries
 */

namespace BauphysikToolWPF.Models.Domain // or core?
{
    public class Element : IEquatable<Element>
    {
        #region Serialization Objects

        public string Name { get; set; } = string.Empty;
        public string ColorCode { get; set; } = "#00FFFFFF";
        public string Tag { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public OrientationType OrientationType { get; set; } = OrientationType.North;
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        public int ConstructionId { get; set; } = 0;
        public List<Layer> Layers { get; set; } = new List<Layer>(0);
        
        #endregion

        #region Non-serialized properties

        [JsonIgnore]
        public int InternalId { get; set; } = -1;

        [JsonIgnore]
        public string OrientationTypeName => OrientationTypeMapping[OrientationType];

        [JsonIgnore]
        public Construction Construction => DatabaseAccess.GetConstructionsQuery().FirstOrDefault(e => e.Id == ConstructionId, Construction.Empty);

        [JsonIgnore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [JsonIgnore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);

        [JsonIgnore]
        public bool IsValid => Name != string.Empty && Layers.Count > 0;

        [JsonIgnore]
        public bool IsInhomogeneous => Layers.Any(l => l.HasSubConstructions);

        [JsonIgnore]
        public Color Color // HEX 'ColorCode' Property to 'Color' Type
        {
            get
            {
                if (ColorCode == "#00FFFFFF") return Colors.Transparent;
                return (Color)ColorConverter.ConvertFromString(ColorCode);
            }
        }

        [JsonIgnore]
        public byte[] DocumentImage { get; set; } = Array.Empty<byte>();

        [JsonIgnore]
        public byte[] Image { get; set; } = Array.Empty<byte>();

        [JsonIgnore]
        public List<string> TagList // Converts string of Tags, separated by Comma, to a List of Tags
        {
            get
            {
                if (Tag == string.Empty) return new List<string>();
                return Tag.Split(',').ToList(); // Splits elements of a string into a List
            }
            set => Tag = (value.Count == 0) ? "" : string.Join(",", value); // Joins elements of a list into a single string with the words separated by commas   
        }
        
        [JsonIgnore]
        public BitmapImage PreviewImage => ImageCreator.ByteArrayToBitmap(Image);

        [JsonIgnore]
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

        [JsonIgnore]
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

        [JsonIgnore]
        public double AreaMassDens // m' in kg/m²
        {
            get
            {
                if (Layers.Count == 0) return 0;
                double val = 0;
                foreach (Layer layer in Layers)
                {
                    if (layer.HasSubConstructions && layer.SubConstruction != null)
                    {
                        val += layer.AreaMassDensity;
                        val += layer.SubConstruction.AreaMassDensity;
                    }
                    else val += layer.AreaMassDensity;
                }
                return Math.Round(val, 2);
            }
        }

        [JsonIgnore]
        public double ArealHeatCapacity // C in kJ/m²K
        {
            get
            {
                if (Layers.Count == 0) return 0;
                double val = 0;
                foreach (Layer layer in Layers)
                {
                    if (layer.HasSubConstructions && layer.SubConstruction != null)
                    {
                        val += layer.ArealHeatCapacity;
                        val += layer.SubConstruction.ArealHeatCapacity;
                    }
                    else val += layer.ArealHeatCapacity;
                }
                return Math.Round(val, 2);

            }
        }

        [JsonIgnore]
        public double RGesValue => Session.CalcResults.RGes; // R_ges in m²K/W
        [JsonIgnore]
        public double RTotValue => Session.CalcResults.RTotal; // R_tot in m²K/W
        [JsonIgnore]
        public double QValue => Session.CalcResults.QValue; // q in W/m²
        [JsonIgnore]
        public double UValue => Session.CalcResults.UValue; // q in W/m²

        [JsonIgnore]
        public List<EnvVars> UsedEnvVars => new List<EnvVars>()
        {
            new EnvVars(Session.CalcResults.Rsi, Symbol.TransferResistanceSurfaceInterior, Unit.SquareMeterKelvinPerWatt),
            new EnvVars(Session.CalcResults.Rse, Symbol.TransferResistanceSurfaceExterior, Unit.SquareMeterKelvinPerWatt),
            new EnvVars(Session.CalcResults.Ti, Symbol.TemperatureInterior, Unit.Celsius),
            new EnvVars(Session.CalcResults.Te, Symbol.TemperatureExterior, Unit.Celsius)
        };

        #endregion

        #region ctors

        #endregion

        #region Public Methods

        public Element Copy()
        {
            var copy = new Element();
            copy.ConstructionId = this.ConstructionId;
            copy.OrientationType = this.OrientationType;
            copy.Name = this.Name + "-Copy";
            copy.Image = this.Image;
            copy.DocumentImage = this.DocumentImage;
            copy.ColorCode = this.ColorCode;
            copy.Tag = this.Tag;
            copy.Comment = this.Comment;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.InternalId = this.InternalId;
            // Deep copy of the Layers list
            copy.Layers = this.Layers.Select(layer => layer.CopyToNewElement(copy)).ToList();
            return copy;
        }

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return Name + " - " + Construction.TypeName + " (InternalId: " + InternalId + ")";
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }
        
        #endregion

        #region IEquatable<Element> Implementation

        public bool Equals(Element? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && ColorCode == other.ColorCode && Tag == other.Tag && Comment == other.Comment && OrientationType == other.OrientationType && ConstructionId == other.ConstructionId && Layers.Equals(other.Layers) && InternalId == other.InternalId && DocumentImage.Equals(other.DocumentImage) && Image.Equals(other.Image);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Element)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Name);
            hashCode.Add(ColorCode);
            hashCode.Add(Tag);
            hashCode.Add(Comment);
            hashCode.Add((int)OrientationType);
            hashCode.Add(ConstructionId);
            hashCode.Add(Layers);
            hashCode.Add(InternalId);
            hashCode.Add(DocumentImage);
            hashCode.Add(Image);
            return hashCode.ToHashCode();
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
