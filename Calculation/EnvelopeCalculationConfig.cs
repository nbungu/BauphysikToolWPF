using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.Calculation
{
    public class EnvelopeCalculationConfig
    {
        public bool IsEFH { get; set; } = true; // true = EFH, false = MFH
        public bool WithAirLeakTest { get; set; }
    }
}
