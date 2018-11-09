using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml;

using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreRange : BiExtract
    {
        public override bool IsValid()
        {
            return true;
        }

        [StringLayoutDelimited(0)]
        public Int64 ID { get; set; }

        [StringLayoutDelimited(1)]
        public string Division { get; set; }

        [StringLayoutDelimited(2)]
        public string Store { get; set; }

        [StringLayoutDelimited(3)]
        [RegularExpression(@"^\d{2}-\d{2}-\d{5}-\d{2}$", ErrorMessage = "SKU must be in the format ##-##-#####-##")]
        public string Sku { get; set; }

        [StringLayoutDelimited(4)]
        public string Size { get; set; }

        [StringLayoutDelimited(5)]
        public string Min { get; set; }

        [StringLayoutDelimited(6)]
        public string Max { get; set; }

        [StringLayoutDelimited(7)]
        public string Days { get; set; }

        [StringLayoutDelimited(8)]
        public string InitialDemand { get; set; }

        [StringLayoutDelimited(9)]
        public string Range { get; set; }

        [StringLayoutDelimited(10, "yyyy-MM-dd")]
        public DateTime? StartDate { get; set; }

        [StringLayoutDelimited(11, "yyyy-MM-dd")]
        public DateTime? EndDate { get; set; }

        [StringLayoutDelimited(12)]
        public string CreatedBy { get; set; }

        [StringLayoutDelimited(13, "yyyy-MM-dd h:mm:ss tt")]
        public DateTime CreateDate { get; set; }

        [StringLayoutDelimited(14, "yyyy-MM-dd")]
        public DateTime? FirstReceiptDate { get; set; }

        [StringLayoutDelimited(15)]
        public string DeliveryGroupName { get; set; }

        [StringLayoutDelimited(16)]
        public Boolean? Launch { get; set; }

        [StringLayoutDelimited(17, "yyyy-MM-dd")]
        public DateTime? LaunchDate { get; set; }

        [StringLayoutDelimited(18)]
        public string Description { get; set; }

        [StringLayoutDelimited(19)]
        public string PlanType { get; set; }

        [StringLayoutDelimited(20, "yyyy-MM-dd")]
        public DateTime? DeliveryGroupStartDate { get; set; }

        [StringLayoutDelimited(21, "yyyy-MM-dd")]
        public DateTime? DeliveryGroupEndDate { get; set; }

        [StringLayoutDelimited(22)]
        public Int64 StoreCount { get; set; }

        [StringLayoutDelimited(23)]
        public Int64 DeliveryGroupStoreCount { get; set; }

        /// <summary>
        /// Initializes a new instance of the StoreRange class.
        /// </summary>
        public StoreRange()
        {
            this.ID = 0L;
            this.Division = String.Empty;
            this.Store = String.Empty;
            this.Sku = String.Empty;
            this.Size = String.Empty;
            this.Min = String.Empty;
            this.Max = String.Empty;
            this.Days = String.Empty;
            this.InitialDemand = String.Empty;
            this.Range = String.Empty;
            this.StartDate = new DateTime?();
            this.EndDate = new DateTime?();
            this.CreatedBy = String.Empty;
            this.CreateDate = DateTime.MinValue;
            this.FirstReceiptDate = new DateTime?();
            this.DeliveryGroupName = String.Empty;
            this.Launch = new Boolean?();
            this.LaunchDate = new DateTime?();
            this.StoreCount = 0L;
            this.DeliveryGroupStoreCount = 0L;
            this.Description = String.Empty;
            this.PlanType = String.Empty;
            this.DeliveryGroupStartDate = new DateTime?();
            this.DeliveryGroupEndDate = new DateTime?();
        }

        /// <summary>
        /// Initializes a new instance of the StoreRange class.
        /// </summary>
        /// <param name="id">The initial value for the identifier property.</param>
        /// <param name="division">The initial value for the division property.</param>
        /// <param name="store">The initial value for the store property.</param>
        /// <param name="size">The initial value for the size property.</param>
        /// <param name="min">The initial value for the minimum property.</param>
        /// <param name="max">The initial value for the maximum property.</param>
        /// <param name="days">The initial value for the days property.</param>
        /// <param name="initialDemand">The initial value for the initial demand property.</param>
        /// <param name="range">The initial value for the range property.</param>
        /// <param name="startDate">The initial value for the start date property.</param>
        /// <param name="endDate">The initial value for the end date property.</param>
        /// <param name="createdBy">The initial value for the created by property.</param>
        /// <param name="createDate">The initial value for the create date property.</param>
        public StoreRange(Int64 id, string division, string store, string sku, string size, string min, string max, string days
                , string initialDemand, string range, DateTime? startDate, DateTime? endDate, string createdBy
                , DateTime createDate, DateTime? firstReceiptDate, string deliveryGroupName, Boolean? launch, DateTime? launchDate
                , Int64 storeCount, Int64 deliveryGroupStoreCount, string description, string planType
                , DateTime? deliveryGroupStartDate, DateTime? deliveryGroupEndDate)
            : this()
        {
            this.ID = id;
            this.Division = division;
            this.Store = store;
            this.Sku = sku;
            this.Size = size;
            this.Min = min;
            this.Max = max;
            this.Days = days;
            this.InitialDemand = initialDemand;
            this.Range = range;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.CreatedBy = createdBy;
            this.CreateDate = createDate;
            this.FirstReceiptDate = firstReceiptDate;
            this.DeliveryGroupName = deliveryGroupName;
            this.Launch = launch;
            this.LaunchDate = launchDate;
            this.StoreCount = storeCount;
            this.DeliveryGroupStoreCount = deliveryGroupStoreCount;
            this.Description = description;
            this.PlanType = planType;
            this.DeliveryGroupStartDate = deliveryGroupStartDate;
            this.DeliveryGroupEndDate = deliveryGroupEndDate;
        }

        /// <summary>
        /// Overrides the Footlocker.Common 'StringLayoutDelimitedUtility' ToString for performance gain
        /// </summary>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public override string ToString(char delimiter)
        {
            var resultBuilder = new StringBuilder();
            var delimiterString = delimiter.ToString();

            resultBuilder.Append(ID.ToString()).Append(delimiterString);
            resultBuilder.Append(Division).Append(delimiterString);
            resultBuilder.Append(Store).Append(delimiterString);
            resultBuilder.Append(Sku).Append(delimiterString);
            resultBuilder.Append(Size).Append(delimiterString);
            resultBuilder.Append(Min).Append(delimiterString);
            resultBuilder.Append(Max).Append(delimiterString);
            resultBuilder.Append(Days).Append(delimiterString);
            resultBuilder.Append(InitialDemand).Append(delimiterString);
            resultBuilder.Append(Range).Append(delimiterString);
            resultBuilder.Append(StartDate.HasValue ? Convert.ToDateTime(StartDate).ToString("M/d/yyyy h:mm:ss tt") : String.Empty).Append(delimiterString);
            resultBuilder.Append(EndDate.HasValue ? Convert.ToDateTime(EndDate).ToString("M/d/yyyy h:mm:ss tt") : String.Empty).Append(delimiterString);
            resultBuilder.Append(CreatedBy).Append(delimiterString);
            resultBuilder.Append(Convert.ToDateTime(CreateDate).ToString("M/d/yyyy h:mm:ss tt")).Append(delimiterString);
            resultBuilder.Append(FirstReceiptDate.HasValue ? Convert.ToDateTime(FirstReceiptDate).ToString("M/d/yyyy h:mm:ss tt") : String.Empty).Append(delimiterString);
            resultBuilder.Append(DeliveryGroupName).Append(delimiterString);
            resultBuilder.Append(Launch.ToString()).Append(delimiterString);
            resultBuilder.Append(LaunchDate.HasValue ? Convert.ToDateTime(LaunchDate).ToString("M/d/yyyy h:mm:ss tt") : String.Empty).Append(delimiterString);
            resultBuilder.Append(Description).Append(delimiterString);
            resultBuilder.Append(PlanType).Append(delimiterString);
            resultBuilder.Append(DeliveryGroupStartDate.HasValue ? Convert.ToDateTime(DeliveryGroupStartDate).ToString("M/d/yyyy h:mm:ss tt") : String.Empty).Append(delimiterString);
            resultBuilder.Append(DeliveryGroupEndDate.HasValue ? Convert.ToDateTime(DeliveryGroupEndDate).ToString("M/d/yyyy h:mm:ss tt") : String.Empty).Append(delimiterString);
            resultBuilder.Append(StoreCount.ToString()).Append(delimiterString);
            resultBuilder.Append(DeliveryGroupStoreCount.ToString()).Append(delimiterString);

            return resultBuilder.ToString();
        }
    }
}
