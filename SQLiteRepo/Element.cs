using BauphysikToolWPF.ComponentCalculations;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using static BauphysikToolWPF.ComponentCalculations.CheckRequirements;

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

        // m:n relationship with EnvVars
        [ManyToMany(typeof(ElementEnvVars), CascadeOperations = CascadeOperation.All)] // ON DELETE CASCADE (When parent Element is removed: Deletes all EnvVars linked to this 'Element')
        public List<EnvVars> EnvVars { get; set; }

        [Ignore]
        public bool IsLocked { get; set; } // For UI Purposes

        // Encapsulate 'Image' variable for use in frontend
        [Ignore]
        public BitmapImage ElementImage
        {
            get
            {
                if (Image == null || Image.Length == 0)
                    return null;

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
        public double ElementThickness_cm // d in cm
        {
            get
            {
                double thickness = 0;
                foreach (Layer layer in Layers)
                {
                    thickness += layer.LayerThickness;
                }
                return Math.Round(thickness, 2);
            }
        }
        public double ElementThickness_m // d in m
        {
            get
            {
                double thickness = 0;
                foreach (Layer layer in Layers)
                {
                    thickness += layer.LayerThickness;
                }
                return Math.Round(thickness/100, 4);
            }
        }

        [Ignore]
        public double ElementSdThickness // sd in m
        {
            get
            {
                double thickness = 0;
                foreach (Layer layer in Layers)
                {
                    thickness += layer.Sd_Thickness;
                }
                return Math.Round(thickness, 2);
            }
        }

        [Ignore]
        public double ElementAreaMassDens // m' in kg/m²
        {
            get
            {
                double areaMassDens = 0;
                foreach (Layer layer in Layers)
                {
                    areaMassDens += layer.AreaMassDensity;
                }
                return Math.Round(areaMassDens, 2);
            }
        }

        [Ignore]
        public double ElementRValue // R_ges in m²K/W
        {
            get
            {
                double r_ges = 0;
                foreach (Layer layer in Layers)
                {
                    r_ges += layer.R_Value;
                }
                return Math.Round(r_ges, 2);
            }
        }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return Name + "_" + Construction.Type + " (Id: " + ElementId + ")";
        }
    }
}
