using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Validation
{
    public class RingFenceInput : IValidatableObject
    {
        private string _store;
        private string _sku;

        [Required]
        public string Division { get; set; }
        public string Store
        {
            get
            {
                return _store;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (value.Length <= 2)
                    {
                        _store = value.PadLeft(2, '0');
                    }
                    else
                    {
                        _store = value.PadLeft(5, '0');
                    }
                }
                else
                {
                    _store = value;
                }
            }
        }

        [Required(ErrorMessage = "SKU is required")]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "Invalid Sku, format should be ##-##-#####-##")]
        public string Sku
        {
            get
            {
                return _sku;
            }
            set
            {
                _sku = value.Trim();
            }
        }

        public string Comments { get; set; }
        public int Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Sku.Substring(0, 2) != Division)
            {
                yield return new ValidationResult("Invalid Sku, division does not match selection.", new[] { "Sku" });
            }
        }


    }
}
