using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class SizeAllocation
    {
        [Key]
        [Column(Order = 0)]
        public Int64 PlanID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Division { get; set; }

        [Key]
        [Column(Order = 2)]
        public string Store { get; set; }

        [Key]
        [Column(Order = 3)]
        public string Size { get; set; }

        public Int32? Min { get; set; }
        //public string MinString 
        //{ 
        //    get 
        //    {
        //        if (Min >= 0)
        //        {
        //            return Min.ToString();
        //        }
        //        else
        //        {
        //            return "";
        //        }
        //    }

        //    set
        //    {
        //        Min = Convert.ToInt32(value);
        //    }
        //}
        public Int32? Max { get; set; }
        public Int32? Days { get; set; }


        public Int16 RangeFromDB 
        {
            get
            {
                return Convert.ToInt16(Range);
            }
            set 
            {
                Range = Convert.ToBoolean(value);
            } 
        }

        [NotMapped]
        public Boolean Range { get; set; }
        public string InitialDemand { get; set; }

        [NotMapped]
        public string League { get; set; }

        [NotMapped]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }

        [NotMapped]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? MinEndDate { get; set; }

        [NotMapped]
        public string RangeType { get; set; }

        [NotMapped]
        public int RangeTypeIndex 
        {
            get
            {
                switch (RangeType)
                { 
                    case "ALR":
                        return 1;
                    case "OP":
                        return 2;
                    default:
                        return 0;
                }
            }
        }
    }
}
