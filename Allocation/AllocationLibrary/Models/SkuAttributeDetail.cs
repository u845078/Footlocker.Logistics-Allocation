using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public enum SkuAttibuteType { Department, Category, LifeOfSku, TeamCode, VendorNumber,BrandID,Skuid1, Skuid2,Skuid3,Skuid4,Skuid5,
    color1,color2,color3,Gender,Material,SizeRange,Size}

    public class SkuAttributeDetail
    {
        public SkuAttributeDetail()
        {
        }
        public SkuAttributeDetail(string type, Boolean mandatory, Decimal weight)
        {
            AttributeType = type;
            Mandatory = mandatory;
            Weight = weight;
        }

        [Key]
        [Column(Order=0)]
        public int HeaderID { get; set; }
        [Key]
        [Column(Order = 1)]
        public string AttributeType { get; set; }
        public Boolean Mandatory { get; set; }
        public Decimal Weight { get; set; }
        [NotMapped]
        public Int32 WeightInt 
        {
            get
            {
                return Convert.ToInt32(Weight * 100);
            }
            set
            {
                Weight = Convert.ToDecimal(value) / Convert.ToDecimal(100.0);
            }
        }

        public Decimal WeightForInactive { get; set; }
        [NotMapped]
        public Int32 WeightForInactiveInt
        {
            get
            {
                return Convert.ToInt32(WeightForInactive * 100);
            }
            set
            {
                WeightForInactive = Convert.ToDecimal(value) / Convert.ToDecimal(100.0);
            }
        }

        [NotMapped]
        public Int32 SortOrder
        {
            get
            {
                switch (AttributeType)
                {
                    case "Department":
                        return 1;
                    case "Category":
                        return 2;
                    case "VendorNumber":
                        return 3;
                    case "BrandID":
                        return 4;
                    case "Size":
                        return 5;
                    case "SizeRange":
                        return 6;

                    default: 
                        return 9999;
                }
            }
        }
    }
}
