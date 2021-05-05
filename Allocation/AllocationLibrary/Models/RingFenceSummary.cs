using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RingFenceSummary
    {
        public long RingFenceID { get; set; }

        public string Division { get; set; }

        public string Store { get; set; }

        public RingFenceStatusCodes RingFenceStatus { get; set; }

        public bool CanPick { get; set; }

        public int PickQuantity { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreateDate { get; set; }

        public long ItemID { get; set; }

        public string Sku;

        public string Size;

        public string DC;

        public string PO;

        public int Qty;

        public string Description;
    }
}
