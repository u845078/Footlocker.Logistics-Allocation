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
        public string PostalCode { get; set; }

        public string State { get; set; }
        public string Country { get; set; }
        
        public int FulfillmentCenterID { get; set; }

        [ForeignKey("FulfillmentCenterID")]
        public virtual DistributionCenter FulfillmentCenter { get; set; }

        [Column("Division")]
        public string FulfillmentDivision { get; set; }

        [Column("Store")]
        public string FulfillmentStore { get; set; }

        [Display(Name = "Effective From Date")]
        public DateTime EffectiveFromDate { get; set; }

        [Display(Name = "Effective To Date")]
        public DateTime? EffectiveToDate { get; set; }


        [Display(Name = "Last Modified User")]
        public string LastModifiedUser { get; set; }
        [Display(Name = "Last Modified Date")]
        public DateTime LastModifiedDate { get; set; }
    }
}
