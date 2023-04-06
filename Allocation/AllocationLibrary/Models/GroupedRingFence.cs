using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("vGroupedRingFences")]
    public class GroupedRingFence
    {
        [Key]
        public long ID { get; set; }
        public string Division { get; set; }
        public string Store { get; set; }
        public string SKU { get; set; }
        
        [ForeignKey("ItemMaster")]
        public long ItemID { get; set; }
        public virtual ItemMaster ItemMaster { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Column("Type")]
        [ForeignKey("RingFenceType")]
        public int RingFenceTypeCode { get; set; }

        public virtual RingFenceType RingFenceType { get; set; }

        [ForeignKey("DistributionCenter")] 
        public int DCID { get; set; }

        public virtual DistributionCenter DistributionCenter { get; set; }

        [Column("TotalQty")]
        public int TotalQuantity { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string Comments { get; set; }

        public string LastModifiedUser { get; set; }
        public DateTime LastModifiedDate { get; set; }

        [NotMapped]
        public bool CanPick
        {
            get
            {
                bool result;

                if (!EndDate.HasValue)
                    result = true;
                else
                    result = EndDate.Value >= DateTime.Now;

                return result;
            }
        }
    }
}
