using System;

namespace Footlocker.Logistics.Allocation.Models
{
    public class InventorySummary
    {
        public long ItemID { get; set; }
        public string Sku { get; set; }
        public string Size { get; set; }
        public int Qty { get; set; }
    }
}
