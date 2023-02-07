﻿using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace BauphysikToolWPF.SQLiteRepo
{
    //https://bitbucket.org/twincoders/sqlite-net-extensions/src/master/
    public class Element
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public int ElementId { get; set; }

        [NotNull]
        public string Name { get; set; }

        [ForeignKey(typeof(Construction))] // FK for the 1:1 relation
        public int ConstructionId { get; set; }

        [ForeignKey(typeof(Project))] // FK for the n:1 relation
        public int ProjectId { get; set; }

        //------Not part of the Database-----//

        [OneToMany] // 1:n relationship with Layer, ON DELETE CASCADE
        public List<Layer> Layers { get; set; } // the corresp. object/Type for the foreign-key. The 'List<Layer>' object itself is not stored in DB!

        [OneToOne] // 1:1 relationship with Construction
        public Construction Construction { get; set; } // Gets the corresp. object linked by the foreign-key. The 'Material' object itself is not stored in DB!

        [ManyToMany(typeof(ElementEnvVars))] // m:n relationship with EnvVars (ElementEnvVars is intermediate entity)
        public List<EnvVars> EnvVars { get; set; }

        [ManyToOne] // n:1 relationship with Project (the parent table)
        public Project Project { get; set; } // Gets the corresp. object linked by the foreign-key. The 'Project' object itself is not stored in DB!


        [Ignore] // TODO add as BLOB!! Or save as static Bitmap
        public string ElementImage
        {
            //Image has to be "Resource" as build action
            get
            {
                string imgName = "Element_" + ElementId.ToString()+".png";
                return "/Resources/ElementImages/"+imgName;
            }
        }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return ElementId + "_" + Name + " (" + this.Construction.Type + ")";
        }
    }
}
