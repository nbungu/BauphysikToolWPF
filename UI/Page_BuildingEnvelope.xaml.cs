using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using System.Windows.Controls;

namespace BauphysikToolWPF.UI
{
    /// <summary>
    /// Interaktionslogik für Page_BuildingEnvelope.xaml
    /// </summary>
    public partial class Page_BuildingEnvelope : UserControl
    {
        public Page_BuildingEnvelope()
        {
            InitalizeEnvelopeItems();

            InitializeComponent();
        }

        private void InitalizeEnvelopeItems()
        {
            if (Session.SelectedProject is null) return;

            Session.SelectedProject.AssignInternalIdsToEnvelopeItems();
        }
    }
}
