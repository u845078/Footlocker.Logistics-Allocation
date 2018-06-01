using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RouteDetail
    {
        [Key]
        [Column(Order = 0)]
        public int InstanceID { get; set; }
        [Key]
        [Column(Order = 1)]
        public int RouteID { get; set; }
        [Key]
        [Column(Order = 2)]
        public int DCID { get; set; }
        [Key]
        [Column(Order = 3)]
        public int ZoneID { get; set; }

        public Int32? Days { get; set; }
    }
}
