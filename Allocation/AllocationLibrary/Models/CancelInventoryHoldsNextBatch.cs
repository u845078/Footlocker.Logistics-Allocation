using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("vCancelInventoryHoldsNextBatch")]
    public class CancelInventoryHoldsNextBatch
    {
        [Key, Column("instanceid", Order = 0)]
        public int InstanceID { get; set; }
        [Key, Column("division", Order = 1)]
        public string Division { get; set; }
        [Key, Column("Store", Order = 2)]
        public string Store { get; set; }
        [Key, Column("id", Order = 3)]
        public long ItemID { get; set; }

        public CancelInventoryHoldsNextBatch()
            : base()
        {
            this.InstanceID = 0;
            this.Division = string.Empty;
            this.Store = string.Empty;
            this.ItemID = 0;
        }

        public CancelInventoryHoldsNextBatch(int instanceID, string division, string store, long itemID)
            : this()
        {
            this.InstanceID = instanceID;
            this.Division = division;
            this.Store = store;
            this.ItemID = itemID;
        }
    }
}
