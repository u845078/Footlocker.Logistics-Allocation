
using System;
using System.Collections.Generic;
using System.Text;
using Footlocker.Logistics.Allocation.Models;
using System.Data;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class SizeAllocationFactory
    {
        public Footlocker.Logistics.Allocation.Models.SizeAllocation Create(DataRow dr)
        {
            Footlocker.Logistics.Allocation.Models.SizeAllocation _newObject = new Footlocker.Logistics.Allocation.Models.SizeAllocation();
            _newObject.PlanID = Convert.ToInt64(dr["PlanID"]);
            _newObject.Division = Convert.ToString(dr["Division"]);
            _newObject.Store = Convert.ToString(dr["Store"]);
            _newObject.Size = Convert.ToString(dr["Size"]);
            _newObject.League = Convert.ToString(dr["League"]);

            if (!(Convert.IsDBNull(dr["Min"])))
            {
                _newObject.Min = Convert.ToInt32(dr["Min"]);
            }
            if (!(Convert.IsDBNull(dr["Max"])))
            {
                _newObject.Max = Convert.ToInt32(dr["Max"]);
            }
            if (!(Convert.IsDBNull(dr["Days"])))
            {
                _newObject.Days = Convert.ToInt32(dr["Days"]);
            }
            if (!(Convert.IsDBNull(dr["Range"])))
            {
                _newObject.Range = Convert.ToBoolean(dr["Range"]);
            }
            if (!(Convert.IsDBNull(dr["InitialDemand"])))
            {
                _newObject.InitialDemand = Convert.ToString(dr["InitialDemand"]);
            }
            if (!(Convert.IsDBNull(dr["StartDate"])))
            {
                _newObject.StartDate = Convert.ToDateTime(dr["StartDate"]);
            }
            if (!(Convert.IsDBNull(dr["EndDate"])))
            {
                _newObject.EndDate = Convert.ToDateTime(dr["EndDate"]);
            }
            if (!(Convert.IsDBNull(dr["MinEndDays"])))
            {
                _newObject.MinEndDays = Convert.ToInt32(dr["MinEndDays"]);
            }
            if (!(Convert.IsDBNull(dr["StoreLeadTime"])))
            {
                _newObject.StoreLeadTime = Convert.ToInt32(dr["StoreLeadTime"]);
            }
            if (!(Convert.IsDBNull(dr["DeliveryGroupStartDate"])))
            {
                _newObject.DeliveryGroupStartDate = Convert.ToDateTime(dr["DeliveryGroupStartDate"]);
            }
            if (!(Convert.IsDBNull(dr["DeliveryGroupMinEndDays"])))
            {
                _newObject.DeliveryGroupMinEndDays = Convert.ToInt32(dr["DeliveryGroupMinEndDays"]);
            }

            return _newObject;
        }
    }
}