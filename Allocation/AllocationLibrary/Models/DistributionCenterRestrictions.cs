using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("DistributionCenterRestrictions")]
    public class DistributionCenterRestrictions
    {
        [Key]
        [Column("DistributionCenterRestriction")]
        public int Code { get; set; }

        [Column("DistributionCenterRestrictionDesc")]
        public string Description { get; set; }
    }
}
