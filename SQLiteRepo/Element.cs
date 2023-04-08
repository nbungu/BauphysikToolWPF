using BauphysikToolWPF.SessionData;
using BauphysikToolWPF.UI;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

/* 
 * https://bitbucket.org/twincoders/sqlite-net-extensions/src/master/
 * https://social.msdn.microsoft.com/Forums/en-US/85b1141b-2144-40c2-b9b3-e1e6cdb0ea02/announcement-cascade-operations-in-sqlitenet-extensions?forum=xamarinlibraries
 */

namespace BauphysikToolWPF.SQLiteRepo
{
    public class Element
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique] // SQL Attributes
        public int ElementId { get; set; }

        [NotNull]
        public string Name { get; set; }

        [ForeignKey(typeof(Construction))] // FK for the 1:1 relationship with Construction
        public int ConstructionId { get; set; }

        [ForeignKey(typeof(Orientation))] // FK for the 1:1 relationship with Orientation
        public int OrientationId { get; set; }

        [ForeignKey(typeof(Project))] // FK for the n:1 relationship with Project
        public int ProjectId { get; set; }

        public byte[]? Image { get; set; }

        //------Not part of the Database-----//

        // n:1 relationship with Project
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Project Project { get; set; }

        // 1:n relationship with Layer
        [OneToMany(CascadeOperations = CascadeOperation.All)] // ON DELETE CASCADE (When parent Element is removed: Deletes all Layers linked to this 'Element')
        public List<Layer> Layers { get; set; }

        // 1:1 relationship with Construction
        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Construction Construction { get; set; }

        // 1:1 relationship with Orientation
        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Orientation Orientation { get; set; }

        // m:n relationship with EnvVars
        [ManyToMany(typeof(ElementEnvVars), CascadeOperations = CascadeOperation.All)] // ON DELETE CASCADE (When parent Element is removed: Deletes all EnvVars linked to this 'Element')
        public List<EnvVars> EnvVars { get; set; }

        [Ignore]
        public bool IsSelectedElement // For UI Purposes
        {
            get { return ElementId == FO0_LandingPage.SelectedElementId; }
        }

        // Encapsulate 'Image' variable for use in frontend
        [Ignore]
        public BitmapImage ElementImage
        {
            get
            {
                if (Image is null)
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/placeholder_256px_light.png"));

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
        public double Thickness_cm // d in cm
        {
            get
            {
                if (Layers == null )
                    return 0;

                double val = 0;
                foreach (Layer layer in Layers)
                {
                    val += layer.LayerThickness;
                }
                return Math.Round(val, 2);
            }
        }
        public double Thickness_m // d in m
        {
            get { return Math.Round(Thickness_cm/100, 4); }
        }

        [Ignore]
        public double SdThickness // sd in m
        {
            get
            {
                if (Layers == null)
                    return 0;

                double val = 0;
                foreach (Layer layer in Layers)
                {
                    if (!layer.IsEffective)
                        continue;
                    val += layer.Sd_Thickness;
                }
                return Math.Round(val, 2);
            }
        }

        [Ignore]
        public double AreaMassDens // m' in kg/m²
        {
            get
            {
                if (Layers == null)
                    return 0;

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
                if (Layers == null)
                    return 0;

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
        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return Name + " - " + Construction.TypeName + " (Id: " + ElementId + ")";
        }
    }
}
