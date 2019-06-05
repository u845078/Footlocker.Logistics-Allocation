using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("EcomCustomerFulfillmentXRef")]
    public class EcomCustomerFulfillmentXref
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long FulfillmentXrefID { get; set; }

        public string City { get; set; }
        public string PostalCode { get; set; }

        public string State { get; set; }
        public string Country { get; set; }
        
        public int FulfillmentCenterID { get; set; }

        [ForeignKey("FulfillmentCenterID")]
        public virtual DistributionCenter FulfillmentCenter { get; set; }

        public DateTime EffectiveFromDate { get; set; }
        public DateTime? EffectiveToDate { get; set; }


        [Display(Name = "Last Modified User")]
        public string LastModifiedUser { get; set; }
        [Display(Name = "Last Modified Date")]
        public DateTime LastModifiedDate { get; set; }
    }
}
