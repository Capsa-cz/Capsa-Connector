using System;
using System.Text.Json.Serialization;

namespace Capsa_Connector.Model
{
    public class DashboardElementStructure
    {
        public string? FileName { get; set; }
        public string? FileExtension { get; set; }
        public string? ImageToPreviewImage { get; set; }
        public DateTime? EditTime { get; set; }
        public string? FileURL { get; set; }

        [JsonIgnore]
        public ActiveFile? ActiveFile { get; set; }
    }
}
