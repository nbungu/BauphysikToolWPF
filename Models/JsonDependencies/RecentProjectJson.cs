using BauphysikToolWPF.Services;
using SQLite;
using System.Text.Json.Serialization;

namespace BauphysikToolWPF.Models.JsonDependencies
{
    public class RecentProjectJson
    {
        [JsonPropertyName("FileName")]
        public string FileName { get; set; } = string.Empty;
        [JsonPropertyName("FilePath")]
        public string FilePath { get; set; } = string.Empty;
        [JsonPropertyName("LastOpened")]
        public long LastOpened { get; set; }
        [Ignore, JsonIgnore]
        public string LastOpenedString => TimeStamp.ConvertToNormalTimeDetailed(LastOpened);
    }
}
