using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreAttribute
    {
        public int ID { get; set; }
        public string Division { get; set; }
        public string Store { get; set; }
        public string LikeDivision { get; set; }
        public string LikeStore { get; set; }
        public string Level { get; set; }
        public string Value { get; set; }

        [Range(1,100)]
        [Display(Name="Like Store Weight")]
        public int Weight { get; set; }

        [Display(Name = "Like Store Demand Scaling Factor")]
        public decimal LikeStoreDemandScalingFactor { get; set; }
        
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)] 
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]   
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }

        private string _valueDescription = null;

        [NotMapped]
        public string ValueDescription
        {
          get 
          {
              if (_valueDescription != null)
                  return _valueDescription;
              else
                  return Value;
          }
          set { _valueDescription = value; }
        }

        public StoreExtension Extension { get; set; }
    }
}
