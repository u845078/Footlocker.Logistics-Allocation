using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    public class DirectToStoreConstraint : BiExtract
    {
        public override bool IsValid()
        {
            return true;
        }

        [Key]
        [Column(Order=0)]
        [StringLayoutDelimited(0)]
        public string Sku { get; set; }

        public Int64 ItemID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLayoutDelimited(1)]
        public string Size { get; set; }

        [StringLayoutDelimited(2)]
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
        [StringLayoutDelimited(3, "yyyy-MM-dd")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End")]
        [StringLayoutDelimited(4, "yyyy-MM-dd")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [StringLayoutDelimited(5)]
        public string CreatedBy { get; set; }

        [StringLayoutDelimited(6, "yyyy-MM-dd h:mm:ss tt")]
        public DateTime CreateDate { get; set; }

        [StringLayoutDelimited(7)]
        [NotMapped]
        public int VendorPackQty { get; set; }

        [NotMapped]
        public string Division
        {
            get
            {
                if (Sku.Length > 2)
                {
                    return Sku.Substring(0, 2);
                }
                return "";
            }

        }

        [NotMapped]
        public string Department
        {
            get
            {
                if (Sku.Length > 5)
                {
                    return Sku.Substring(3, 2);
                }
                return "";
            }
        }

        [NotMapped]
        public string Description { get; set; }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------
        // HACK: It seems as though the 'BIExtract' objects have been leveraged to support new UIs, rather than creating new models
        // These properties below are needed soley for the UI, not the extract, and as our concerns are not separated, we hope these changes for the UI do not compromise our extract
        // --------------------------------------------------------------------------------------------------------------------------------------------------------
        [NotMapped]
        public string OrderDays { get; set; }

        [NotMapped]
        public string VendorNumber { get; set; }
        
        [NotMapped]
        public string VendorDesc { get; set; }
        // --------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the DirectToStoreConstraint class.
        /// </summary>
        public DirectToStoreConstraint()
        {
            this.Sku = String.Empty;
            this.ItemID = 0L;
            this.Size = String.Empty;
            this.MaxQty = 0;
            this.StartDate = DateTime.MinValue;
            this.EndDate = new DateTime?();
            this.CreatedBy = String.Empty;
            this.CreateDate = DateTime.MinValue;
            this.Description = String.Empty;
            this.VendorPackQty = 0;
        }

        /// <summary>
        /// Initializes a new instance of the DirectToStoreConstraint class.
        /// </summary>
        /// <param name="sku">The initial value for the stock keeping unit property.</param>
        /// <param name="itemId">The initial value for the item identifier property.</param>
        /// <param name="size">The initial value for the size property.</param>
        /// <param name="maxQty">The initial value for the maximum quantity property.</param>
        /// <param name="startDate">The initial value for the start date property.</param>
        /// <param name="endDate">The initial value for the end date property.</param>
        /// <param name="createdBy">The initial value for the created by property.</param>
        /// <param name="createDate">The initial value for the create date property.</param>
        /// <param name="description">The initial value for the description property.</param>
        public DirectToStoreConstraint(string sku, Int64 itemId, string size, int maxQty, DateTime startDate
                , DateTime? endDate, string createdBy, DateTime createDate, string description, int vendorPackQty)
            : this()
        {
            this.Sku = sku;
            this.ItemID = itemId;
            this.Size = size;
            this.MaxQty = maxQty;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.CreatedBy = createdBy;
            this.CreateDate = createDate;
            this.Description = description;
            this.VendorPackQty = vendorPackQty;
        }
    }
}
