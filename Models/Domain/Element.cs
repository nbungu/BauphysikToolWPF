using BauphysikToolWPF.Calculation;
using BauphysikToolWPF.Models.Database;
using BauphysikToolWPF.Repositories;
using BauphysikToolWPF.Services.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BauphysikToolWPF.Models.Domain.Helper;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;
using static BauphysikToolWPF.Models.Database.Helper.Enums;

namespace BauphysikToolWPF.Models.Domain
{
    public class Element : IDomainObject<Element>
    {
        #region Serialization Objects

        public string Name { get; set; } = string.Empty;

        private string _shortName = string.Empty;
        public string ShortName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_shortName))
                {
                    if (ConstructionTypeShortNameMapping.TryGetValue(Construction.ConstructionType, out string mappedShort))
                    {
                        return mappedShort + InternalId;
                    }
                    // Fallback to just InternalId if nothing else is available
                    return InternalId.ToString();
                }
                return _shortName;
            }
            set => _shortName = value;
        }
        public string ColorCode { get; set; } = "#00FFFFFF";
        public string Tag { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public OrientationType OrientationType { get; set; } = OrientationType.North;
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        public int ConstructionId { get; set; } = 0;
        public List<Layer> Layers { get; set; } = new List<Layer>();
        public ThermalValuesCalcConfig ThermalCalcConfig { get; set; } = new ThermalValuesCalcConfig();
        public EnvelopeCalculationConfig EnvelopeCalcConfig { get; set; } = new EnvelopeCalculationConfig();

        #endregion

        #region Non-serialized properties

        [JsonIgnore]
        public int InternalId { get; set; } = -1;

        [JsonIgnore]
        public Project? ParentProject { get; set; }

        [JsonIgnore]
        public string OrientationTypeName => OrientationTypeMapping[OrientationType];

        [JsonIgnore]
        public Construction Construction => DatabaseAccess.QueryConstructionById(ConstructionId, recursive: true);

        [JsonIgnore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);
        [JsonIgnore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);

        [JsonIgnore]
        public bool IsValid => Name != string.Empty && Layers.Count > 0;

        [JsonIgnore]
        public bool IsInhomogeneous => Layers.Any(l => l.SubConstruction != null);

        [JsonIgnore]
        public Color Color => ColorCode == "#00FFFFFF" ? Colors.Transparent : (Color)ColorConverter.ConvertFromString(ColorCode); // HEX 'ColorCode' Property to 'Color' Type

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
                    if (layer.SubConstruction != null)
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
                    if (layer.SubConstruction != null)
                    {
                        val += layer.ArealHeatCapacity;
                        val += layer.SubConstruction.ArealHeatCapacity;
                    }
                    else val += layer.ArealHeatCapacity;
                }
                return Math.Round(val, 2);
            }
        }

        #region Calculation Results

        [JsonIgnore]
        public double RGesValue => ThermalResults.RGes; // R_ges in m²K/W
        [JsonIgnore]
        public double RTotValue => ThermalResults.RTotal; // R_tot in m²K/W
        [JsonIgnore]
        public double QValue => ThermalResults.QValue; // q in W/m²
        [JsonIgnore]
        public double UValue => ThermalResults.UValue; // q in W/m²

        /// <summary>
        /// Recalculate Flag only gets set by LayerSetup Page: All Changes to the Layers and EnvVars,
        /// which would require a re-calculation, are made there.
        /// </summary>
        [JsonIgnore]
        public bool Recalculate { get; private set; } = true;

        // Use GlaserCalc as Collection for Results due to Polymorphism;
        // You can use GlaserCalc objects wherever ThermalValuesCalc and TemperatureCurveCalc objects are expected.
        private ThermalValuesCalc _thermalResults = new ThermalValuesCalc();
        [JsonIgnore]
        public ThermalValuesCalc ThermalResults
        {
            get
            {
                if (Recalculate)
                {
                    _thermalResults = new ThermalValuesCalc(this, this.ThermalCalcConfig);
                    Recalculate = false;
                }
                return _thermalResults;
            }
        }

        private CheckRequirements? _requirements;
        [JsonIgnore]
        public CheckRequirements Requirements
        {
            get
            {
                if (_requirements is null)
                {
                    _requirements = new CheckRequirements(this);
                }
                return _requirements;
            }
        }

        #endregion


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
            copy.ThermalCalcConfig = this.ThermalCalcConfig.Copy();
            copy.EnvelopeCalcConfig = this.EnvelopeCalcConfig.Copy();
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            // Deep copy of the Layers list
            this.Layers.ForEach(l => l.CopyToElement(copy));
            return copy;
        }

        public void CopyToProject(Project project)
        {
            var copy = Copy();
            copy.ParentProject = project; // Set the parent project reference
            project.AddElement(copy); // Add the copied element to the project);
        }

        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return Name + " - " + Construction.TypeName;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }

        public void RefreshResults()
        {
            Recalculate = true;
        }

        #endregion
    }
}
