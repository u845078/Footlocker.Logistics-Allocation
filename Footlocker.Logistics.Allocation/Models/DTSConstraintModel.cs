using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("vDTSConstraints")]
    public class DTSConstraintModel
    {

        [Key]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        [StringLayoutDelimited(0)]
        public string Sku { get; set; }
        public Int64 ItemID { get; set; }
        [StringLayoutDelimited(1)]
        public string Vendor { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [StringLayoutDelimited(2, "yyyy-MM-dd")]
        public DateTime StartDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [StringLayoutDelimited(3, "yyyy-MM-dd")]
        public DateTime? EndDate { get; set; }
        public Boolean Rounding { get; set; }
        public String RoundMethod { get; set; }
        //[NotMapped]
        //public string Rnd
        //{
        //    get {
        //        if (Rounding)
        //        {
        //            return "Yes";
        //        }
        //        else
        //        {
        //            return "No";
        //        }
        //    }
        //    set { }
        //}
        [Display(Name = "Buying Multiple")]
        [StringLayoutDelimited(4)]
        public int VendorPackQty { get; set; }
        [Display(Name = "Sun")]
        [StringLayoutDelimited(5)]
        public Boolean OrderSun { get; set; }
        [Display(Name = "Mon")]
        [StringLayoutDelimited(6)]
        public Boolean OrderMon { get; set; }
        [Display(Name = "Tue")]
        [StringLayoutDelimited(7)]
        public Boolean OrderTue { get; set; }
        [Display(Name = "Wed")]
        [StringLayoutDelimited(8)]
        public Boolean OrderWed { get; set; }
        [Display(Name = "Thurs")]
        [StringLayoutDelimited(9)]
        public Boolean OrderThur { get; set; }
        [Display(Name = "Fri")]
        [StringLayoutDelimited(10)]
        public Boolean OrderFri { get; set; }
        [Display(Name = "Sat")]
        [StringLayoutDelimited(11)]
        public Boolean OrderSat { get; set; }
        [StringLayoutDelimited(12)]
        public string CreatedBy { get; set; }
        [StringLayoutDelimited(13, "yyyy-MM-dd h:mm:ss tt")]
        public DateTime? CreateDate { get; set; }
        [NotMapped]
        public string OrderDays
        {
            get
            {
                string retval = "";
                if (OrderSun)
                {
                    retval = retval + "Sun,";
                }
                if (OrderMon)
                {
                    retval = retval + "Mon,";
                }
                if (OrderTue)
                {
                    retval = retval + "Tue,";
                }
                if (OrderWed)
                {
                    retval = retval + "Wed,";
                }
                if (OrderThur)
                {
                    retval = retval + "Thur,";
                }
                if (OrderFri)
                {
                    retval = retval + "Fri,";
                }
                if (OrderSat)
                {
                    retval = retval + "Sat,";
                }
                if (retval.Length > 0)
                {
                    retval = retval.Substring(0, retval.Length - 1);
                }
                return retval;
            }
            set { }
        }

        [NotMapped]
        public string Division
        {
            get
            {
                try
                {
                    return Sku.Substring(0, 2);
                }
                catch
                {
                    return "";
                }
            }
        }

        [NotMapped]
        public string Department
        {
            get
            {
                try
                {
                    return Sku.Substring(3, 2);
                }
                catch
                {
                    return "";
                }
            }
        }

        public List<string> Vendors;

        public virtual ItemMaster ItemMaster { get; set; }

    }
}