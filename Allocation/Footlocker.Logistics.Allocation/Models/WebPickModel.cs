using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class WebPickModel
    {
        public RDQ RDQ { get; set; }
        public List<Division> Divisions { get; set; }
        public List<DistributionCenter> DCs { get; set; }
        public string Message { get; set; }
        //[Display(Name="Allow pick even though inventory will go negative")]
        //public Boolean AllowPickAnyway { get; set; }
        public List<SelectListItem> PickOptions { get; set; }
    }
}