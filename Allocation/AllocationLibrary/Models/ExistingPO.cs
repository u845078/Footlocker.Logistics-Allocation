using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace Footlocker.Logistics.Allocation.Models
{
    public class ExistingPO 
    {
        public string Division { get; set; }
        public string PO { get; set; }
        public string Sku { get; set; }

        public DateTime ExpectedDeliveryDate { get; set; }
        public DateTime? OverrideDate { get; set; }

        public string Description { get; set; }
        public decimal Retail { get; set; }
        public int Units { get; set; }
        public Boolean DirectToStore { get; set; }
        public string WarehouseNumber { get; set; }

        public string POStatusCode { get; set; }
        public string vendorNumber { get; set; }
        public DateTime createDate { get; set; }
        public int receivedQuantity { get; set; }
    }
}
