using BauphysikToolWPF.Services.Application;
using BT.Logging;
using System;
using System.IO;

namespace BauphysikToolWPF.Repositories
{
    internal static class DatabaseInstaller
    {
        private static readonly string RootProjectDatabaseFile = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\Repositories\\InitialDB.db"));

        /// <summary>
        /// Moves and Installs the Initial Database from Installation/Output directory to C:\ProgramData or C:\Users\{current_user}\AppData.
        /// All Application Data should be stored where read/write access is not limited.
        /// </summary>
        /// <returns>Connection String to the new Database Location where the App has full read/write permissions</returns>
        public static string SetupFile(bool forceReplace = false, bool copyFromBuildPath = true)
        {
            try
            {
                // Set the path to CommonApplicationDataPath
                // Or for user-specific AppData folder

                string userDatabaseFilePath = PathService.UserDatabaseFilePath;

                // Example: Copy the database from the output folder to ProgramData if it doesn't already exist
                string sourceDatabasePath = copyFromBuildPath ? PathService.BuildDirDatabaseFilePath : RootProjectDatabaseFile;

                // Ensure the directory exists
                if (!Directory.Exists(PathService.UserProgramDataPath))
                {
                    Directory.CreateDirectory(PathService.UserProgramDataPath);
                }
                if (!File.Exists(userDatabaseFilePath))
                {
                    Logger.LogInfo($"Copying Database from: {sourceDatabasePath} to {userDatabaseFilePath}");
                    File.Copy(sourceDatabasePath, userDatabaseFilePath);
                }
                if (forceReplace)
                {
                    Logger.LogInfo($"Force replacing Database from: {sourceDatabasePath} to {userDatabaseFilePath}");
                    File.Delete(userDatabaseFilePath);
                    File.Copy(sourceDatabasePath, userDatabaseFilePath);
                }
                Logger.LogInfo($"Connecting to Database: {userDatabaseFilePath}");
                return userDatabaseFilePath;
            }
            catch (Exception e)
            {
                Logger.LogError($"Could not get installed Database: {e.Message}");
                return PathService.BuildDirDatabaseFilePath;
            }
        }

        /// <summary>
        /// Original InitialDB.db file located in the root visual studio project folder /Repositories
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
        public static string GetInitialDatabaseFromBuildDir()
        {
            Logger.LogInfo($"Connecting to Database: {PathService.BuildDirDatabaseFilePath}");
            return PathService.BuildDirDatabaseFilePath;
        }


    }
}
