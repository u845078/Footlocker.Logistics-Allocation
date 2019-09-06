using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("Regions")]
    public class Region
    {
        [Key, Column(Order = 0)]
        public int InstanceID { get; set; }

        [Key, Column("Div", Order = 1)]
        public string Division { get; set; }

        [Key, Column("Region", Order = 2)]
        public string RegionCode { get; set; }

        public string Description { get; set; }
    }
}
