using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("vValidRingFences")]
    public class ValidRingFence
    {
        [Key]
        [Column("ID")]
        public long RingFenceID { get; set; }
        public string Division { get; set; }
        public string Store { get; set; }
        public string SKU { get; set; }
        public long ItemID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Column("Type")]
        [ForeignKey("RingFenceType")]
        public int RingFenceTypeCode { get; set; }

        public virtual RingFenceType RingFenceType { get; set; }

        public string Size { get; set; }
        
        [ForeignKey("DistributionCenter")]
        public int DCID { get; set; }
                
        public virtual DistributionCenter DistributionCenter { get; set; }
        public string PO { get; set; }

        [Column("Qty")]
        public int Quantity { get; set; }

        [ForeignKey("RingFenceStatus")]
        public string RingFenceStatusCode { get; set; }

        public virtual RingFenceStatusCodes RingFenceStatus { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string Comments { get; set; }
    }
}
