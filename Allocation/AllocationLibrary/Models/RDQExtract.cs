using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RDQExtract : BiExtract
    {
        public override bool IsValid()
        {
            return true;
        }

        [StringLayoutDelimited(0)]
        public Int64 ID { get; set; }

        [RegularExpression(@"^\d{2}$", ErrorMessage = "Division must be in the format ##")]
        [StringLayoutDelimited(1)]
        public String Division { get; set; }

        [RegularExpression(@"^\d{5}$", ErrorMessage = "Store number must be in the format #####")]
        [StringLayoutDelimited(2)]
        public String Store { get; set; }

        [StringLayoutDelimited(3)]
        public String Sku { get; set; }

        [StringLayoutDelimited(4)]
        public String Size { get; set; }

        //[StringLayoutDelimited(5)]
        public Int32 Qty { get; set; }

        [NotMapped]
        [Display(Name = "Bin Qty")]
        [StringLayoutDelimited(6)]
        public Int32 BinQty { get; set; }

        [NotMapped]
        [Display(Name = "Case Qty")]
        [StringLayoutDelimited(7)]
        public Int32 CaseQty { get; set; }

        [StringLayoutDelimited(8)]
        public String DCID { get; set; }

        [StringLayoutDelimited(9)]
        public String PO { get; set; }

        [StringLayoutDelimited(10)]
        public String Type { get; set; }

        [StringLayoutDelimited(11)]
        public String DestinationType { get; set; }

        [StringLayoutDelimited(12)]
        public String Status { get; set; }

        [StringLayoutDelimited(13)]
        public String CreatedBy { get; set; }

        [StringLayoutDelimited(14, "yyyy-MM-dd h:mm:ss tt")]
        public DateTime? CreateDate { get; set; }

        [NotMapped]
        [StringLayoutDelimited(15)]
        public String ActiveInd { get; set; }

        [StringLayoutDelimited(16, "yyyy-MM-dd h:mm:ss tt")]
        public DateTime? ExpectedShipDate { get; set; }

        [StringLayoutDelimited(17, "yyyy-MM-dd h:mm:ss tt")]
        public DateTime? ExpectedReceiptDate { get; set; }

        [StringLayoutDelimited(18)]
        public Decimal NeedQty { get; set; }

        [StringLayoutDelimited(19)]
        public Decimal TargetQty { get; set; }

        [StringLayoutDelimited(20)]
        public Decimal ForecastQty { get; set; }

        [StringLayoutDelimited(21)]
        public Decimal? OptimalQty { get; set; }

        [StringLayoutDelimited(22)]
        public Decimal UserRequestedQty { get; set; }

        [StringLayoutDelimited(23)]
        public Decimal? RequestedQty { get; set; }

        /// <summary>
        /// Initializes a new instance of the RDQExtract class.
        /// </summary>
        public RDQExtract()
        {
            this.ID = 0L;
            this.Division = String.Empty;
            this.Store = String.Empty;
            this.Sku = String.Empty;
            this.Size = String.Empty;
            this.Qty = 0;
            this.BinQty = 0;
            this.CaseQty = 0;
            this.DCID = String.Empty;
            this.PO = String.Empty;
            this.Type = String.Empty;
            this.DestinationType = String.Empty;
            this.Status = String.Empty;
            this.CreatedBy = String.Empty;
            this.CreateDate = new DateTime?();
            this.ActiveInd = String.Empty;
            this.ExpectedShipDate = new DateTime?();
            this.ExpectedReceiptDate = new DateTime?();
            this.NeedQty = 0;
            this.TargetQty = 0;
            this.ForecastQty = 0;
            this.OptimalQty = 0;
            this.UserRequestedQty = 0;
            this.RequestedQty = 0;
        }

        /// <summary>
        /// Initializes a new instance of the RDQExtract class.
        /// </summary>
        /// <param name="id">The initial value for the identifier property.</param>
        /// <param name="division">The initial value for the division property.</param>
        /// <param name="store">The initial value for the store property.</param>
        /// <param name="sku">The initial value for the stock keeping unit property.</param>
        /// <param name="size">The initial value for the size property.</param>
        /// <param name="qty">The initial value for the quantity property.</param>
        /// <param name="dcid">The initial value for the distribution center identifier property.</param>
        /// <param name="po">The initial value for the purchase order property.</param>
        /// <param name="type">The initial value for the type property.</param>
        /// <param name="destinationType">The initial value for the destination type property.</param>
        /// <param name="status">The initial value for the status property.</param>
        /// <param name="createdBy">The initial value for the created by property.</param>
        /// <param name="createDate">The initial value for the create date property.</param>
        public RDQExtract(Int64 id, string division, string store, string sku, string size, Int32 qty, Int32 binQty, Int32 caseQty
            , string dcid, string po, string type, string destinationType, string status, string createdBy, DateTime? createDate
            , string activeInd, DateTime? expectedShipDate, DateTime? expectedReceiptDate, Decimal needQty, Decimal targetQty
            , Decimal forecastQty, Decimal? optimalQty, Decimal userRequestedQty, Decimal? requestedQty)
            : this()
        {
            this.ID = id;
            this.Division = division;
            this.Store = store;
            this.Sku = sku;
            this.Size = size;
            this.Qty = qty;
            this.BinQty = binQty;
            this.CaseQty = caseQty;
            this.DCID = dcid;
            this.PO = po;
            this.Type = type;
            this.DestinationType = destinationType;
            this.Status = status;
            this.CreatedBy = createdBy;
            this.CreateDate = createDate;
            this.ActiveInd = activeInd;
            this.ExpectedShipDate = new DateTime?();
            this.ExpectedReceiptDate = new DateTime?();
            this.NeedQty = needQty;
            this.TargetQty = targetQty;
            this.ForecastQty = forecastQty;
            this.OptimalQty = optimalQty;
            this.UserRequestedQty = userRequestedQty;
            this.RequestedQty = requestedQty;
        }
    }
}
