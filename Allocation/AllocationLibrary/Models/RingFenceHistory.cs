using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

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
    }
}
