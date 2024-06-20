using System;
using System.IO;

namespace BauphysikToolWPF.Repository
{
    internal static class DatabaseInstaller
    {
        public static string InitialDatabasePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Repository\\InitialDB.db"));

        /// <summary>
        /// Moves and Installs the Initial Database from Installation/Output directory to C:\ProgramData or C:\Users{current_user}\AppData.
        /// All Application Data should be stored where read/write access is not limited.
        /// </summary>
        /// <returns>Connection String to the new Database Location where the App has full read/write permissions</returns>
        public static string Install(bool forceUpdate = false)
        {
            try
            {
                // Set the path to C:\ProgramData\BauphysikTool
                string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                // Or for user-specific AppData folder
                // string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(programDataPath, "BauphysikTool");
                string databaseFilePath = Path.Combine(appFolder, "BauDB.db");

                // Ensure the directory exists
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                // Example: Copy the database from the output folder to ProgramData if it doesn't already exist
                string sourceDatabasePath = InitialDatabasePath;//Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DemoDB.db");

                if (forceUpdate)
                {
                    if (!File.Exists(databaseFilePath))
                    {
                        File.Copy(sourceDatabasePath, databaseFilePath);
                    }
                    else
                    {
                        File.Delete(databaseFilePath);
                        File.Copy(sourceDatabasePath, databaseFilePath);
                    }
                }
                if (!File.Exists(databaseFilePath))
                {
                    File.Copy(sourceDatabasePath, databaseFilePath);
                }
                return databaseFilePath;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
