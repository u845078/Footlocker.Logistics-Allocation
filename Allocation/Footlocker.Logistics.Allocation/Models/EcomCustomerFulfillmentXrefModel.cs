using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Models
{
    public class EcomCustomerFulfillmentXrefModel
    {
        public EcomCustomerFulfillmentXref DataRec { get; set; }
        public List<SelectListItem> FulfillmentCenters { get; set; }

        public string SelectedInstanceID { get; set; }

        public List<SelectListItem> Instances { get; set; }
    }
}