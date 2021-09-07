using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("vDTSConstraints")]
    public class DTSConstraintModel
    {

        [Key]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        public string Sku { get; set; }
        public Int64 ItemID { get; set; }
        public string Vendor { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }
        public Boolean Rounding { get; set; }
        public String RoundMethod { get; set; }

        [Display(Name = "Buying Multiple")]
        public int VendorPackQty { get; set; }
        [Display(Name = "Sun")]
        public Boolean OrderSun { get; set; }
        [Display(Name = "Mon")]
        public Boolean OrderMon { get; set; }
        [Display(Name = "Tue")]
        public Boolean OrderTue { get; set; }
        [Display(Name = "Wed")]
        public Boolean OrderWed { get; set; }
        [Display(Name = "Thurs")]
        public Boolean OrderThur { get; set; }
        [Display(Name = "Fri")]
        public Boolean OrderFri { get; set; }
        [Display(Name = "Sat")]
        public Boolean OrderSat { get; set; }
        public string CreatedBy { get; set; }
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