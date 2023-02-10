using BauphysikToolWPF.ComponentCalculations;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace BauphysikToolWPF.SQLiteRepo
{
    public class Project
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public int ProjectId { get; set; }

        public string? Name { get; set; }

        public string? UserName { get; set; }

        [NotNull]
        public int BuildingUsage { get; set; } // Boolean values are stored as integers 0 (false) and 1 (true)

        [NotNull]
        public int BuildingAge { get; set; } // Boolean values are stored as integers 0 (false) and 1 (true)

        //------Not part of the Database-----//

        // 1:n relationship with Element
        [OneToMany(CascadeOperations = CascadeOperation.All)] // ON DELETE CASCADE (When a Project is removed: Deletes all Elements linked to this 'Project' aswell)
        public List<Element> Elements { get; set; }

        // Encapsulate/Hide BuildingUsage and BuildingAge to convert to bool
        [Ignore]
        public bool IsResidentialUsage // = 1
        {
            get { return BuildingUsage == (int)CheckRequirements.BuildingUsage.Residential; }
            set
            {
                BuildingUsage = (value) ? 1 : 0;
            }
        }
        [Ignore]
        public bool IsNonResidentialUsage // = 0
        {
            get { return BuildingUsage == (int)CheckRequirements.BuildingUsage.NonResidential; }
            set
            {
                BuildingUsage = (value) ? 0 : 1;
            }
        }
        [Ignore]
        public bool IsNewConstruction // = 1
        {
            get { return BuildingAge == (int)CheckRequirements.BuildingAge.New; }
            set
            {
                BuildingAge = (value) ? 1 : 0;
            }
        }
        [Ignore]
        public bool IsExistingConstruction // = 0
        {
            get { return BuildingAge == (int)CheckRequirements.BuildingAge.Existing; }
            set
            {
                BuildingAge = (value) ? 0 : 1;
            }
        }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return Name + " (Id: " + ProjectId + ")";
        }
    }
}
