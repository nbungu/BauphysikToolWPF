using BauphysikToolWPF.Repository.Models;
using BauphysikToolWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.UI.ViewModels
{
    public partial class Page_BuildingEnvelope_VM : ObservableObject
    {
        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Everything the user can edit or change: All objects affected by user interaction.
         */

        [ObservableProperty]
        private EnvelopeItem? _selectedEnvelopeItem;

        [ObservableProperty]
        private List<EnvelopeItem> _envelopeItems = new List<EnvelopeItem>();

        /*
        * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
        * 
        * Not Observable, not directly mutated by user input
        */

        public IEnumerable<int> ElementIds => Session.SelectedProject.Elements.Select(e => e.InternalId);
    }
}
