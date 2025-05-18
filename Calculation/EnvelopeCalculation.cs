using System;
using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.UI;
using System.Collections.Generic;
using System.Linq;
using static BauphysikToolWPF.Models.Database.Helper.Enums;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Calculation
{
    public class EnvelopeCalculation
    {
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
        public static readonly Dictionary<TransmissionTransfer, string> TransmissionTypeMapping = new()
        {
            { TransmissionTransfer.ToOutside, "direkte Transmission nach außen nach 6.2.1" },
            { TransmissionTransfer.ThroughUnheatedRooms, "Transmission durch unbeheizte oder ungekühlte Räume nach 6.2.2" },
            { TransmissionTransfer.ToAdjacentZone, "Transmission zu angrenzenden Zonen nach 6.2.3" },
            { TransmissionTransfer.OverGround, "Transmission über das Erdreich nach 6.2.1 oder 6.2.4" },
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
        public static readonly Dictionary<VentilationTransfer, string> VentilationTypeMapping = new()
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
        /// Möglichkeiten zur Bestimmung von Wärmebrücken nach DIN V 18599-2
        /// </summary>
        public enum ThermalBridgeSurchargeType
        {
            NoCalculation,
            ProofOfEquivalence,
            ProjectRelatedValue
        }
        public static readonly Dictionary<ThermalBridgeSurchargeType, string> ThermalBridgeSurchargeMapping = new()
        {
            { ThermalBridgeSurchargeType.NoCalculation, "Ohne Nachweis" },
            { ThermalBridgeSurchargeType.ProofOfEquivalence, "Überprüfung der Gleichwertigkeit nach DIN 4108 Beiblatt 2" },
            { ThermalBridgeSurchargeType.ProjectRelatedValue, "Projektbezogener Wärmebrückenzuschlag" }
        };

        private readonly List<EnvelopeItem> _inputCollection;
        private readonly EnvelopeCalculationConfig _config;
        
        public double HeatedRoomVolume { get; private set; }
        public double VentilatedVolume => _config.IsEFH ? 0.76 * HeatedRoomVolume : 0.80 * HeatedRoomVolume;
        public double EnvelopeArea => _inputCollection.Sum(itm => itm.EnvelopeArea);
        public double UsableArea { get; private set; }
        public double LivingArea => _config.IsEFH ? UsableArea / 1.35 : UsableArea / 1.20;
        public double AVRatio { get; private set; }
        public double PrimaryEnergy { get; private set; }
        public double PrimaryEnergyPerArea => PrimaryEnergy / UsableArea;
        public double AirExchangeRate { get; private set; }
        public double ThermalBridgeSurcharge { get; private set; }
        public double TransmissionHeatTransferCoefSum => _inputCollection.Sum(itm => itm.TransmissionHeatTransferCoef);
        public double TransmissionHeatTransferCoefProject => TransmissionHeatTransferCoefSum + ThermalBridgeSurcharge * EnvelopeArea;

        public IEnumerable<IPropertyItem> PropertyBag => new List<IPropertyItem>
        {
            new PropertyItem<double>("beheiztes Volumen", Symbol.Volume, () => HeatedRoomVolume) { SymbolSubscriptText = "e"},
            new PropertyItem<double>("belüftetes Volumen", Symbol.Volume, () => VentilatedVolume) { SymbolSubscriptText = "bel."},
            new PropertyItem<double>("Hüllfläche", Symbol.Area, () => EnvelopeArea) { SymbolSubscriptText = "Hülle", Comment = "wärmeübertragende Umfassungsfläche. Grenze zwischen konditionierten Räumen und der Außenluft, dem Erdreich oder nicht konditionierten Räumen."},
            new PropertyItem<double>("Nutzfläche", Symbol.Area, () => UsableArea) { SymbolSubscriptText = "N"},
            new PropertyItem<double>("Wohnfläche", Symbol.Area, () => LivingArea) { SymbolSubscriptText = "wo"},
            new PropertyItem<double>(Symbol.AToVRatio, () => AVRatio),
            new PropertyItem<double>(Symbol.PrimaryEnergy, () => PrimaryEnergy),
            new PropertyItem<double>(Symbol.PrimaryEnergyPerArea, () => PrimaryEnergyPerArea),
            new PropertyItem<double>(Symbol.AirExchangeRate, () => AirExchangeRate),
        };

        public EnvelopeCalculation()
        {
            _inputCollection = new List<EnvelopeItem>();
            _config = new EnvelopeCalculationConfig();
        }
        public EnvelopeCalculation(IEnumerable<EnvelopeItem> inputCollection, EnvelopeCalculationConfig config)
        {
            _inputCollection = inputCollection.ToList();
            _config = config;
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

        /// <summary>
        /// DIN V 18599-2, 6.7.1 Wirksame Wärmekapazität C_wirk
        /// </summary>
        /// <returns></returns>
        private double SimplifiedEffectiveHeatCapacity()
        {
            return 0.0;
        }

        private double TimeConstantOfZone(RoomUsageType zone, double cWirk, double heatTransferCoef)
        {
            return cWirk / heatTransferCoef;
        }

        private void HeatTransferCoef(RoomUsageType zone)
        {

        }

        private void VentilationHeatTransferCoef(RoomUsageType zone)
        {

        }
    }
}
