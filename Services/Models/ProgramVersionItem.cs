using System;
using System.Text.Json.Serialization;

namespace BauphysikToolWPF.Services.Models
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

    #endregion
}
