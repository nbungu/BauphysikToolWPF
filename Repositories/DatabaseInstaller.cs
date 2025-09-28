using BauphysikToolWPF.Services.Application;
using BT.Logging;
using System;
using System.IO;

namespace BauphysikToolWPF.Repositories
{
    internal static class DatabaseInstaller
    {
        public static string RootProjectDatabaseFile = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\Repositories\\InitialDB.db"));
        public static string BuildDirectoryDatabaseFile = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Repositories\\InitialDB.db"));

        /// <summary>
        /// Moves and Installs the Initial Database from Installation/Output directory to C:\ProgramData or C:\Users{current_user}\AppData.
        /// All Application Data should be stored where read/write access is not limited.
        /// </summary>
        /// <returns>Connection String to the new Database Location where the App has full read/write permissions</returns>
        public static string GetInstalledDatabase(bool forceReplace = false, bool copyFromBuildPath = true)
        {
            try
            {
                // Set the path to C:\ProgramData\BauphysikTool
                //string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                // Or for user-specific AppData folder

                string appFolder = PathService.LocalAppDataPath;
                string databaseFilePath = Path.Combine(appFolder, "BauDB.db");

                // Ensure the directory exists
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                // Example: Copy the database from the output folder to ProgramData if it doesn't already exist
                string sourceDatabasePath = copyFromBuildPath ? BuildDirectoryDatabaseFile : RootProjectDatabaseFile; //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BauDB.db");
                
                if (!File.Exists(databaseFilePath))
                {
                    Logger.LogInfo($"Copying Database from: {sourceDatabasePath} to {databaseFilePath}");
                    File.Copy(sourceDatabasePath, databaseFilePath);
                }
                if (forceReplace)
                {
                    Logger.LogInfo($"Force replacing Database from: {sourceDatabasePath} to {databaseFilePath}");
                    File.Delete(databaseFilePath);
                    File.Copy(sourceDatabasePath, databaseFilePath);
                }
                Logger.LogInfo($"Connecting to Database: {databaseFilePath}");
                return databaseFilePath;
            }
            catch (Exception e)
            {
                Logger.LogError($"Could not get installed Database: {e.Message}");
                return BuildDirectoryDatabaseFile;
            }
        }

        /// <summary>
        /// Original InitialDB.db file located in the root project folder /Repositories
        /// </summary>
        /// <returns>Connection string linked to the InitialDB.db file which is at the 'BauphysikToolWPF/Repositories' directory</returns>
        public static string GetInitialDatabase()
        {
            Logger.LogInfo($"Connecting to Database: {RootProjectDatabaseFile}");
            return RootProjectDatabaseFile;
        }

        /// <summary>
        /// Copy of original InitialDB.db which is located in the Build-Directory (bin/Debug/... build path).
        /// Gets replaced every build: "Copy, if newer".
        /// </summary>
        /// <returns>Connection string linked to the InitialDB.db file which is at the 'BauphysikToolWPF/bin/Debug/net8.0-windows10.0.22621.0/Repositories' directory</returns>
        public static string GetInitialDatabaseFromBuildPath()
        {
            Logger.LogInfo($"Connecting to Database: {RootProjectDatabaseFile}");
            return BuildDirectoryDatabaseFile;
        }


    }
}
