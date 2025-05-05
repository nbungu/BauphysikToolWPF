using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.UI;
using System.Collections.Generic;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Calculation
{
    public class EnvelopeCalculation
    {
        private readonly EnvelopeCalculationConfig _config;
        
        public double HeatedRoomVolume { get; private set; }
        public double VentilatedVolume => _config.IsEFH ? 0.76 * HeatedRoomVolume : 0.80 * HeatedRoomVolume;
        public double EnvelopeArea { get; private set; }
        public double UsableArea { get; private set; }
        public double LivingArea => _config.IsEFH ? UsableArea / 1.35 : UsableArea / 1.20;
        public double AVRatio { get; private set; }
        public double PrimaryEnergy { get; private set; }
        public double PrimaryEnergyPerArea => PrimaryEnergy / UsableArea;
        public double AirExchangeRate { get; private set; }

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


        public EnvelopeCalculation(IEnumerable<EnvelopeItem> inputCollection, EnvelopeCalculationConfig config = null)
        {
            _config = config ?? new EnvelopeCalculationConfig();
        }

        public void CalculateEnvelope()
        {
            // Calculation logic here
        }

        public void DisplayResults()
        {
            // Display results logic here
        }
    }
}
