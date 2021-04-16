using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("Renumber")]
    public class Renumber
    {
        [Key]
        [Column(Order = 0)]
        public int InstanceID { get; set; }

        [Key]
        [Column(Order = 1)]
        public long ID { get; set; }
        public string OldSKU { get; set; }
        public string NewSKU { get; set; }

        [Column("RenumberDate")]
        public string RenumberDateString { get; set; }

        [Column("OldID")]
        public long OldItemID { get; set; }

        [Column("NewID")]
        public long NewItemID { get; set; }
    }
}
