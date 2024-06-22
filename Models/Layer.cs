﻿using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.UI.Helper;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace BauphysikToolWPF.Models
{
    public class Layer : LayerGeometry, ISavefileElement<Layer>
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [NotNull, PrimaryKey, AutoIncrement, Unique]
        public int Id { get; set; }

        [NotNull]
        public int LayerPosition { get; set; } // Inside = 1

        [ForeignKey(typeof(Element))] // FK for the n:1 relationship with Element
        public int ElementId { get; set; }

        [ForeignKey(typeof(Material))] // FK for the 1:1 relationship with Material
        public int MaterialId { get; set; }

        [ForeignKey(typeof(LayerSubConstruction))] // FK for the 1:1 relationship with LayerSubConstruction
        public int SubConstructionId { get; set; }

        [NotNull]
        public double LayerThickness { get; set; } // Layer thickness in cm

        [NotNull]
        public int Effective { get; set; } // For Calculation Purposes - Whether a Layer is considered in the Calculations or not

        [NotNull]
        public long CreatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        [NotNull]
        public long UpdatedAt { get; set; } = TimeStamp.GetCurrentUnixTimestamp();


        //------Not part of the Database-----//

        [Ignore]
        public int InternalId { get; set; }

        [Ignore]
        public string CreatedAtString => TimeStamp.ConvertToNormalTime(CreatedAt);

        [Ignore]
        public string UpdatedAtString => TimeStamp.ConvertToNormalTime(UpdatedAt);

        // n:1 relationship with Element
        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Element Element { get; set; } = new Element();

        // 1:1 relationship with Material
        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public Material Material { get; set; } = new Material();

        // 1:1 relationship with LayerSubConstruction
        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public LayerSubConstruction SubConstruction { get; set; } = new LayerSubConstruction();

        [Ignore]
        public bool HasSubConstruction => SubConstruction.IsValid;

        [Ignore]
        public bool IsSelected { get; set; } // For UI Purposes 

        // Encapsulate/Hide 'Effective' to convert to bool
        [Ignore]
        public bool IsEffective // true = 1
        {
            get => Effective != 0;
            set => Effective = (value) ? 1 : 0;
        }

        [Ignore]
        public double R_Value
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                return Math.Round((this.LayerThickness / 100) / Material.ThermalConductivity, 3);
            }
        }

        [Ignore]
        public double Sd_Thickness // sd thickness in m
        {
            get
            {
                if (!Material.IsValid) return 0;
                return Math.Round((this.LayerThickness / 100) * Material.DiffusionResistance, 3);
            }
        }

        [Ignore]
        public double AreaMassDensity // m' in kg/m²
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                return Math.Round(this.LayerThickness / 100 * Material.BulkDensity, 3);
            }
        }

        // Temperaturleitfähigkeit a gibt das Mass für die Geschwindigkeit an,
        // mit der sich eine Temperaturänderung im Material ausbreitet:
        [Ignore]
        public double TemperatureConductivity // a in m²/s
        {
            get
            {
                if (!Material.IsValid || !IsEffective) return 0;
                return Math.Round(Material.ThermalConductivity / Convert.ToDouble(Material.BulkDensity / Material.SpecificHeatCapacity), 2);
            }
        }

        //------Konstruktor-----//

        public Layer() : base()
        {
            // Initialize the Layer properties first
            // TODO: ensure....

            // Now call the base class initializer
            //base.Initialize(this);
        }

        //------Methoden-----//

        // TODO: inherited Fields must update!
        //public void Init()
        //{
        //    base.Initialize(this);
        //}

        public Layer Copy()
        {
            var copy = new Layer();
            copy.Id = this.Id;
            copy.LayerPosition = this.LayerPosition;
            copy.Element = this.Element;
            copy.MaterialId = this.MaterialId;
            copy.Element = this.Element;
            copy.Material = this.Material;
            copy.LayerThickness = this.LayerThickness;
            copy.Effective = this.Effective;
            copy.IsSelected = false;
            copy.CreatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
            copy.InternalId = this.InternalId;
            return copy;
        }

        public override string ToString() // Überlagert vererbte standard ToString() Methode 
        {
            return LayerThickness + " cm, " + Material.Name + " (Pos.: " + LayerPosition + ")";
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = TimeStamp.GetCurrentUnixTimestamp();
        }
    }
}
