﻿using BT.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BauphysikToolWPF.Services
{
    public class Updater : IEquatable<Updater>
    {
        public static string InstalledUpdaterFilePath = GetInstalledUpdaterFilePath();
        public static string ServerUpdaterQuery = "http://192.168.0.160:1337/api/downloads?sort=publishedAt:desc&fields[0]=semanticVersion&fields[1]=versionTag";
        // testing: curl -v "http://192.168.0.160:1337/api/downloads?sort=publishedAt:desc&fields%5B0%5D=semanticVersion&fields%5B1%5D=versionTag"

        public static Updater LocalUpdaterFile = FetchLocalVersion(InstalledUpdaterFilePath);

        public string Latest { get; set; } = string.Empty;
        public string LatestTag { get; set; } = string.Empty;
        public string Current { get; set; } = string.Empty;
        public string CurrentTag { get; set; } = string.Empty;
        public long LastUpdateCheck { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        public long LastNotification { get; set; } = TimeStamp.GetCurrentUnixTimestamp();

        public static void CheckForUpdates()
        {
            var updaterLocal = LocalUpdaterFile;
            var updaterServer = FetchVersionFromServer(ServerUpdaterQuery).Result;

            // If server version is newer
            if (CompareSemanticVersions(updaterLocal.Current, updaterServer.Latest) < 0)
            {
                Logger.LogInfo($"Found new Version! Writing new Version to local updater file");
                updaterLocal.Latest = updaterServer.Latest;
                updaterLocal.LatestTag = updaterServer.LatestTag;
                updaterLocal.LastUpdateCheck = TimeStamp.GetCurrentUnixTimestamp();
                WriteToLocalUpdaterFile(updaterLocal);
            }
        }
        public static async Task<Updater> FetchVersionFromServer(string serverAddress)
        {
            Updater updater = new Updater();
            try
            {
                updater = await FetchAndDeserializeUpdaterAsync(serverAddress);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error fetching updater file from server: {serverAddress}, {ex.Message}");
            }
            return updater;
        }

        public static Updater FetchLocalVersion(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };
            Updater updater = JsonSerializer.Deserialize<Updater>(jsonString, options);
            if (updater != null)
            {
                Logger.LogInfo($"Successfully fetched local version file: {filePath}");
                return updater;
            }
            Logger.LogWarning($"Could not fetch local version file: {filePath}");
            return new Updater();
        }

        private static async Task<Updater> FetchAndDeserializeUpdaterAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10); // Set a 5-second timeout
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                Logger.LogInfo($"Got response from URL {url}: {response}");

                string jsonString = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                };

                JsonResponse jsonResponse = JsonSerializer.Deserialize<JsonResponse>(jsonString, options);

                if (jsonResponse != null && jsonResponse.Data.Length > 0)
                {
                    var attributes = jsonResponse.Data[0].Attributes;
                    var updater = new Updater
                    {
                        Latest = attributes.SemanticVersion,
                        LatestTag = attributes.VersionTag
                    };

                    Logger.LogInfo($"Successfully deserialized updater file from server: {url}");
                    return updater;
                }

                Logger.LogError($"Could not deserialize the fetched updater file: {url}");
                return new Updater();
            }
        }

        public static void WriteToLocalUpdaterFile(Updater localUpdaterFile, string filePath = "")
        {
            try
            {
                if (filePath == "") filePath = GetInstalledUpdaterFilePath();
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                };
                string jsonString = JsonSerializer.Serialize(localUpdaterFile, options);
                File.WriteAllText(filePath, jsonString);
                Logger.LogInfo($"Successfully saved local updater to file: {filePath}");
            }
            catch (Exception e)
            {
                Logger.LogError($"Error saving local updater file: {filePath}, {e.Message}");
            }
        }

        public static int CompareSemanticVersions(string version1, string version2)
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

        private static string GetInstalledUpdaterFilePath(bool forceReplace = false)
        {
            try
            {
                // User-specific AppData folder
                string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); //%appdata%/BauphysikTool
                string appFolder = Path.Combine(programDataPath, "BauphysikTool");
                string updaterFilePath = Path.Combine(appFolder, "updater.json");

                // Ensure the directory exists
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                // Example: Copy the file from the output folder to ProgramData if it doesn't already exist
                string sourceUpdaterFile = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Services\\updater.json")); ;

                if (!File.Exists(updaterFilePath))
                {
                    File.Copy(sourceUpdaterFile, updaterFilePath);
                }
                if (forceReplace)
                {
                    File.Delete(updaterFilePath);
                    File.Copy(sourceUpdaterFile, updaterFilePath);
                }
                return updaterFilePath;
            }
            catch (Exception e)
            {
                Logger.LogError($"Could not get installed updater file: {e.Message}");
                return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Services\\updater.json")); ;
            }
        }

        public bool Equals(Updater? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Latest == other.Latest && Current == other.Current;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Updater)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Latest, Current);
        }
    }
    internal class JsonResponse
    {
        public DataItem[] Data { get; set; }
    }

    internal class DataItem
    {
        public int Id { get; set; }
        public Attributes Attributes { get; set; }
    }

    internal class Attributes
    {
        [JsonPropertyName("semanticVersion")]
        public string SemanticVersion { get; set; }

        [JsonPropertyName("versionTag")]
        public string VersionTag { get; set; }
    }
}