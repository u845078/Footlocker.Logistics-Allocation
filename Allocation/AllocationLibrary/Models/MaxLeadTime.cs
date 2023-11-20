using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("vMaxLeadTimes")]
    public class MaxLeadTime
    {
        [Key]
        [Column(Order=0)]
        public string Division { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Store { get; set; }

        public int LeadTime { get; set; }
    }
}
