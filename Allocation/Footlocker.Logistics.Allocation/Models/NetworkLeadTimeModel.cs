using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class NetworkLeadTimeModel
    {
        public string Message { get; set; }

        public List<NetworkLeadTime> NetworkLeadTimes { get; set; }

        public List<Instance> AvailableInstances { get; set; }

        [Display(Name="Instance")]
        public int InstanceID { get; set; }
        
        [Required(ErrorMessage="* Required")]
        [Display(Name="LeadTime")]
        public string NewLeadTime { get; set; }

    }
}