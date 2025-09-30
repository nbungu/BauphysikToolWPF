using BauphysikToolWPF.Models.Application;
using BT.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BauphysikToolWPF.Services.Application
{
    public class UpdaterManager
    {
        // HttpClient should be a singleton to avoid socket exhaustion
        private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        // testing: curl -v "http://192.168.0.160:1337/api/downloads?sort=publishedAt:desc&fields%5B0%5D=semanticVersion&fields%5B1%5D=versionTag"

        internal static readonly UpdaterJsonData ProgramVersionState = GetUpdaterJsonData();
        internal static bool NewVersionAvailable => CompareSemanticVersions(ProgramVersionState.Current, ProgramVersionState.Latest) < 0;
        internal static bool IsServerAvailable => GetServerStatus();
        
        public static void CheckForUpdates()
        {
            var serverData = FetchVersionFromServer(PathService.ServerUpdaterQuery).Result;

            ProgramVersionState.Latest = serverData.Latest;
            ProgramVersionState.LatestTag = serverData.LatestTag;
            ProgramVersionState.LastUpdateCheck = TimeStamp.GetCurrentUnixTimestamp();

            UpdateUpdaterJson(ProgramVersionState);
        }

        public static void UpdateUpdaterJson(UpdaterJsonData content)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                };
                string jsonString = JsonSerializer.Serialize(content, options);
                File.WriteAllText(PathService.UserUpdaterFilePath, jsonString);
                Logger.LogInfo($"Successfully saved content to local updater.json");
            }
            catch (Exception e)
            {
                Logger.LogError($"Error saving to local updater.json: {e.Message}");
            }
        }

        #region private methods

        public static void SetupFile(bool forceReplace = false)
        {
            // file under user-specific AppData folder
            string userUpdaterJsonFilePath = PathService.UserUpdaterFilePath;
            // file under installation directory folder
            string sourceUpdaterJsonFilePath = PathService.BuildDirUpdaterFilePath;

            try
            {
                // Ensure the directory exists
                if (!Directory.Exists(PathService.UserProgramDataPath))
                {
                    Directory.CreateDirectory(PathService.UserProgramDataPath);
                }

                // Example: Copy the file from the output folder to ProgramData if it doesn't already exist
                if (!File.Exists(userUpdaterJsonFilePath))
                {
                    Logger.LogInfo($"Copying updater.json from: {sourceUpdaterJsonFilePath} to {userUpdaterJsonFilePath}");
                    File.Copy(sourceUpdaterJsonFilePath, userUpdaterJsonFilePath);
                }
                if (forceReplace)
                {
                    Logger.LogInfo($"Force replacing updater.json from: {sourceUpdaterJsonFilePath} to {userUpdaterJsonFilePath}");
                    File.Delete(userUpdaterJsonFilePath);
                    File.Copy(sourceUpdaterJsonFilePath, userUpdaterJsonFilePath);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Could not get installed updater.json: {e.Message}");
            }
        }

        private static bool GetServerStatus()
        {
            try
            {
                return Task.Run(async () =>
                {
                    using var response = await Client.GetAsync(PathService.ServerUpdaterQuery);
                    return response.IsSuccessStatusCode;
                }).GetAwaiter().GetResult();
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        private static async Task<UpdaterJsonData> FetchVersionFromServer(string serverAddress)
        {
            UpdaterJsonData updaterManager = new UpdaterJsonData();
            try
            {
                updaterManager = await FetchAndDeserializeUpdaterAsync(serverAddress);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error fetching updater file from server: {serverAddress}, {ex.Message}");
            }
            return updaterManager;
        }

        private static UpdaterJsonData GetUpdaterJsonData()
        {
            if (!File.Exists(PathService.UserUpdaterFilePath))
            {
                Logger.LogWarning($"Local version file not found");
                return new UpdaterJsonData();
            }
            try
            {
                string jsonString = File.ReadAllText(PathService.UserUpdaterFilePath);

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                };

                var updater = JsonSerializer.Deserialize<UpdaterJsonData>(jsonString, options);

                if (updater != null)
                {
                    Logger.LogInfo($"Successfully fetched local updater.json");
                    return updater;
                }

                Logger.LogWarning($"Failed to deserialize local updater.json");
            }
            catch (IOException e)
            {
                Logger.LogError($"I/O error while reading local updater.json: {e.Message}");
            }
            catch (JsonException e)
            {
                Logger.LogError($"JSON deserialization error for local updater.json: {e.Message}");
            }
            catch (Exception e)
            {
                Logger.LogError($"Unexpected error while fetching local updater.json: {e.Message}");
            }

            return new UpdaterJsonData();
        }

        private static async Task<UpdaterJsonData> FetchAndDeserializeUpdaterAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                var response = client.GetAsync(url).Result;  // Blocking call!

                if (response.IsSuccessStatusCode)
                {
                    Logger.LogInfo($"Received Status Code 200 OK from GET request: {url}");

                    // Parse the response body. Blocking!
                    var jsonString = response.Content.ReadAsStringAsync().Result;
                    var options = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.Preserve,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                    };

                    ProgramVersionItem jsonResponse = JsonSerializer.Deserialize<ProgramVersionItem>(jsonString, options);

                    if (jsonResponse != null && jsonResponse.Data.Length > 0)
                    {
                        var data = jsonResponse.Data[0];
                        var updater = new UpdaterJsonData
                        {
                            Latest = data.SemanticVersion,
                            LatestTag = data.VersionTag
                        };

                        Logger.LogInfo($"Successfully deserialized updater file from server");
                        return updater;
                    }
                    Logger.LogWarning($"Could not deserialize the fetched updater file");
                    return new UpdaterJsonData();
                }
                var result = $"{(int)response.StatusCode} ({response.ReasonPhrase})";
                Logger.LogWarning($"Received invalid Status Code from GET request: {result}");
                return new UpdaterJsonData();
            }
        }

        private static int CompareSemanticVersions(string version1, string version2)
        {
            if (version1 == "" || version2 == "") return 0;

            // Split the versions into major, minor, and patch components
            var v1Components = version1.Split('.');
            var v2Components = version2.Split('.');

            for (int i = 0; i < 3; i++) // Loop through major, minor, and patch
            {
                // Parse each component to an integer
                int v1Part = i < v1Components.Length ? int.Parse(v1Components[i]) : 0;
                int v2Part = i < v2Components.Length ? int.Parse(v2Components[i]) : 0;

                // Compare each component
                if (v1Part > v2Part) return 1; // result > 0: version1 is newer
                if (v1Part < v2Part) return -1; // result < 0: version2 is newer
            }

            // Versions are equal
            return 0;
        }

        #endregion
    }
}
