using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    /// <summary>
    /// Provides an object representation of a range plan factory.
    /// </summary>
    public class RangePlanFactory : IBiExtractFactory<RangePlan>
    {
        public RangePlan Create(IDataReader reader)
        {
            Int64 id = Convert.ToInt64(reader["Id"]);
            string description = Convert.ToString(reader["PlanDescription"]);
            string planType = Convert.ToString(reader["PlanType"]);
            DateTime createDate = Convert.ToDateTime(reader["CreateDTTM"]);
            string createdBy = Convert.ToString(reader["CreatedBy"]);
            DateTime? updateDate
                = Convert.IsDBNull(reader["UpdateDTTM"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["UpdateDTTM"]));
            string updatedBy = Convert.ToString(reader["UpdatedBy"]);
            string sku = Convert.ToString(reader["Sku"]);
            DateTime? startDate
                = Convert.IsDBNull(reader["StartDate"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["StartDate"]));
            DateTime? endDate
                = Convert.IsDBNull(reader["EndDate"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["EndDate"]));

            return new RangePlan(id, sku, description, startDate, endDate, planType, updatedBy, updateDate, createdBy
                , createDate, String.Empty, String.Empty, new Int64?(), 0, null);
        }
    }
}
