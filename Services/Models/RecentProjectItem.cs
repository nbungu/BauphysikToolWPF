using System.Text.Json.Serialization;
using SQLite;

namespace BauphysikToolWPF.Services.Models
{
    public class RecentProjectItem
    {
        [JsonPropertyName("FileName")]
        public string FileName { get; set; } = string.Empty;
        [JsonPropertyName("FilePath")]
        public string FilePath { get; set; } = string.Empty;
        [JsonPropertyName("LastOpened")]
        public long LastOpened { get; set; }
        [Ignore, JsonIgnore]
        public string LastOpenedString => TimeStamp.ConvertToNormalTimeDetailed(LastOpened);
        [Ignore, JsonIgnore]
        public bool IsValid => FileName != "" && FilePath != "" && LastOpened > 0;
    }
}
