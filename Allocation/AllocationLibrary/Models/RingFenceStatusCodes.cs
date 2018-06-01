using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("RingFenceStatusCodes")]
    public class RingFenceStatusCodes
    {
        [Key]
        public string ringFenceStatusCode { get; set; }

        public string ringFenceStatusDesc { get; set; }

        //public virtual List<RingFenceDetail> RingFenceDetails { get; set; }
    }
}
