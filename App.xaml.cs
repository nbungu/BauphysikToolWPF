using System.IO;
using BauphysikToolWPF.Models;
using System.Windows;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.SessionData;
using System;
using BauphysikToolWPF.Repository;

namespace BauphysikToolWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            // Log the arguments to a file
            ApplicationServices.AppendToLogFile(e.Args);

            if (e.Args.Length > 0)
            {
                string filePath = e.Args[0];
                if (File.Exists(filePath))
                {
                    try
                    {
                        // Load the project from the specified file
                        Project loadedProject = ApplicationServices.LoadProjectFromFile(filePath);
                        UserSaved.SelectedProject = loadedProject;
                        ApplicationServices.AppendToLogFile($"Successfully loaded: {UserSaved.SelectedProject}");
                        MessageBox.Show($"Success"); 
                    }
                    catch (Exception ex)
                    {
                        // Log any exceptions
                        ApplicationServices.AppendToLogFile($"Error: {ex.Message}");
                        MessageBox.Show($"Error loading project: {ex.Message}");
                    }
                }
                else
                {
                    ApplicationServices.AppendToLogFile($"File does not exist: {filePath}");
                    MessageBox.Show("File does not exist.");
                }
            }
            else
            {
                ApplicationServices.AppendToLogFile($"No .btk file as Arguments specified");
                UserSaved.SelectedProject = DatabaseAccess.QueryProjectById(1);
                ApplicationServices.AppendToLogFile($"Select Project from Database: {UserSaved.SelectedProject}");
            }
        }

        // cmd test:
        //
        // "C:\Users\arnes\source\repos\BauphysikToolWPF\bin\Debug\net8.0-windows10.0.22621.0\BauphysikToolWPF.exe" "C:\Users\arnes\Desktop\project.btk"
    }
}
