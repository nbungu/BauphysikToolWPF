using BauphysikToolWPF.Models.Domain;
using System;
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
            ToOutside,
            ThroughUnheatedRooms,
            ToAdjacentZone,
            OverGround,
            ViaThermalBridges,
        }
        public static readonly Dictionary<TransmissionTransfer, string> TransmissionTransferMapping = new()
        {
            { TransmissionTransfer.ToOutside, "direkte Transmission nach außen" },
            { TransmissionTransfer.ThroughUnheatedRooms, "Transmission durch unbeheizte oder ungekühlte Räume" },
            { TransmissionTransfer.ToAdjacentZone, "Transmission zu angrenzenden Zonen" },
            { TransmissionTransfer.OverGround, "Transmission über das Erdreich" },
            { TransmissionTransfer.ViaThermalBridges, "Transmission über Wärmebrücken" }
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
            ProofOfEquivalence_CatB,
            ProofOfEquivalence_CatMixed,
            ProofOfEquivalence_CatNotMet,
            ProjectRelatedValue
        }
        public static readonly Dictionary<ThermalBridgeSurchargeType, string> ThermalBridgeSurchargeMapping = new()
        {
            { ThermalBridgeSurchargeType.NoCalculation, "Ohne Nachweis" },
            { ThermalBridgeSurchargeType.WithInternalInsulationAndIntegralSolidCeiling, "Außenbauteile mit innenliegender Dämmschicht und einbindender Massivdecke" },
            { ThermalBridgeSurchargeType.ProofOfEquivalence_CatB, "DIN 4108 Beiblatt 2: Gleichwertigkeit bei allen Anschlüssen nach Kategorie B erfüllt." },
            { ThermalBridgeSurchargeType.ProofOfEquivalence_CatMixed, "DIN 4108 Beiblatt 2: Gleichwertigkeit bei allen Anschlüssen nach Kategorie B oder Kategorie A erfüllt." },
            { ThermalBridgeSurchargeType.ProofOfEquivalence_CatNotMet, "DIN 4108 Beiblatt 2: Gleichwertigkeit nicht bei allen Anschlüssen erfüllt." },
            { ThermalBridgeSurchargeType.ProjectRelatedValue, "Projektbezogener Wärmebrückenzuschlag" }
        };
        public static readonly Dictionary<ThermalBridgeSurchargeType, double> ThermalBridgeSurchargeValues = new()
        {
            { ThermalBridgeSurchargeType.NoCalculation, 0.1 },
            { ThermalBridgeSurchargeType.WithInternalInsulationAndIntegralSolidCeiling, 0.15 },
            { ThermalBridgeSurchargeType.ProofOfEquivalence_CatB, 0.03 },
            { ThermalBridgeSurchargeType.ProofOfEquivalence_CatMixed, 0.05 },
        };


        public List<EnvelopeItem> InputCollection { get; set; }
        public EnvelopeCalculationConfig Config { get; set; }
        public double HeatedRoomVolume { get; private set; }
        //public double VentilatedVolume => _config.IsEFH ? 0.76 * HeatedRoomVolume : 0.80 * HeatedRoomVolume;
        public double EnvelopeArea => InputCollection.Sum(itm => itm.EnvelopeArea);
        public double UsableArea { get; private set; }
        public double LivingArea => Config.IsEFH ? UsableArea / 1.35 : UsableArea / 1.20;
        public double AVRatio { get; private set; }
        public double PrimaryEnergy { get; private set; }
        public double PrimaryEnergyPerArea => PrimaryEnergy / UsableArea;
        public double AirExchangeRateInf { get; private set; }
        public double AirExchangeRateWin { get; private set; }
        public double AirExchangeRateMech { get; private set; }
        public double AirExchangeRateUe { get; private set; }

        public double ThermalBridgeSurchargeValue { get; set; } = ThermalBridgeSurchargeValues[ThermalBridgeSurchargeType.NoCalculation];
        public bool ThermalBridgeSurchargeCustomValue => (int)ThermalBridgeSurcharge <= 3; 

        private ThermalBridgeSurchargeType _thermalBridgeSurcharge;
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

        public void CalculateEnvelope()
        {
            // Calculation logic here
        }

        public void DisplayResults()
        {
            // Display results logic here
        }

        /// <summary>
        /// DIN V 18599-2, C.4.1 bis C.4.12: Wärmeströme durch Transmission
        /// </summary>
        /// <param name="heatTransferCoef">der Wärmetransferkoeffizient. a) zwischen der betrachteten Gebäudezone und außen; b) über das Erdreich; c) zwischen der betrachteten Gebäudezone und einem angrenzenden Bereich</param>
        /// <param name="tempInt"></param>
        /// <param name="tempExt">das Tagesmittel der Außentemperatur am Auslegungstag (siehe DIN V 18599-10:2018-09, Tabelle 9)</param>
        /// <returns></returns>
        private double CalcHeatFlowQdot(double heatTransferCoef, double tempInt, double tempExt)
        {
            return heatTransferCoef * Math.Abs(tempInt - tempExt);
        }

        //private double GetQ()
        //{

        //}

        ///// <summary>
        ///// DIN V 18599-2, 6.7.1 Wirksame Wärmekapazität C_wirk
        ///// </summary>
        ///// <returns></returns>
        //private double SimplifiedEffectiveHeatCapacity()
        //{
        //    return 0.0;
        //}

        //private double TimeConstantOfZone(RoomUsageType zone, double cWirk, double heatTransferCoef)
        //{
        //    return cWirk / heatTransferCoef;
        //}

        // H_T,x
        private double TransmissionHeatTransferCoef(List<EnvelopeItem> itms, TransmissionTransfer transferType)
        {
            switch (transferType)
            {
                case TransmissionTransfer.ToOutside:
                case TransmissionTransfer.ThroughUnheatedRooms:
                case TransmissionTransfer.ToAdjacentZone:
                case TransmissionTransfer.OverGround: // TODO: H_G nach DIN EN ISO 13370
                    return itms.Sum(itm => itm.UValue * itm.EnvelopeArea);
                case TransmissionTransfer.ViaThermalBridges:
                    return ThermalBridgeSurchargeValue * itms.Sum(itm => itm.EnvelopeArea);
                default: return itms.Sum(itm => itm.UValue * itm.EnvelopeArea);
            }
        }
        // H_T'
        private double SpecificTransmissionHeatTransferCoef(List<EnvelopeItem> itms)
        {
            // TODO: elemente aufteilen nach TransmissionTransfer

            return 0.0;
        }

        // Q_T,x
        private double QTransmission(List<EnvelopeItem> itms, TransmissionTransfer transferType, double tempInt, double tempExt)
        {
            switch (transferType)
            {
                // 6.2.1 Direkte Transmission nach außen (45) und (46)
                case TransmissionTransfer.ToOutside:
                    var htd = TransmissionHeatTransferCoef(itms, transferType);
                    var htwb = TransmissionHeatTransferCoef(itms, TransmissionTransfer.ViaThermalBridges);
                    return (htd + htwb) * Math.Abs(tempInt - tempExt) * _dayFactor;
                case TransmissionTransfer.ThroughUnheatedRooms:
                    var htiu = TransmissionHeatTransferCoef(itms, transferType);
                    return htiu * Math.Abs(tempInt - tempExt) * _dayFactor;
                case TransmissionTransfer.ToAdjacentZone:
                    var htiz = TransmissionHeatTransferCoef(itms, transferType);
                    return htiz * Math.Abs(tempInt - tempExt) * _dayFactor;
                case TransmissionTransfer.OverGround:
                    var hts = TransmissionHeatTransferCoef(itms, transferType);
                    return hts * Math.Abs(tempInt - tempExt) * _dayFactor;
                //Falls der Wärmeaustrag über das Erdreich einen erheblichen Anteil an den
                //Gesamtwärmesenken ausmacht, ist abzuwägen, ob eine detaillierte Berechnung des Wärmestroms über das
                //Erdreich nach DIN EN ISO 13370:2018 - 03, C.1, durchzuführen ist
                default: return 0.0;
            }
        }

        private double QTransmissionTotal()
        {
            // TODO: elemente aufteilen nach TransmissionTransfer
            return 0.0;
        }

        // H_V,x
        private void VentilationHeatTransferCoef(EnvelopeItem itm, VentilationTransfer ventilationType)
        {

        }
    }
}
