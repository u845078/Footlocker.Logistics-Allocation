using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using Footlocker.Logistics.Allocation.Validation;

namespace Footlocker.Logistics.Allocation.Models
{
    public class EcomCustomerFulfillmentXrefModel
    {
        public EcomCustFulfillmentXref DataRec { get; set; }

        [Display(Name = "Fulfillment Center")]
        public SelectList FulfillmentCenters { get; set; }

        [Display(Name = "ECOM Store")]
        public List<SelectListItem> ECOMStores { get; set; }

        [Display(Name = "Country")]
        public List<SelectListItem> CountryCodes { get; set; }

        [Display(Name = "State/Province")]
        public List<SelectListItem> StateCodes { get; set; }

        public int ID { get; set; }
    }
}