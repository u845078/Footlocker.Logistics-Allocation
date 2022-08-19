using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RouteModel
    {
        [Display(Name = "Instance")]
        public int InstanceID { get; set; }
        public SelectList AvailableInstances { get; set; }
        public List<Route> Routes { get; set; }
    }
}