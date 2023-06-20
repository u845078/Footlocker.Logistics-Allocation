using System;
using System.Collections.Generic;
using System.Text;
using Footlocker.Logistics.Allocation.Models;
using System.Data;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class SizeAllocationFactory
    {
        public SizeAllocation Create(DataRow dr)
        {
            SizeAllocation _newObject = new SizeAllocation()
            {
                PlanID = Convert.ToInt64(dr["PlanID"]),
                Division = Convert.ToString(dr["Division"]),
                Store = Convert.ToString(dr["Store"]),
                Size = Convert.ToString(dr["Size"]),
                League = Convert.ToString(dr["League"])
            };

            if (!Convert.IsDBNull(dr["Min"]))            
                _newObject.Min = Convert.ToInt32(dr["Min"]);
            
            if (!Convert.IsDBNull(dr["Max"]))
                _newObject.Max = Convert.ToInt32(dr["Max"]);

            if (!Convert.IsDBNull(dr["Days"]))
                _newObject.Days = Convert.ToInt32(dr["Days"]);

            if (!Convert.IsDBNull(dr["Range"]))
                _newObject.Range = Convert.ToBoolean(dr["Range"]);

            if (!Convert.IsDBNull(dr["InitialDemand"]))
                _newObject.InitialDemand = Convert.ToDecimal(dr["InitialDemand"]);

            if (!Convert.IsDBNull(dr["StartDate"]))
                _newObject.StartDate = Convert.ToDateTime(dr["StartDate"]);

            if (!Convert.IsDBNull(dr["EndDate"]))
                _newObject.EndDate = Convert.ToDateTime(dr["EndDate"]);

            if (!Convert.IsDBNull(dr["MinEndDays"]))
                _newObject.MinEndDays = Convert.ToInt32(dr["MinEndDays"]);

            if (!Convert.IsDBNull(dr["StoreLeadTime"]))
                _newObject.StoreLeadTime = Convert.ToInt32(dr["StoreLeadTime"]);

            if (!Convert.IsDBNull(dr["DeliveryGroupStartDate"]))
                _newObject.DeliveryGroupStartDate = Convert.ToDateTime(dr["DeliveryGroupStartDate"]);

            if (!Convert.IsDBNull(dr["DeliveryGroupMinEndDays"]))
                _newObject.DeliveryGroupMinEndDays = Convert.ToInt32(dr["DeliveryGroupMinEndDays"]);
                       
            _newObject.Fringe = Convert.ToInt32(dr["Fringe"]) == 1;

            return _newObject;
        }
    }
}