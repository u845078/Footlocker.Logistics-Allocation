using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class MandatoryCrossdock
    {
        [Key]
        [Column(Order = 0)]
        public int InstanceID { get; set; }

        [Key]
        [Column(Order = 1)]
        public long ItemID { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreateDate { get; set; }

        public virtual ItemMaster ItemMaster { get; set; }
    }
}
