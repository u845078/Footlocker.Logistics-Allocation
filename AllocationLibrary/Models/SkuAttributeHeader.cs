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
                if ((Category == null)||(Category == ""))
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
                if ((Brand == null) || (Brand == ""))
                    return "default";
                else
                    return Brand;
            }
            set { }
        }
        public string CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public decimal WeightActive { get; set; }
        [NotMapped]
        public Int32 WeightActiveInt 
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
        public Int32 WeightInactive { get { return Convert.ToInt32(1 - WeightActive * 100); } }

        public List<SkuAttributeDetail> SkuAttributeDetails { get; set; }
    }
}
