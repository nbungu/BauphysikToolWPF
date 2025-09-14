using BauphysikToolWPF.Models.Domain;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.Calculation
{
    public class EnvelopeCalculation
    {
        private const int _dayFactor = 24; // hours in a day
        
        /// <summary>
        /// Transmissionswärmesenken bzw. -quellen
        /// </summary>
        public enum TransmissionTransfer
        {
            ToOutside = 0,
            ThroughUnheatedRooms,
            ToAdjacentZone,
            OverGround,
            ViaThermalBridges,
        }
        public static readonly Dictionary<TransmissionTransfer, string> TransmissionTransferMapping = new()
        {
            { TransmissionTransfer.ToOutside, "nach außen" },
            { TransmissionTransfer.ThroughUnheatedRooms, "durch unbeheizte oder ungekühlte Räume" },
            { TransmissionTransfer.ToAdjacentZone, "zu angrenzenden Zonen" },
            { TransmissionTransfer.OverGround, "über das Erdreich" },
            //{ TransmissionTransfer.ViaThermalBridges, "über Wärmebrücken" }
        };

        /// <summary>
        /// Lüftungswärmesenken bzw. -quellen
        /// </summary>
        public enum VentilationTransfer
        {
            Infiltration,
            WindowVentilation,
            MechanicalVentilation,
            AirExchangeBetweenZones,
        }
        public static readonly Dictionary<VentilationTransfer, string> VentilationTransferMapping = new()
        {
            { VentilationTransfer.Infiltration, "Infiltration" },
            { VentilationTransfer.WindowVentilation, "Fensterlüftung" },
            { VentilationTransfer.MechanicalVentilation, "Mechanische Lüftung" },
            { VentilationTransfer.AirExchangeBetweenZones, "Luftwechsel zwischen Zonen" }
        };

        /// <summary>
        /// Strahlungswärmesenken bzw. -quellen
        /// </summary>
        public enum RadiationTransfer
        {
            ViaTransparentElements,
            ViaOpaqueElements,
            ViaUnheatedGlassPorch,
        }
        public static readonly Dictionary<RadiationTransfer, string> RadiationTypeMapping = new()
        {
            { RadiationTransfer.ViaTransparentElements, "Über transparente Bauteile" },
            { RadiationTransfer.ViaOpaqueElements, "Über opake Bauteile" },
            { RadiationTransfer.ViaUnheatedGlassPorch, "Über unbeheizte oder ungekühlte Glasvorbauten" }
        };

        /// <summary>
        /// Möglichkeiten zur Bestimmung von Wärmebrücken nach DIN V 18599-2.
        /// 6.2.5 Transmission über Wärmebrücken
        /// </summary>
        public enum ThermalBridgeSurchargeType
        {
            NoCalculation,
            WithInternalInsulationAndIntegralSolidCeiling,
            ProofOfEquivalenceAllCategoryB,
            ProofOfEquivalenceMixedCategories,
            ProjectRelatedValue,
            Detailed,
            // TODO: Gl. (59) und (60) berücksichtigen
        }
        public static readonly Dictionary<ThermalBridgeSurchargeType, string> ThermalBridgeSurchargeMapping = new()
        {
            { ThermalBridgeSurchargeType.NoCalculation, "Ohne Nachweis" },
            { ThermalBridgeSurchargeType.WithInternalInsulationAndIntegralSolidCeiling, "Außenbauteile mit innenliegender Dämmschicht und einbindender Massivdecke" },
            { ThermalBridgeSurchargeType.ProofOfEquivalenceAllCategoryB, "DIN 4108 Beiblatt 2: Gleichwertigkeit bei allen Anschlüssen nach Kategorie B erfüllt." },
            { ThermalBridgeSurchargeType.ProofOfEquivalenceMixedCategories, "DIN 4108 Beiblatt 2: Gleichwertigkeit bei allen Anschlüssen nach Kategorie B oder Kategorie A erfüllt." },
            { ThermalBridgeSurchargeType.ProjectRelatedValue, "Projektbezogener Wärmebrückenzuschlag" },
            { ThermalBridgeSurchargeType.Detailed, "Eigene Wärmebrückenberechnung" }
        };
        public static readonly Dictionary<ThermalBridgeSurchargeType, double> ThermalBridgeSurchargeValues = new()
        {
            { ThermalBridgeSurchargeType.NoCalculation, 0.1 },
            { ThermalBridgeSurchargeType.WithInternalInsulationAndIntegralSolidCeiling, 0.15 },
            { ThermalBridgeSurchargeType.ProofOfEquivalenceAllCategoryB, 0.03 },
            { ThermalBridgeSurchargeType.ProofOfEquivalenceMixedCategories, 0.05 },
            { ThermalBridgeSurchargeType.ProjectRelatedValue, 0.1 }, // Placeholder, actual values would depend on the user entry
        };

        public List<EnvelopeItem> InputCollection { get; set; }
        public EnvelopeCalculationConfig Config { get; set; }


        public double HeatedRoomVolume => InputCollection.Sum(itm => itm.RoomVolumeNet);
        public double VentilatedVolume => Config.IsEFH ? 0.76 * HeatedRoomVolume : 0.80 * HeatedRoomVolume;
        public double UsableArea => InputCollection.Sum(itm => itm.RoomAreaNet);
        public double LivingArea => Config.IsEFH ? UsableArea / 1.35 : UsableArea / 1.20;

        public double EnvelopeArea => InputCollection.Sum(itm => itm.EnvelopeArea);
        public double AVRatio { get; private set; }
        public double AnnualPrimaryEnergy { get; private set; }
        public double AnnualPrimaryEnergyPerArea => AnnualPrimaryEnergy / UsableArea;
        public double AirExchangeRateInf { get; private set; }
        public double AirExchangeRateWin { get; private set; }
        public double AirExchangeRateMech { get; private set; }
        public double AirExchangeRateUe { get; private set; }

        #region Fx Control

        private FxType _fxMode = FxType.Default;
        public FxType FxMode
        {
            get => _fxMode;
            set
            {
                _fxMode = value;
                FxGlobalValue = FxTypeValues.TryGetValue(value, out var fx) ? fx : 0.0;
            }
        }
        public double? FxGlobalValue { get; set; } = FxTypeValues[FxType.Default];
        public bool IsFxCustomValue => FxMode == FxType.Custom || FxMode == FxType.Fictional;
        public bool IsFxGlobalValue => FxGlobalValue != null;

        public enum FxType
        {
            Default,
            FromTable,
            IncludedInConstructiveUValue,
            Fictional,
            Custom,
        }
        public static readonly Dictionary<FxType, string> FxTypeMapping = new()
        {
            { FxType.Default, "Standardwert (keine Abminderung)" },
            { FxType.FromTable, "Einzeln, für jede Bauteilart (DIN V 18599-2, Tabelle 5 und 6)" },
            { FxType.IncludedInConstructiveUValue, "Wirkung der unbeheizten Zone bereits im U-Wert berücksichtigt" },
            { FxType.Fictional, "fiktiver Fx-Wert (DIN V 18599-2, Anhang F)" },
            { FxType.Custom, "Benutzerdefiniert" },

        };
        public static readonly Dictionary<FxType, double?> FxTypeValues = new()
        {
            { FxType.Default, 1.0 },
            { FxType.FromTable, null }, // Placeholder, actual values would depend on the table used
            { FxType.IncludedInConstructiveUValue, 1.0 },
            { FxType.Fictional, 1.0 }, // Placeholder, actual value would depend on the calculation
            { FxType.Custom, 1.0 },
        };

        #endregion

        public double ThermalBridgeSurchargeValue { get; set; } = ThermalBridgeSurchargeValues[ThermalBridgeSurchargeType.NoCalculation];
        public bool IsThermalBridgeSurchargeCustomValue => ThermalBridgeSurcharge == ThermalBridgeSurchargeType.ProjectRelatedValue; 

        private ThermalBridgeSurchargeType _thermalBridgeSurcharge = ThermalBridgeSurchargeType.NoCalculation;
        public ThermalBridgeSurchargeType ThermalBridgeSurcharge
        {
            get => _thermalBridgeSurcharge;
            set
            {
                _thermalBridgeSurcharge = value;
                ThermalBridgeSurchargeValue = ThermalBridgeSurchargeValues.TryGetValue(value, out var surcharge) ? surcharge : 0.0;
            }
        }

        public double ZoneTempInteriorHeating { get; private set; } // TODO: Zone temperature calculation
        public double ZoneTempInteriorCooling { get; private set; } // TODO: Zone temperature calculation
        public double ZoneTempInterior { get; private set; } // TODO: Zone temperature calculation
        //public double SpecificTransmissionHeatTransferCoef => InputCollection.Sum(itm => itm.HeatTransferLoss);

        //public double TransmissionHeatTransferCoefSum => _inputCollection.Sum(itm => itm.TransmissionHeatTransferCoef);
        //public double TransmissionHeatTransferCoefProject => TransmissionHeatTransferCoefSum + ThermalBridgeSurcharge * EnvelopeArea;

        //public IEnumerable<IPropertyItem> PropertyBag => new List<IPropertyItem>
        //{
        //    new PropertyItem<double>("beheiztes Volumen", Symbol.Volume, () => HeatedRoomVolume) { SymbolSubscriptText = "e"},
        //    new PropertyItem<double>("belüftetes Volumen", Symbol.Volume, () => VentilatedVolume) { SymbolSubscriptText = "bel."},
        //    new PropertyItem<double>("Hüllfläche", Symbol.Area, () => EnvelopeArea) { SymbolSubscriptText = "Hülle", Comment = "wärmeübertragende Umfassungsfläche. Grenze zwischen konditionierten Räumen und der Außenluft, dem Erdreich oder nicht konditionierten Räumen."},
        //    new PropertyItem<double>("Nutzfläche", Symbol.Area, () => UsableArea) { SymbolSubscriptText = "N"},
        //    new PropertyItem<double>("Wohnfläche", Symbol.Area, () => LivingArea) { SymbolSubscriptText = "wo"},
        //    new PropertyItem<double>(Symbol.AToVRatio, () => AVRatio),
        //    new PropertyItem<double>(Symbol.PrimaryEnergy, () => PrimaryEnergy),
        //    new PropertyItem<double>(Symbol.PrimaryEnergyPerArea, () => PrimaryEnergyPerArea),
        //    new PropertyItem<double>(Symbol.AirExchangeRate, () => AirExchangeRate),
        //};

        public double TotalArea { get; set; }         // Summe A
        public double HT_Total { get; set; }          // Σ(U*A*f_x + Ψ*l) + ΔUWB*A
        public double HT_Specific { get; set; }       // H'_T = HT_Total / A

        public EnvelopeCalculation()
        {
            InputCollection = new List<EnvelopeItem>();
            Config = new EnvelopeCalculationConfig();
        }

        public EnvelopeCalculation(IEnumerable<EnvelopeItem> inputCollection)
        {
            InputCollection = inputCollection.ToList();
            Config = new EnvelopeCalculationConfig();
        }
        public EnvelopeCalculation(IEnumerable<EnvelopeItem> inputCollection, EnvelopeCalculationConfig config)
        {
            InputCollection = inputCollection.ToList();
            Config = config;
        }

        // Spezifischer Transmissionswärmetransferkoeffizient (HT’)
        public double CalculateSpecificTransmissionCoefficient()
        {
            double totalArea = GetTotalEnvelopeArea();
            if (totalArea <= 0) return 0;

            return CalculateTotalTransmissionCoefficient() / totalArea;
        }

        // Hüllfläche (A)
        public double GetTotalEnvelopeArea() => InputCollection.Sum(i => i.EnvelopeArea);


        // Transmissionswärmetransferkoeffizient der gesamten Gebäudehülle (HT)
        public double CalculateTotalTransmissionCoefficient()
        {
            double ht = 0;

            foreach (var item in InputCollection)
            {

                // Alle Transmissionsanteile: HT_D; HT_iu, HT_s
                // nach Außen, geg. unbeheizte Räume, zu angrenzende Zonen, geg. Erdreich
                ht += GetElementTransmissionHeatLoss(item, (TransmissionTransfer)item.TransmissionTransferType);

                // Wärmebrückenanteile HT_WB
                ht += GetThermalBridgeTransmissionHeatLoss(item);
            }
            
            return ht;
        }

        // HT_D; HT_iu, HT_s
        private double GetElementTransmissionHeatLoss(EnvelopeItem itm, TransmissionTransfer transferType)
        {
            switch (transferType)
            {
                case TransmissionTransfer.ToOutside:
                case TransmissionTransfer.ToAdjacentZone:
                    return itm.UValue * itm.EnvelopeArea;

                case TransmissionTransfer.OverGround:
                case TransmissionTransfer.ThroughUnheatedRooms:
                    return itm.UValue * itm.EnvelopeArea * itm.FxValue;

                //case TransmissionTransfer.ViaThermalBridges:
                //    return ThermalBridgeSurchargeValue * itm.EnvelopeArea;

                default: return itm.UValue * itm.EnvelopeArea;
            }
        }

        // HT_WB
        private double GetThermalBridgeTransmissionHeatLoss(EnvelopeItem itm)
        {
            // Distinct between different methods of calculating thermal bridges
            // A: pauschaler Zuschlag nach 6.2.5
            // B: Projektbezogener Wert nach Anhang H
            // C: Detailliert / Einzelberechnug
            
            // A and B:
            return itm.ThermalBridge.DeltaUWB * itm.EnvelopeArea;
        }

    }
}
