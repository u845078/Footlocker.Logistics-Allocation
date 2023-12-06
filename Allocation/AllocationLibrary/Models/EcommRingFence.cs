using System;

namespace Footlocker.Logistics.Allocation.Models
{
    public class EcommRingFence
    {
        public string Sku { get; set; }
        public string Size { get; set; }
        public string PO { get; set; }
        public int Qty { get; set; }
        public string EndDate { get; set; }
        public string Comments { get; set; }

        public EcommRingFence()
            : base()
        {
            this.Sku = string.Empty;
            this.Size = string.Empty;
            this.PO = string.Empty;
            this.Qty = 0;
            this.EndDate = string.Empty;
            this.Comments = string.Empty;
        }
    }
}
