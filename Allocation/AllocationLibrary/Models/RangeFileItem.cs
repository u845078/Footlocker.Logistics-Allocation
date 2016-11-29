namespace Footlocker.Logistics.Allocation.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Footlocker.Common.Utilities.File;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RangeFileItem : StringLayoutDelimitedUtility
    {
        #region Fields

        private readonly string _DEFAULT_END_OF_TIME_DATE_VALUE = "99991231";

        #endregion

        private bool valid = true;
        public void MarkInValid()
        {
            valid = false;
        }

        public override bool IsValid()
        {
            return valid;
        }

        [StringLayoutDelimited(0)]
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

        [StringLayoutDelimited(1)]
        public string LocationTypeCode { get; set; }
        [StringLayoutDelimited(2)]
        public string LocationID { get; set; }
        [StringLayoutDelimited(3)]
        public string ClusterID { get; set; }
        [StringLayoutDelimited(4)]
        public string StrategyCode { get; set; }
        [StringLayoutDelimited(5)]
        public string MaxStockQty { get; set; }
        [StringLayoutDelimited(6)]
        public string NonsellableQty { get; set; }
        [StringLayoutDelimited(7)]
        public string SodStockQty { get; set; }
        [StringLayoutDelimited(8)]
        public string EodStockQty { get; set; }
        [StringLayoutDelimited(9)]
        public string SourceLocTypeCode { get; set; }
        [StringLayoutDelimited(10)]
        public string SourceLocID { get; set; }
        [StringLayoutDelimited(11)]
        public string SourceLeadTime { get; set; }
        [StringLayoutDelimited(12)]
        public string OnRangeDt { get; set; }
        [StringLayoutDelimited(13)]
        public string MarkdownDt { get { return _DEFAULT_END_OF_TIME_DATE_VALUE; } }
        [StringLayoutDelimited(14)]
        public string OffRangeDt { get; set; }
        [StringLayoutDelimited(15)]
        public string TodayUnitCost { get; set; }
        [StringLayoutDelimited(16)]
        public string TodayUnitRetail { get; set; }
        [StringLayoutDelimited(17)]
        public string MarkdownRetail { get; set; }
        [StringLayoutDelimited(18)]
        public string RcvAfterMdInd { get; set; }
        [StringLayoutDelimited(19)]
        public string AllowExcessInd { get; set; }
        [StringLayoutDelimited(20)]
        public string MinLife { get; set; }
        [StringLayoutDelimited(21)]
        public string InitWklyDemand { get; set; }
        [StringLayoutDelimited(22)]
        public string Attribute1 { get; set; }
        [StringLayoutDelimited(23)]
        public string Attribute2 { get; set; }
        [StringLayoutDelimited(24)]
        public string Attribute3 { get; set; }
        [StringLayoutDelimited(25)]
        public string Attribute4 { get; set; }
        [StringLayoutDelimited(26)]
        public string Attribute5 { get; set; }
        [StringLayoutDelimited(27)]
        public string Attribute6 { get; set; }
        [StringLayoutDelimited(28)]
        public string Attribute7 { get; set; }
        [StringLayoutDelimited(29)]
        public string Attribute8 { get; set; }
        [StringLayoutDelimited(30)]
        public string Attribute9 { get; set; }
        [StringLayoutDelimited(31)]
        public string Attribute10 { get; set; }
        [StringLayoutDelimited(32)]
        public string Attribute11 { get; set; }
        [StringLayoutDelimited(33)]
        public string Attribute12 { get; set; }
        [StringLayoutDelimited(34)]
        public string Attribute13 { get; set; }
        [StringLayoutDelimited(35)]
        public string Attribute14 { get; set; }
        [StringLayoutDelimited(36)]
        public string Attribute15 { get; set; }
        [StringLayoutDelimited(37)]
        public string Attribute16 { get; set; }
        [StringLayoutDelimited(38)]
        public string Attribute17 { get; set; }
        [StringLayoutDelimited(39)]
        public string Attribute18 { get; set; }
        [StringLayoutDelimited(40)]
        public string Attribute19 { get; set; }
        [StringLayoutDelimited(41)]
        public string MldInd { get; set; }
        [StringLayoutDelimited(42)]
        public string FirstReceivableDt { get; set; }
        public string LearningTransitionCode { get; set; }
        public string MinEndDate { get; set; }
        public Boolean Ranged { get; set; }

    }
}
