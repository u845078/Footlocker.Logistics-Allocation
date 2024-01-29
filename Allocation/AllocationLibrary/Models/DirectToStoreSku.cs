using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class DirectToStoreSku 
    {
        [Key]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        public string Sku { get; set; }
        public long ItemID { get; set; }

        public string Vendor { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }
        public bool Rounding { get; set; }
        public string RoundMethod { get; set; }

        [Display(Name="Buying Multiple")]
        public int VendorPackQty { get; set; }
        [Display(Name = "Sun")]
        public bool OrderSun { get; set; }
        [Display(Name = "Mon")]
        public bool OrderMon { get; set; }
        [Display(Name = "Tue")]
        public bool OrderTue { get; set; }
        [Display(Name = "Wed")]
        public bool OrderWed { get; set; }
        [Display(Name = "Thurs")]
        public bool OrderThur { get; set; }
        [Display(Name = "Fri")]
        public bool OrderFri { get; set; }
        [Display(Name = "Sat")]
        public bool OrderSat { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        [NotMapped]
        public string OrderDays
        {
            get
            {
                string retval = "";
                
                if (OrderSun)                
                    retval += "Sun,";
                
                if (OrderMon)                
                    retval += "Mon,";
                
                if (OrderTue)                
                    retval += "Tue,";
                
                if (OrderWed)                
                    retval += "Wed,";
                
                if (OrderThur)                
                    retval += "Thur,";
                
                if (OrderFri)                
                    retval += "Fri,";
                
                if (OrderSat)                
                    retval += "Sat,";
                
                if (retval.Length > 0)                
                    retval = retval.Substring(0, retval.Length - 1);
                
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
