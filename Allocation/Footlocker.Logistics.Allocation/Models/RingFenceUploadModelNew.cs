using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RingFenceUploadModelNew
    {
        public string Division { get; set; }
        public string Store { get; set; }
        public string Sku { get; set; }
        public DateTime? EndDate { get; set; }
        public string PO { get; set; }
        public string DC { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public string Comments { get; set; }

        public RingFenceUploadModelNew()
            : base()
        {
            this.Division = string.Empty;
            this.Store = string.Empty;
            this.Sku = string.Empty;
            this.EndDate = null;
            this.PO = string.Empty;
            this.DC = string.Empty;
            this.Size = string.Empty;
            this.Quantity = 0;
            this.Comments = string.Empty;
        }

        public RingFenceUploadModelNew(string division, string store, string sku, DateTime endDate, string po, string dc, string size, int quantity, string comments)
            : this()
        {
            this.Division = division;
            this.Store = store;
            this.Sku = sku;
            this.EndDate = endDate;
            this.PO = po;
            this.DC = dc;
            this.Size = size;
            this.Quantity = quantity;
            this.Comments = comments;
        }
    }
}