using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("RingFenceHistory")]
    public class RingFenceHistory
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Int64 ID { get; set; }

        public Int64 RingFenceID { get; set; }
        public string Division { get; set; }
        public string Store { get; set; }
        public string Sku { get; set; }
        //public Int64 ItemID { get; set; }
        [Display(Name = "Caselot / Size")]
        public string Size { get; set; }
        public int DCID { get; set; }
        public string PO { get; set; }
        public int Qty { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string Action { get; set; }
        [NotMapped]
        public string Warehouse { get; set; }

        public RingFenceHistory()
            : base()
        {
            this.RingFenceID = 0;
            this.Division = string.Empty;
            this.Store = string.Empty;
            this.Sku = string.Empty;
            this.Size = string.Empty;
            this.DCID = 0;
            this.PO = string.Empty;
            this.Qty = 0;
            this.StartDate = DateTime.MinValue;
            this.EndDate = DateTime.MinValue;
            this.CreateDate = DateTime.MinValue;
            this.CreatedBy = string.Empty;
            this.Action = string.Empty;
        }

        public RingFenceHistory(long ringFenceID, string division, string store, string sku, int qty, DateTime? startDate, DateTime? endDate, DateTime createDate, string createdBy, string action)
            : this()
        {
            this.RingFenceID = ringFenceID;
            this.Division = division;
            this.Store = store;
            this.Sku = sku;
            this.Qty = qty;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.CreateDate = createDate;
            this.CreatedBy = createdBy;
            this.Action = action;
        }

        public RingFenceHistory(long ringFenceID, string division, string store, string sku, string size, int dcid, string po, int qty, DateTime? startDate, DateTime? endDate, DateTime createDate, string createdBy, string action)
            : this(ringFenceID, division, store, sku, qty, startDate, endDate, createDate, createdBy, action)
        {
            this.Size = size;
            this.DCID = dcid;
            this.PO = po;
        }
    }
}
