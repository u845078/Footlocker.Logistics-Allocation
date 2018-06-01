using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class NetworkZoneStore
    {
        [Key]
        [Column(Order = 0)]
        public string Division { set; get; }
        [Key]
        [Column(Order = 1)]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Store number must be in the format #####")]
        public string Store { set; get; }
        public int ZoneID { set; get; }
    }
}
