using BauphysikToolWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.Services
{
    public class Updater
    {
        public string Latest { get; set; } = string.Empty;
        public string LatestTag { get; set; } = string.Empty;
        public string Current { get; set; } = string.Empty;
        public string CurrentTag { get; set; } = string.Empty;
        public long LastUpdateCheck { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
        public long LastNotification { get; set; } = TimeStamp.GetCurrentUnixTimestamp();
    }
}
