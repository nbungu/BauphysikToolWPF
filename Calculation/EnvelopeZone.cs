using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BauphysikToolWPF.Models.Domain;

namespace BauphysikToolWPF.Calculation
{
    public class EnvelopeZone
    {
        public string ZoneName => EnvelopeItem.FirstOrDefault()?.UsageZoneName ?? string.Empty;
        public List<EnvelopeItem> EnvelopeItem { get; set; } 

        public EnvelopeZone() { }
    }
}
