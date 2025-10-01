using BauphysikToolWPF.Services.Application;
using BT.Logging;
using System;
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

            #region Handle Arguments
            
            var args = ArgumentParser.Parse(e.Args);
            
            // Handle first start simulation
            if (args.SimulateFirstStart)
            {
                Logger.LogWarning("Simulating first start. Force replace all repository files with initial default files");
            }

            UpdaterManager.SetupFile(args.SimulateFirstStart);
            RecentProjectsManager.SetupFile(args.SimulateFirstStart);
            DatabaseManager.SetupFile(args.SimulateFirstStart);

            // Only on the very first program start, add the Demoprojekt.btk to the recently used projects
            if (UpdaterManager.ProgramVersionState.LastUpdateCheck == 0)
            {
                RecentProjectsManager.AddRecentProject(PathService.UserDemoProjectFilePath);
            }

            if (!string.IsNullOrEmpty(args.ProjectPath))
            {
                Logger.LogInfo("Startup with .btk file");
                ProjectLoader.TryOpenProject(args.ProjectPath, isModifiedState: false);
            }
            else
            {
                Logger.LogInfo("Blank Startup (without .btk file)");
            }

            #endregion
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
