using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class EcomCustomerFulfillmentXrefModel
    {
        public EcomCustomerFulfillmentXref DataRec { get; set; }

        [Display(Name = "Fulfillment Center")]
        public List<SelectListItem> FulfillmentCenters { get; set; }

        [Display(Name = "ECOM Store")]
        public List<SelectListItem> ECOMStores { get; set; }

        public string SelectedFulfillmentCenter { get; set; }
        public string SelectedECOMStore { get; set; }
    }
}