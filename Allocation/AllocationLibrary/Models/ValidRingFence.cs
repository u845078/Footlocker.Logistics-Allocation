using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("vValidRingFences")]
    public class ValidRingFence
    {
        [Key]
        [Column("ID", Order = 0)]      
        public long RingFenceID { get; set; }
        
        [Key]
        [Column(Order = 1)]
        public string Division { get; set; }
        
        [Key]
        [Column(Order = 2)]
        public string Store { get; set; }
        
        [Key]
        [Column(Order = 3)]
        public string SKU { get; set; }
        public long ItemID { get; set; }

        [Key]
        [Column(Order = 4)]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Column("Type")]
        [ForeignKey("RingFenceType")]
        public int RingFenceTypeCode { get; set; }

        public virtual RingFenceType RingFenceType { get; set; }

        [Key]
        [Column(Order = 5)]
        public string Size { get; set; }

        [Key]
        [Column(Order = 6)]
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
