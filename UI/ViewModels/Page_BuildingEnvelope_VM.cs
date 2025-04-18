using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Services.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using static BauphysikToolWPF.Models.Domain.Helper.Enums;

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
        private List<EnvelopeItem> _envelopeItems = Session.SelectedProject?.EnvelopeItems ?? new List<EnvelopeItem>(0);

        /*
         * MVVM Capsulated Properties + Triggered + Updated by other Properties (NotifyPropertyChangedFor)
         * 
         * Not Observable, not directly mutated by user input
         */

        public EnvelopeItem? SelectedEnvelopeItem => Session.SelectedEnvelopeItem; // Cannot be directly mutated via binding like ListViewItems, since ints wrapped as button in a WrapPanel

        public List<string> OrientationTypeList => OrientationTypeMapping.Values.ToList();
    }
}
