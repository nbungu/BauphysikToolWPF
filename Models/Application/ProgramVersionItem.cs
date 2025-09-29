using BauphysikToolWPF.Services.Application;
using System;
using System.Text.Json.Serialization;

namespace BauphysikToolWPF.Models.Application
{
    #region Strapi API-Response Structure

    internal class ProgramVersionItem
    {
        [JsonPropertyName("data")]
        public DataItem[] Data { get; set; } = Array.Empty<DataItem>();
    }

    internal class DataItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("semanticVersion")]
        public string SemanticVersion { get; set; } = string.Empty;

        [JsonPropertyName("versionTag")]
        public string VersionTag { get; set; } = string.Empty;

        [JsonPropertyName("documentId")]
        public string DocumentId { get; set; } = string.Empty;
    }

    public class UpdaterJsonData
    {
        public string Latest { get; set; } = string.Empty;
        public string LatestTag { get; set; } = string.Empty;
        public string Current { get; set; } = string.Empty;
        public string CurrentTag { get; set; } = string.Empty;
        public long LastUpdateCheck { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        public long LastNotification { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
    }

    #endregion
}
