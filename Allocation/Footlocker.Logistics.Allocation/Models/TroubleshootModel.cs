using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Logistics.Allocation.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Footlocker.Logistics.Allocation.Models
{
    public class TroubleshootModel
    {
        [Required]
        public string Sku { get; set; }

        public string Division
        {
            get 
            {
                if (string.IsNullOrEmpty(Sku))
                    return "";
                else
                    return Sku.Split('-')[0];
            }
        }
        public string Size { get; set; }
        public List<DistributionCenter> AllDCs { get; set; }
        public int Warehouse { get; set; }
        public string Store { get; set; }

        public List<RangePlan> RangePlans { get; set; }
        //public List<RangePlanDetail> RangePlanDetails { get; set; }
        public List<Hold> Holds { get; set; }
        public List<RingFence> RingFences { get; set; }
        //public List<RingFenceDetail> RingFenceDetails {get;set;}
        public List<ExpeditePO> POOverrides { get; set; }

        public List<SizeObj> Sizes { get; set; }
        public ItemMaster ItemMaster { get; set; }

        public Departments Department { get; set; }

        public string DepartmentDisplay
        {
            get
            {
                if (Department != null)
                    return Department.departmentCode + " - " + Department.departmentName;
                else
                    return "";
            }
        }

        public Categories Category { get; set; }

        public string CategoryDisplay
        {
            get
            {
                if (Category != null)
                    return Category.CategoryDisplay;
                else
                    return "";
            }
        }

        public Vendors Vendor { get; set; }

        public string VendorDisplay
        {
            get
            {
                if (Vendor != null)
                    return Vendor.VendorDisplay;
                else
                    return "";
            }
        }

        public BrandIDs BrandID { get; set; }
        public string BrandIDDisplay
        {
            get
            {
                if (BrandID != null)
                    return BrandID.brandIDDisplay;
                else
                    return "";
            }
        }

        public TeamCodes TeamCode { get; set; }

        public string TeamCodeDisplay
        {
            get
            {
                if (TeamCode != null)
                    return TeamCode.TeamCodeDisplay;
                else
                    return "";
            }
        }

        public string ColorsDisplay
        {
            get
            {
                string result = "";

                if (!string.IsNullOrEmpty(ItemMaster.Color1))
                    result += ItemMaster.Color1 + " - " + ItemMaster.Color1Desc;

                if (!string.IsNullOrEmpty(ItemMaster.Color2))
                    result += "; " + ItemMaster.Color2 + " - " + ItemMaster.Color2Desc;

                if (!string.IsNullOrEmpty(ItemMaster.Color3))
                    result += "; " + ItemMaster.Color3 + " - " + ItemMaster.Color3Desc;

                return result;
            }
        }

        public string LifeCycleDisplay
        {
            get
            {
                string result = "";

                if (!string.IsNullOrEmpty(ItemMaster.LifeCycle))
                    result += ItemMaster.LifeCycle + " (" + ItemMaster.LifeCycleDays.ToString() + " days)";

                return result;
            }
        }

        public string MaterialDisplay
        {
            get
            {
                string result = "";

                if (!string.IsNullOrEmpty(ItemMaster.Material))
                    result += ItemMaster.Material + " - " + ItemMaster.MaterialDesc;

                return result;
            }
        }
        public bool ValidItem { get; set; }
        public AllocationDriver AllocationDriver { get; set; }
        public DateTime ControlDate { get; set; }
        public string CPID { get; set; }

        public string RetailPriceCurrency { get; set; }

        [DisplayName("Retail Price")]
        public decimal RetailPrice { get; set; }

        public List<RDQ> RDQs { get; set; }
    }
}