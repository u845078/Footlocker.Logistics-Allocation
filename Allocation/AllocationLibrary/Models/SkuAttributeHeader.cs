using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class SkuAttributeHeader
    {

        public SkuAttributeHeader()
        {
            this.SkuAttributeDetails = new List<SkuAttributeDetail>();
        }
        public int ID { get; set; }
        public string Division { get; set; }
        public string Dept { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }

        [Display(Name="Category")]
        [NotMapped]
        public string CategoryForDisplay 
        {
            get
            {
                if (string.IsNullOrEmpty(Category))
                    return "default";
                else
                    return Category;
            }
            set { }
        }

        [Display(Name = "BrandID")]
        [NotMapped]
        public string BrandForDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(Brand))
                    return "default";
                else
                    return Brand;
            }
            set { }
        }
        public string CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public decimal WeightActive { get; set; }
        
        public string SKU { get; set; }
        [NotMapped]
        public int WeightActiveInt 
        { 
            get 
            { 
                return Convert.ToInt32(WeightActive * 100); 
            }
            set
            {
                WeightActive = Convert.ToDecimal(value) / 100;
            }
        }

        [NotMapped]
        public int WeightInactive 
        { 
            get 
            { 
                return Convert.ToInt32(1 - WeightActive * 100); 
            } 
        }

        [NotMapped]
        public string ErrorMessage { get; set; }

        public List<SkuAttributeDetail> SkuAttributeDetails { get; set; }
    }
}
