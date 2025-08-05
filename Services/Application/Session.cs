using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Services.UI;
using System.Linq;
using static BauphysikToolWPF.Models.UI.Enums;

namespace BauphysikToolWPF.Services.Application
{
    public delegate void Notify(); // delegate (signature: return type void, no input parameters)
    public delegate void NotifyPageChanged(NavigationPage targetPage, NavigationPage? originPage = null);
    public delegate void EnvVarChangedHandler(Symbol changedVar);

    public static class Session // publisher of 'EnvVarsChanged' event
    {
        #region EventHandlers

        // The subscriber class must register to these event and handle it with the method whose signature matches Notify delegate
        public static event Notify? SelectedProjectChanged; // event
        public static event Notify? NewProjectAdded; // event
        public static event Notify? SelectedElementChanged;
        public static event Notify? NewElementAdded;
        public static event Notify? ElementRemoved;
        public static event Notify? SelectedLayerChanged;
        public static event Notify? SelectedLayerIndexChanged;
        public static event Notify? EnvelopeItemsChanged;
        public static event NotifyPageChanged? PageChanged;
        public static event EnvVarChangedHandler? EnvVarsChanged;

        // event handlers - publisher
        public static void OnSelectedProjectChanged(bool updateIsModified = true) //protected virtual method
        {
            if (SelectedProject == null) return;
            if (updateIsModified) SelectedProject.IsModified = true;
            SelectedProjectChanged?.Invoke(); //if SelectedProjectChanged is not null then call delegate
        }
        public static void OnNewProjectAdded(bool updateIsModified = true)
        {
            if (SelectedProject == null) return;
            if (updateIsModified) SelectedProject.IsModified = true;
            NewProjectAdded?.Invoke();
        }
        public static void OnSelectedElementChanged(bool updateIsModified = true)
        {
            if (SelectedProject == null) return;
            if (updateIsModified) SelectedProject.IsModified = true;
            SelectedElementChanged?.Invoke();
        }
        public static void OnNewElementAdded(bool updateIsModified = true)
        {
            if (SelectedProject == null) return;
            if (updateIsModified) SelectedProject.IsModified = true;
            NewElementAdded?.Invoke();
        }
        public static void OnElementRemoved(bool updateIsModified = true)
        {
            if (SelectedProject == null) return;
            if (updateIsModified) SelectedProject.IsModified = true;
            ElementRemoved?.Invoke();
        }
        public static void OnSelectedLayerChanged(bool updateIsModified = true)
        {
            if (SelectedProject == null) return;
            if (updateIsModified) SelectedProject.IsModified = true;
            SelectedLayerChanged?.Invoke();
        }
        public static void OnSelectedLayerIndexChanged()
        {
            if (SelectedProject == null) return;
            SelectedLayerIndexChanged?.Invoke();
        }
        public static void OnEnvVarsChanged(Symbol changedVar)
        {
            EnvVarsChanged?.Invoke(changedVar);
        }
        public static void OnEnvelopeItemsChanged(bool updateIsModified = true)
        {
            if (SelectedProject == null) return;
            if (updateIsModified) SelectedProject.IsModified = true;
            EnvelopeItemsChanged?.Invoke();
        }
        public static void OnPageChanged(NavigationPage targetPage, NavigationPage? originPage = null)
        {
            if (SelectedProject == null) return;
            PageChanged?.Invoke(targetPage, originPage);
        }

        #endregion

        public static string ProjectFilePath { get; set; } = string.Empty;

        public static Project? SelectedProject { get; set; }

        /// <summary>
        /// InternalID des ausgewählten Elements
        /// </summary>
        public static int SelectedElementId { get; set; } = -1;

        /// <summary>
        /// Zeigt auf das entsprechende Element aus dem aktuellen Projekt auf Basis der InternalID von 'SelectedElementId'
        /// </summary>
        public static Element? SelectedElement => SelectedProject?.Elements.FirstOrDefault(e => e?.InternalId == SelectedElementId, null);
    }
}
