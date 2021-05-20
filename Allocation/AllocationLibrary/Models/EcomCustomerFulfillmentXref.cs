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
        
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Display(Name = "State/Province")]
        public string StateCode { get; set; }

        public virtual State State { get; set; }

        [ForeignKey("Country")]
        public string CountryCode { get; set; }


        public virtual Country Country { get; set; }

        [ForeignKey("FulfillmentCenter")]
        public int FulfillmentCenterID { get; set; }

        public virtual DistributionCenter FulfillmentCenter { get; set; }

        [ForeignKey("EcommWarehouse"), Column(Order = 0)]
        public string Division { get; set; }

        [ForeignKey("EcommWarehouse"), Column(Order = 1)]
        public string Store { get; set; }

        [NotMapped]
        public string EcomStore
        {
            get
            {
                if (string.IsNullOrEmpty(Division) || string.IsNullOrEmpty(Store))
                {
                    return "";
                }
                else
                    return string.Format("{0}-{1}", Division, Store);
            }
        }

        public virtual EcommWarehouse EcommWarehouse { get; set; }

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
