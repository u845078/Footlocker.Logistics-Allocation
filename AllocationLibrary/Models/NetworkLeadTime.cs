using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class NetworkLeadTime
    {
        public int ID { get; set; }
        [Required]
        [Display(Name="LeadTime")]
        public String Name { get; set; }
        public String CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int InstanceID { get; set; }
   }
}
