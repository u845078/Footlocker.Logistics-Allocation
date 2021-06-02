using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Validation
{
    public class EcomCustFulfillmentXref : IValidatableObject
    {
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Display(Name = "State/Province")]
        public string StateCode { get; set; }
        
        [Required]
        [Display(Name = "Country")]
        public string CountryCode { get; set; }
        public int FulfillmentCenterID { get; set; }
        public string Division { get; set; }
        public string Store { get; set; }
        
        [Required]
        [Display(Name = "Effective From Date")]
        public DateTime EffectiveFromDate { get; set; }

        [Display(Name = "Effective To Date")]
        public DateTime? EffectiveToDate { get; set; }

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
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Division = "";
                    Store = "";
                }
                else
                {
                    Division = value.Substring(0, 2);
                    Store = value.Substring(3, 5);
                }
            }
        }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            if (EffectiveToDate.HasValue)
            {
                if (EffectiveToDate.Value < EffectiveFromDate)
                {
                    yield return new ValidationResult("Effective To date cannot be before Effective From date", new[] { "EffectiveToDate" });
                }
            }
        }
    }
}
