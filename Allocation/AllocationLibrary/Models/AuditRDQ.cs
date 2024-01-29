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
        public long ID { get; set; }
        public long RDQID { get; set; }
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Division must be in the format ##")]
        public string Division { get; set; }

        [Required]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Store number must be in the format #####")]
        public string Store { get; set; }
        
        public int? DCID { get; set; }
        [NotMapped]
        [Display(Name="Warehouse")]
        public string WarehouseName {get;set;}

        public string PO { get; set; }
        public long? ItemID { get; set; }
        //public String Type { get; set; }
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "Sku must be in the format ##-##-#####-##")]
        public string Sku { get; set; }
        [RegularExpression(@"^\d{3}(?:\d{2})?$", ErrorMessage = "Size must be 3 or 5 digits")]
        public string Size { get; set; }
        public int Qty { get; set; }
        public int? TargetQty { get; set; }
        public int? ForecastQty { get; set; }
        public int? NeedQty { get; set; }
        [NotMapped]
        public int UserRequestedQty
        {
            get
            {
                if (!string.IsNullOrEmpty(UserRequestedQtyString))                
                    return Convert.ToInt32(UserRequestedQtyString);                
                else                
                    return 0;                
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
        public string UserRequestedQtyString { get; set; }

        public string Comment { get; set; }
        public string PickedBy { get; set; }
        public DateTime PickDate { get; set; }
    }
}
