using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    /// <summary>
    /// Provides an object representation of a store range factory.
    /// </summary>
    public class StoreRangeFactory : IBiExtractFactory<StoreRange>
    {
        /// <summary>
        /// Create a new store range.
        /// </summary>
        /// <param name="reader">The data reader containing the store range's properties.</param>
        /// <returns>The new store range.</returns>
        public StoreRange Create(IDataReader reader)
        {
            Int64 id = Convert.ToInt64(reader["ID"]);
            string division = Convert.ToString(reader["Division"]);
            string store = Convert.ToString(reader["Store"]);
            string sku = Convert.ToString(reader["Sku"]);
            string createdBy = Convert.ToString(reader["CreatedBy"]);
            DateTime createDate = Convert.ToDateTime(reader["CreateDTTM"]);
            DateTime? startDate
                = Convert.IsDBNull(reader["StartDate"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["StartDate"]));
            DateTime? endDate
                = Convert.IsDBNull(reader["EndDate"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["EndDate"]));
            string size = Convert.ToString(reader["Size"]);
            string min = Convert.ToString(reader["Min"]);
            string max = Convert.ToString(reader["Max"]);
            string days = Convert.ToString(reader["Days"]);
            string range = Convert.ToString(reader["Range"]);
            string initialDemand = Convert.ToString(reader["InitialDemand"]);
            DateTime? firstReceiptDate
                = Convert.IsDBNull(reader["FirstReceiptDate"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["FirstReceiptDate"]));
            string deliveryGroupName = Convert.ToString(reader["DeliveryGroupName"]);
            Boolean? launch
                = Convert.IsDBNull(reader["Launch"]) ? new Boolean?()
                    : new Boolean?(Convert.ToBoolean(reader["Launch"]));
            DateTime? launchDate
                = Convert.IsDBNull(reader["LaunchDate"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["LaunchDate"]));
            Int64 storeCount = Convert.ToInt64(reader["RangePlanStoreCount"]);
            Int64 deliveryGroupStoreCount = Convert.ToInt64(reader["DeliveryGroupStoreCount"]);
            string description = Convert.ToString(reader["PlanDescription"]);
            string planType = Convert.ToString(reader["PlanType"]);
            DateTime? deliveryGroupStartDate
                = Convert.IsDBNull(reader["DeliveryGroupStartDate"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["DeliveryGroupStartDate"]));
            DateTime? deliveryGroupEndDate
                = Convert.IsDBNull(reader["DeliveryGroupEndDate"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["DeliveryGroupEndDate"]));

            return new StoreRange(id, division, store, sku, size, min, max, days, initialDemand, range, startDate, endDate
                , createdBy, createDate, firstReceiptDate, deliveryGroupName, launch, launchDate, storeCount
                , deliveryGroupStoreCount, description, planType, deliveryGroupStartDate, deliveryGroupEndDate);
        }
    }
}
