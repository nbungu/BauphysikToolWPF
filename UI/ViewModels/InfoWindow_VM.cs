﻿using BauphysikToolWPF.Services;
using BauphysikToolWPF.SessionData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System;
using BT.Logging;
using System.Windows;

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
                    window.Close();
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
                    window.Close();
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

        /*
         * MVVM Properties: Observable, if user triggers the change of these properties via frontend
         * 
         * Initialized and Assigned with Default Values
         */

        [ObservableProperty]
        private string _selectedElementName = UserSaved.SelectedElement.IsValid ? UserSaved.SelectedElement.Name : "Neues Element";

        /*
         * MVVM Capsulated Properties or Triggered by other Properties
         */
        public string ServerStatus => Updater.CheckServerStatus(Updater.ServerUpdaterQuery) ? "Server Online" : "Server nicht erreichbar";

        public string LicensePath => "https://github.com/nbungu/BauphysikToolWPF?tab=GPL-3.0-1-ov-file#readme";

        public string ProgramVersion => $"Aktuelle Version: {Updater.LocalUpdaterFile.CurrentTag}";
        public string LatestProgramVersion => $"Neueste Version: {Updater.LocalUpdaterFile.LatestTag}";
        public string Website => $"https://bauphysik-tool.de";
    }
}