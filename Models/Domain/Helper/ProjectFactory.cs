using BauphysikToolWPF.Services.Application;
using BauphysikToolWPF.UI.CustomControls;

namespace BauphysikToolWPF.Models.Domain.Helper
{
    public class ProjectFactory
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
