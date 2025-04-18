using BauphysikToolWPF.Services.Application;
using BT.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows;
using BauphysikToolWPF.Repositories;

namespace BauphysikToolWPF.UI.ViewModels
{
    //ViewModel for AddElementWindow.xaml: Used in xaml as "DataContext"
    public partial class InfoWindow_VM : ObservableObject
    {
        // Called by 'InitializeComponent()' from AddElementWindow.cs due to Class-Binding in xaml via DataContext
        public string Title => "";

        /*
         * MVVM Commands - UI Interaction with Commands
         * 
         * Update ONLY UI-Used Values by fetching from Database!
         */

        [RelayCommand]
        private void OpenWebsite(Window? window)
        {
            if (!string.IsNullOrWhiteSpace(Website))
            {
                try
                {
                    window?.Close();
                    // Use the Process.Start method to open the URL in the default web browser
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Website,
                        UseShellExecute = true // This is required to use the shell to open the URL
                    });
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur while trying to open the website
                    Logger.LogError($"Couldn't open Website: {Website}, {ex.Message}");
                }
            }
            else
            {
                Logger.LogWarning("Website URL is not set.");
            }
        }

        [RelayCommand]
        private void OpenLicense(Window? window)
        {
            if (!string.IsNullOrWhiteSpace(LicensePath))
            {
                try
                {
                    window?.Close();
                    // Use the Process.Start method to open the URL in the default web browser
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = LicensePath,
                        UseShellExecute = true // This is required to use the shell to open the URL
                    });
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur while trying to open the website
                    Logger.LogError($"Couldn't open Website: {LicensePath}, {ex.Message}");
                }
            }
            else
            {
                Logger.LogWarning("Website URL is not set.");
            }
        }

        [RelayCommand]
        private void Cancel(Window? window) => window?.Close();  // To be able to Close EditElementWindow from within this ViewModel

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private string _selectedElementName = Session.SelectedElement != null && Session.SelectedElement.IsValid ? Session.SelectedElement.Name : "Neues Element";

        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */
        public bool IsServerOnline => UpdaterManager.IsServerAvailable;
        public string ServerStatusLabel => IsServerOnline ? "Server Online" : "Server nicht erreichbar";
        public string IsServerOnlineColorCode => IsServerOnline ? "#2cde00" : "#fc0303";

        public string LicensePath => "https://github.com/nbungu/BauphysikToolWPF?tab=GPL-3.0-1-ov-file#readme";

        public string ProgramVersion => $"Aktuelle Version: {UpdaterManager.LocalUpdaterManagerFile.CurrentTag}";
        public string LatestProgramVersion => $"Neueste Version: {UpdaterManager.LocalUpdaterManagerFile.LatestTag}";
        public string Website => $"https://bauphysik-tool.de";
    }
}
