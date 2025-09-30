using BauphysikToolWPF.Models.Domain;
using BauphysikToolWPF.Models.Domain.Helper;
using BauphysikToolWPF.Services.Application;
using BT.Logging;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BauphysikToolWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //
        private bool _simulateFirstStart = false;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            #region Logging Setup

            try
            {
                Logger.SetLogFilePath();
                Logger.ClearLog();
            }
            catch (Exception ex)
            {
                // Minimal console output; full exception goes to developer trace if configured
                Console.WriteLine("Logging setup failed: " + ex.Message);
            }

            #endregion

            // Check for --simulateFirstStart in args
            if (e.Args.Any(arg => arg.Equals("--simulateFirstStart", StringComparison.OrdinalIgnoreCase)))
            {
                _simulateFirstStart = true;
                Logger.LogWarning("Simulating first start. Force replace all repository files with initial default files");
            }

            UpdaterManager.SetupFile(_simulateFirstStart);
            RecentProjectsManager.SetupFile(_simulateFirstStart);
            DatabaseManager.SetupFile(_simulateFirstStart);

            // Only on the very first program start, add the Demoprojekt.btk to the recently used projects
            if (UpdaterManager.ProgramVersionState.LastUpdateCheck == 0)
            {
                RecentProjectsManager.AddRecentProject(PathService.UserDemoProjectFilePath);
            }

            if (e.Args.Length > 0)
            {
                // Mask args when writing to general info logs (don't print secrets/file contents).
                Logger.LogInfo($"Opening Application with {e.Args.Length} argument(s).");

                string rawFilePath = e.Args[0];

                if (rawFilePath == "--simulateFirstStart") return;

                // Sanitize and validate path before using it
                try
                {
                    // Reject UNC / network paths by default (optional)
                    if (PathService.IsUncPath(rawFilePath))
                    {
                        Logger.LogWarning("Rejected UNC/network path provided as startup argument.");
                        return;
                    }

                    // Normalize and validate
                    string fullPath = Path.GetFullPath(rawFilePath);

                    if (!PathService.IsAllowedExtension(fullPath, new[] { ".btk", ".btkproj" }))
                    {
                        Logger.LogWarning($"Rejected file with disallowed extension: {PathService.MaskPath(fullPath)}");
                        return;
                    }

                    if (!File.Exists(fullPath))
                    {
                        Logger.LogWarning($"File does not exist: {PathService.MaskPath(fullPath)}");
                        return;
                    }

                    if (!PathService.IsReasonableFileSize(fullPath, maxBytes: 10 * 1024 * 1024)) // 10 MB limit example
                    {
                        Logger.LogWarning($"Rejected file due to excessive size: {PathService.MaskPath(fullPath)}");
                        return;
                    }

                    // Load the project from the specified file
                    Project loadedProject = ProjectSerializer.GetProjectFromFile(rawFilePath);
                    Session.SelectedProject = loadedProject;
                    Session.ProjectFilePath = rawFilePath;
                    RecentProjectsManager.AddRecentProject(rawFilePath);
                    // TODO Testing:
                    Session.SelectedProject.RenderMissingElementImages(withDecorations: false);
                    Logger.LogInfo($"Loaded Project: '{Session.SelectedProject}'");
                    
                }
                catch (Exception ex)
                {
                    // Do not expose stack trace to general logs — log a short message and optionally a developer-only trace
                    Logger.LogError($"Error processing startup argument: {ex.Message}");
                }
            }
            else
            {
                Logger.LogInfo("Opened Application without Arguments");
                Logger.LogInfo("Startup without Project selected!");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.LogInfo($"Checking for Updates...");
            UpdaterManager.CheckForUpdates();

            Logger.LogInfo($"Closing Application with ExitCode: {e.ApplicationExitCode}");
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && !textBox.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                textBox.Focus();
            }
        }
        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox) textBox.SelectAll();
        }

        // cmd test:
        //
        // "C:\Users\arnes\source\repos\BauphysikToolWPF\bin\Debug\net8.0-windows10.0.22621.0\BauphysikToolWPF.exe" "C:\Users\arnes\Desktop\debug_project.btk"
    }
}
