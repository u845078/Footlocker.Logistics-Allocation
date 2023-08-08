using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class NetworkZone
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        public int LeadTimeID { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
