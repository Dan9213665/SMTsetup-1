using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTsetup
{
    public class BomItem
    {
        public string? SetNo { get; set; }
        public string? CompName { get; set; }
        public string? Comments { get; set; }

        public string? FdrType { get; set; }
        public string? PitchIndex { get; set;}

        public bool FoundTheItem { get; set; }
    }
}
