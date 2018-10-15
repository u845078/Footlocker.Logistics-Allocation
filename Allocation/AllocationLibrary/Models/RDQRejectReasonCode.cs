using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("RDQRejectReasonCodes")]
    public class RDQRejectReasonCode
    {
        [Key]
        [Column("RDQRejectReasonCode")]
        public int Code { get; set; }

        [Column("RDQRejectReasonDesc")]
        public string Description { get; set; }
    }
}
