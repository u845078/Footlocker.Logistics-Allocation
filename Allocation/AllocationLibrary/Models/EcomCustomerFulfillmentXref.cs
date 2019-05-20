using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class EcomCustomerFulfillmentXref
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FulfillmentXrefID { get; set; }

        public string City { get; set; }
        public string PostalCode { get; set; }

        public string State { get; set; }
        public string Country { get; set; }

        [ForeignKey("DistributionCenter")]
        public Int32 FulfillmentCenterID { get; set; }

        public virtual DistributionCenter FulfillmentCenter { get; set; }

        public DateTime EffectiveFromDate { get; set; }
        public DateTime? EffectiveToDate { get; set; }


        [Display(Name = "Last Modified User")]
        public string LastModifiedUser { get; set; }
        [Display(Name = "Last Modified Date")]
        public DateTime LastModifiedDate { get; set; }
    }
}
