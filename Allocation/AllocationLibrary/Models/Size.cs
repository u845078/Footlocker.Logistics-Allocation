using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("Sizes")]
    public class SizeObj
    {
        [Key]
        [Column(Order = 0)]
        public int InstanceID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Sku { get; set; }

        [Key]
        [Column(Order = 2)]
        public string Size { get; set; }
    }
}
