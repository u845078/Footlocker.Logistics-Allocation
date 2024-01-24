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
            InstanceID = 0;
            Division = string.Empty;
            Store = string.Empty;
            ItemID = 0;
        }
    }
}
