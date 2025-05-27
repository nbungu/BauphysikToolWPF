using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.UI.CustomControls;

namespace BauphysikToolWPF.Services.Application
{
    public class DomainModelFactory
    {
        public static void CreateNewProject()
        {
            Project project = new Project();
            project.CreatedByUser = true;
            Session.SelectedProject = project;
            Session.ProjectFilePath = "";
            MainWindow.ShowToast("Neues Projekt erstellt!", ToastType.Success);
        }

        // TODO: EnvelopeItemFactory
    }

}
