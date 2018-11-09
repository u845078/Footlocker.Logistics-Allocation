using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class Instance
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }
    }
}
