using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("vMinihubStores")]
    public class MinihubStore
    {
        [Key]
        [Column(Order = 1)]
        public string Division { get; set; }
        [Key]
        [Column(Order = 2)]
        public string Store { get; set; }

        public int MinihubID { get; set; }

        public string MinihubCode { get; set; }
    }
}
