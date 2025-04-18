using BauphysikToolWPF.Services.Application;
using System.Text.Json.Serialization;

namespace BauphysikToolWPF.Models.Application
{
    public class RecentProjectItem
    {
        [JsonPropertyName("FileName")]
        public string FileName { get; set; } = string.Empty;
        [JsonPropertyName("FilePath")]
        public string FilePath { get; set; } = string.Empty;
        [JsonPropertyName("LastOpened")]
        public long LastOpened { get; set; }
        [JsonIgnore]
        public string LastOpenedString => TimeStamp.ConvertToNormalTimeDetailed(LastOpened);
        [JsonIgnore]
        public bool IsValid => FileName != "" && FilePath != "" && LastOpened > 0;
        [JsonIgnore]
        public static RecentProjectItem Empty => new RecentProjectItem() { FileName = "keine zuletzt geöffneten Projekte" }; // Optional static default (for easy reference)
    }
}
