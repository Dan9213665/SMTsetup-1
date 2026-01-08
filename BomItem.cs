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

        public int? Required { get; set; }
        public int[]? FoundQtys { get; set; }

        public int TotalInKit => FoundQtys?.Sum() ?? 0;
        // 👇 this feeds the Calc column
        public string Calc =>
            FoundQtys == null || FoundQtys.Length == 0
                ? string.Empty
                : string.Join(" ", FoundQtys);
    }
}
