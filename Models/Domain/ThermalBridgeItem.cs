using BauphysikToolWPF.UI.CustomControls;

namespace BauphysikToolWPF.Models.Domain
{
    public class ThermalBridgeItem
    {
        private double _psiValue;
        public double PsiValue
        {
            get => _psiValue;
            set
            {
                if (value < 0) MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                _psiValue = value;
            }
        }

        private double _length;
        public double Length
        {
            get => _length;
            set
            {
                if (value < 0) MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                _length = value;
            }
        }

        private double _detaUwb;
        public double DeltaUWB
        {
            get => _detaUwb;
            set
            {
                if (value < 0) MainWindow.ShowToast("Wert darf nicht negativ sein!", ToastType.Error);
                _detaUwb = value;
            }
        }

        public ThermalBridgeItem Copy()
        {
            return new ThermalBridgeItem
            {
                PsiValue = this.PsiValue,
                Length = this.Length,
                DeltaUWB = this.DeltaUWB
            };
        }
    }
}
