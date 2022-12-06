using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public enum SkuAttibuteType { Department, Category, LifeOfSku, TeamCode, VendorNumber,BrandID,Skuid1, Skuid2,Skuid3,Skuid4,Skuid5,
    color1,color2,color3,Gender,Material,SizeRange,Size}

    public class SkuAttributeDetail
    {
        public SkuAttributeDetail()
        {
        }
        public SkuAttributeDetail(string type, bool mandatory, decimal weight)
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
        public bool Mandatory { get; set; }
        public decimal Weight { get; set; }
        [NotMapped]
        public int WeightInt 
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

        public decimal WeightForInactive { get; set; }
        [NotMapped]
        public int WeightForInactiveInt
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
        public int SortOrder
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

        public SkuAttributeHeader header { get; set; }
    }
}
