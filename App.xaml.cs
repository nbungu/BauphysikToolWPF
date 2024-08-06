using BauphysikToolWPF.Models;
using BauphysikToolWPF.Repository;
using BauphysikToolWPF.Services;
using BauphysikToolWPF.SessionData;
using BT.Logging;
using System;
using System.IO;
using System.Windows;

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

            try
            {
                Logger.SetLogFilePath();
                Logger.ClearLog();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error during Logging Setup: {exception}");
            }

            if (e.Args.Length > 0)
            {
                Logger.LogInfo($"Opening Application with Arguments: {e.Args}");

                string filePath = e.Args[0];
                if (File.Exists(filePath))
                {
                    try
                    {
                        // Load the project from the specified file
                        Project loadedProject = ApplicationServices.LoadProjectFromFile(filePath);
                        UserSaved.SelectedProject = loadedProject;
                        UserSaved.ProjectFilePath = filePath;
                        Logger.LogInfo($"Loaded Project: '{UserSaved.SelectedProject}' from Arguments!");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error reading Project from Arguments: {ex.Message}");
                    }
                }
                else
                {
                    Logger.LogWarning($"File does not exist: {filePath}");
                }
            }
            else
            {
                Logger.LogInfo("Opened Application without Arguments");
                UserSaved.SelectedProject = DatabaseAccess.QueryProjectById(1);
                Logger.LogInfo($"Loaded Project: '{UserSaved.SelectedProject}' from Database!");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.LogInfo($"Checking for Updates...");
            Updater.CheckForUpdates();

            Logger.LogInfo($"Closing Application with ExitCode: {e.ApplicationExitCode}");
        }

        // cmd test:
        //
        // "C:\Users\arnes\source\repos\BauphysikToolWPF\bin\Debug\net8.0-windows10.0.22621.0\BauphysikToolWPF.exe" "C:\Users\arnes\Desktop\project.btk"
    }
}
