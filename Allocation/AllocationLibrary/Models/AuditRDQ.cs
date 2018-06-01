using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    public class AuditRDQ
    {
        public Int64 ID { get; set; }
        public Int64 RDQID { get; set; }
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Division must be in the format ##")]
        public String Division { get; set; }

        [Required]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Store number must be in the format #####")]
        public String Store { get; set; }
        
        public Int32? DCID { get; set; }
        [NotMapped]
        [Display(Name="Warehouse")]
        public String WarehouseName {get;set;}

        public String PO { get; set; }
        public Int64? ItemID { get; set; }
        //public String Type { get; set; }
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "Sku must be in the format ##-##-#####-##")]
        public String Sku { get; set; }
        [RegularExpression(@"^\d{3}(?:\d{2})?$", ErrorMessage = "Size must be 3 or 5 digits")]
        public String Size { get; set; }
        public Int32 Qty { get; set; }
        public Int32? TargetQty { get; set; }
        public Int32? ForecastQty { get; set; }
        public Int32? NeedQty { get; set; }
        [NotMapped]
        public Int32 UserRequestedQty
        {
            get
            {
                if (UserRequestedQtyString != null)
                {
                    return Convert.ToInt32(UserRequestedQtyString);
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                try
                {
                    UserRequestedQtyString = Convert.ToString(value);
                }
                catch { }
            }
        }
        [Column("UserRequestedQty")]
        public string UserRequestedQtyString { get; set; }        //public String CreatedBy { get; set; }
        //public DateTime? CreateDate { get; set; }
        //public String DestinationType { get; set; }
        //public String Status { get; set; }

        //[NotMapped]
        //public String Pick
        //{
        //    get
        //    {
        //        switch (Status)
        //        { 
        //            case "WEB PICK":
        //                return "Store's next pick day";
        //            case "FORCE PICK":
        //                return "Pick tomorrow";
        //        }
        //        return "unknown";
        //    }
        //}

        public string Comment { get; set; }
        public string PickedBy { get; set; }
        public DateTime PickDate { get; set; }

        //public virtual DistributionCenter DistributionCenter { get; set; }

    }
}
