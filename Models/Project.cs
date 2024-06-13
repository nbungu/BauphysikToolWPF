﻿using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BauphysikToolWPF.Models
{
    public enum BuildingUsageType
    {
        NonResidential, // 0, Nichtwohngebäude
        Residential     // 1, Wohngebäude
    }
    public enum BuildingAgeType
    {
        Existing,       // 0, Bestandsgebäude
        New             // 1, Neubau
    }
    public class Project
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public int ProjectId { get; set; }
        [NotNull]
        public string Name { get; set; } = string.Empty;
        [NotNull]
        public string UserName { get; set; } = string.Empty;

        [NotNull]
        public int BuildingUsage { get; set; } // Boolean values are stored as integers 0 (false) and 1 (true)

        [NotNull]
        public int BuildingAge { get; set; } // Boolean values are stored as integers 0 (false) and 1 (true)

        //------Not part of the Database-----//

        // 1:n relationship with Element
        [OneToMany(CascadeOperations = CascadeOperation.All)] // ON DELETE CASCADE (When a Project is removed: Deletes all Elements linked to this 'Project' aswell)
        public List<Element> Elements { get; set; } = new List<Element>();

        // Encapsulate/Hide BuildingUsage and BuildingAge to convert to bool
        [Ignore]
        public bool IsResidentialUsage // true = 1
        {
            get => BuildingUsage == (int)BuildingUsageType.Residential;
            set => BuildingUsage = (value) ? 1 : 0;
        }
        [Ignore]
        public bool IsNonResidentialUsage // = 0
        {
            get => BuildingUsage == (int)BuildingUsageType.NonResidential;
            set => BuildingUsage = (value) ? 0 : 1;
        }
        [Ignore]
        public bool IsNewConstruction // = 1
        {
            get => BuildingAge == (int)BuildingAgeType.New;
            set => BuildingAge = (value) ? 1 : 0;
        }
        [Ignore]
        public bool IsExistingConstruction // = 0
        {
            get => BuildingAge == (int)BuildingAgeType.Existing;
            set => BuildingAge = (value) ? 0 : 1;
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