using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class SizeAllocation
    {
        [Key]
        [Column(Order = 0)]
        public long PlanID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Division { get; set; }

        [Key]
        [Column(Order = 2)]
        public string Store { get; set; }

        [Key]
        [Column(Order = 3)]
        public string Size { get; set; }

        public int? Min { get; set; }

        public int? Max { get; set; }

        public decimal? InitialDemand { get; set; }

        public int? Days { get; set; }

        public short RangeFromDB 
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

        public int? MinEndDays { get; set; }

        [NotMapped]
        public DateTime DeliveryGroupStartDate { get; set; }

        [NotMapped]
        public int? DeliveryGroupMinEndDays { get; set; }

        [NotMapped]
        public int? StoreLeadTime { get; set; }

        [NotMapped]
        public string CalculatedMinEndDate
        {
            get
            {
                if (StoreLeadTime != null && DeliveryGroupStartDate != DateTime.MinValue && MinEndDays != null)
                {
                    int bufferDays = (int)(StoreLeadTime + MinEndDays);
                    return DeliveryGroupStartDate.AddDays(bufferDays).ToString("MM/dd/yyyy");
                }
                else if (StoreLeadTime != null && DeliveryGroupStartDate != DateTime.MinValue && MinEndDays == null && DeliveryGroupMinEndDays != null && Range)
                {
                    int bufferDays = (int)(StoreLeadTime + DeliveryGroupMinEndDays);
                    return DeliveryGroupStartDate.AddDays(bufferDays).ToString("MM/dd/yyyy");
                }

                return null;
            }
        }

        [NotMapped]
        public bool Range { get; set; }

        [NotMapped]
        public bool Fringe { get; set; }

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
