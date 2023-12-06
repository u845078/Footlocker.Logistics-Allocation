using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RingFenceSizeSummary
    {
        public string Size { get; set; }
        [Display(Name="Total Units")]
        public int TotalQty { get; set; }
        [Display(Name = "Future Units")]
        public int FutureQty { get; set; }
        [Display(Name = "Caselot Units")]
        public int CaselotQty { get; set; }
        [Display(Name = "Bin Units")]
        public int BinQty { get; set; }
    }
}
