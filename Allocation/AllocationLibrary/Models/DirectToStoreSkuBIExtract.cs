using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    public class DirectToStoreSkuBIExtract : BiExtract
    {
        public override bool IsValid()
        {
            return true;
        }

        [Key]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        [StringLayoutDelimited(0)]
        public string Sku { get; set; }

        [StringLayoutDelimited(1)]
        public string Vendor { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [StringLayoutDelimited(2, "yyyy-MM-dd")]
        public DateTime StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [StringLayoutDelimited(3, "yyyy-MM-dd")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Pack Qty")]
        [StringLayoutDelimited(4)]
        public int VendorPackQty { get; set; }

        [Display(Name = "Sun")]
        [StringLayoutDelimited(5)]
        public string OrderSun { get; set; }

        [Display(Name = "Mon")]
        [StringLayoutDelimited(6)]
        public string OrderMon { get; set; }

        [Display(Name = "Tue")]
        [StringLayoutDelimited(7)]
        public string OrderTue { get; set; }

        [Display(Name = "Wed")]
        [StringLayoutDelimited(8)]
        public string OrderWed { get; set; }

        [Display(Name = "Thurs")]
        [StringLayoutDelimited(9)]
        public string OrderThur { get; set; }

        [Display(Name = "Fri")]
        [StringLayoutDelimited(10)]
        public string OrderFri { get; set; }

        [Display(Name = "Sat")]
        [StringLayoutDelimited(11)]
        public string OrderSat { get; set; }

        [StringLayoutDelimited(12)]
        public string CreatedBy { get; set; }

        [StringLayoutDelimited(13, "yyyy-MM-dd h:mm:ss tt")]
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// Initializes a new instance of the DirectToStoreSkuBIExtract class.
        /// </summary>
        public DirectToStoreSkuBIExtract()
        {
            this.Sku = String.Empty;
            this.Vendor = String.Empty;
            this.StartDate = DateTime.MinValue;
            this.EndDate = new DateTime?();
            this.VendorPackQty = 0;
            this.OrderSun = String.Empty;
            this.OrderMon = String.Empty;
            this.OrderTue = String.Empty;
            this.OrderWed = String.Empty;
            this.OrderThur = String.Empty;
            this.OrderFri = String.Empty;
            this.OrderSat = String.Empty;
            this.CreatedBy = String.Empty;
            this.CreateDate = new DateTime?();
        }

        /// <summary>
        /// Initialize a new instance of the DirectToStoreSkuBIExtract class.
        /// </summary>
        /// <param name="sku">The initial value for the stock keeping unit property.</param>
        /// <param name="vendor">The initial value for the vendor property.</param>
        /// <param name="startDate">The initial value for the start date property.</param>
        /// <param name="endDate">The initial value for the end date property.</param>
        /// <param name="vendorPackQty">The initial value for the vendor pack quantity property.</param>
        /// <param name="orderSun">The initial value for the order Sunday property.</param>
        /// <param name="orderMon">The initial value for the order Monday property.</param>
        /// <param name="orderTue">The initial value for the order Tuesday property.</param>
        /// <param name="orderWed">The initial value for the order Wednesday property.</param>
        /// <param name="orderThur">The initial value for the order Thursday property.</param>
        /// <param name="orderFri">The initial value for the order Friday property.</param>
        /// <param name="orderSat">The initial value for the order Saturday property.</param>
        /// <param name="createdBy">The initial value for the created by property.</param>
        /// <param name="createDate">The initial value for the create date property.</param>
        public DirectToStoreSkuBIExtract(string sku, string vendor, DateTime startDate, DateTime? endDate
                , int vendorPackQty, string orderSun, string orderMon, string orderTue, string orderWed, string orderThur
                , string orderFri, string orderSat, string createdBy, DateTime? createDate)
            : this()
        {
            this.Sku = sku;
            this.Vendor = vendor;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.VendorPackQty = vendorPackQty;
            this.OrderSun = orderSun;
            this.OrderMon = orderMon;
            this.OrderTue = orderTue;
            this.OrderWed = orderWed;
            this.OrderThur = orderThur;
            this.OrderFri = orderFri;
            this.OrderSat = orderSat;
            this.CreatedBy = createdBy;
            this.CreateDate = createDate;
        }
    }
}
