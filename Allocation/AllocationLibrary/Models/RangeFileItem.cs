namespace Footlocker.Logistics.Allocation.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RangeFileItem 
    {
        #region Fields

        private readonly string _DEFAULT_END_OF_TIME_DATE_VALUE = "99991231";

        #endregion

        private bool valid = true;
        public void MarkInValid()
        {
            valid = false;
        }

        public bool IsValid()
        {
            return valid;
        }

        public string ProductIdent { get; set; }

        public string MerchantSku
        {
            get
            {
                try
                {
                    //return SkuSize.Substring(0, 14);
                    return ProductIdent.Substring(0, 2) + "-" + ProductIdent.Substring(2, 2) + "-" + ProductIdent.Substring(4, 5) + "-" + ProductIdent.Substring(9, 2);
                }
                catch
                {
                    return "";
                }
            }
            set { }
        }

        public string Size
        {
            get
            {
                try
                {
                    return ProductIdent.Substring(11);
                }
                catch {
                    return "";
                }
            }
            set { }
        }

        public string LocationTypeCode { get; set; }
        public string LocationID { get; set; }
        public string ClusterID { get; set; }

        public string StrategyCode { get; set; }

        public string MaxStockQty { get; set; }

        public string NonsellableQty { get; set; }

        public string SodStockQty { get; set; }

        public string EodStockQty { get; set; }

        public string SourceLocTypeCode { get; set; }

        public string SourceLocID { get; set; }

        public string SourceLeadTime { get; set; }

        public string OnRangeDt { get; set; }

        public string MarkdownDt { get { return _DEFAULT_END_OF_TIME_DATE_VALUE; } }

        public string OffRangeDt { get; set; }

        public string TodayUnitCost { get; set; }

        public string TodayUnitRetail { get; set; }

        public string MarkdownRetail { get; set; }

        public string RcvAfterMdInd { get; set; }

        public string AllowExcessInd { get; set; }

        public string MinLife { get; set; }

        public string InitWklyDemand { get; set; }

        public string Attribute1 { get; set; }

        public string Attribute2 { get; set; }

        public string Attribute3 { get; set; }

        public string Attribute4 { get; set; }

        public string Attribute5 { get; set; }

        public string Attribute6 { get; set; }
 
        public string Attribute7 { get; set; }

        public string Attribute8 { get; set; }

        public string Attribute9 { get; set; }

        public string Attribute10 { get; set; }

        public string Attribute11 { get; set; }

        public string Attribute12 { get; set; }

        public string Attribute13 { get; set; }

        public string Attribute14 { get; set; }

        public string Attribute15 { get; set; }

        public string Attribute16 { get; set; }

        public string Attribute17 { get; set; }

        public string Attribute18 { get; set; }

        public string Attribute19 { get; set; }

        public string MldInd { get; set; }

        public string FirstReceivableDt { get; set; }
        public string LearningTransitionCode { get; set; }
        public string MinEndDate { get; set; }
        public bool Ranged { get; set; }

    }
}
