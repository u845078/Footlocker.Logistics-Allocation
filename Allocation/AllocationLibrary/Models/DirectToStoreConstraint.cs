using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class DirectToStoreConstraint 
    {
        [Key]
        [Column(Order=0)]
        public string Sku { get; set; }

        public long ItemID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Size { get; set; }

        public int MaxQty { get; set; }

        private int _maxQtyCase = -1;
        [NotMapped]
        public int MaxQtyCase 
        {
            get
            {
                if (_maxQtyCase < 0) { _maxQtyCase = Math.Max(0, MaxQty)/Math.Max(1, VendorPackQty); }

                return _maxQtyCase;
            }
            set
            {
                _maxQtyCase = value;
            }
        }

        [Display(Name="Start")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreateDate { get; set; }

        [NotMapped]
        public int VendorPackQty { get; set; }

        [NotMapped]
        public string Division
        {
            get
            {
                if (Sku.Length > 2)                
                    return Sku.Substring(0, 2);
                else
                    return "";
            }
        }

        [NotMapped]
        public string Department
        {
            get
            {
                if (Sku.Length > 5)                
                    return Sku.Substring(3, 2);
                else
                    return "";
            }
        }

        [NotMapped]
        public string Description { get; set; }

        [NotMapped]
        public string OrderDays { get; set; }

        [NotMapped]
        public string VendorNumber { get; set; }
        
        [NotMapped]
        public string VendorDesc { get; set; }

        /// <summary>
        /// Initializes a new instance of the DirectToStoreConstraint class.
        /// </summary>
        public DirectToStoreConstraint()
        {
            this.Sku = string.Empty;
            this.ItemID = 0L;
            this.Size = string.Empty;
            this.MaxQty = 0;
            this.StartDate = DateTime.MinValue;
            this.EndDate = new DateTime?();
            this.CreatedBy = string.Empty;
            this.CreateDate = DateTime.MinValue;
            this.Description = string.Empty;
            this.VendorPackQty = 0;
        }
    }
}
