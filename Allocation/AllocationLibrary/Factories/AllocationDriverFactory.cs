using System;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class AllocationDriverFactory
    {
        public AllocationDriver Create(DataRow dr)
        {
            AllocationDriver _newObject = new AllocationDriver()
            {
                Division = Convert.ToString(dr["Division"]),
                Department = Convert.ToString(dr["Department"]),
                CreatedBy = Convert.ToString(dr["CreatedBy"]),
                CreateDate = Convert.ToDateTime(dr["CreateDate"]),
                StockedInMinihub = Convert.ToBoolean(dr["MinihubInd"])
            };

            if (!Convert.IsDBNull(dr["AllocateDate"]))            
                _newObject.AllocateDate = Convert.ToDateTime(dr["AllocateDate"]);
            
            if (!Convert.IsDBNull(dr["ConvertDate"]))            
                _newObject.ConvertDate = Convert.ToDateTime(dr["ConvertDate"]);
            
            if (!Convert.IsDBNull(dr["OrderPlanningDate"]))            
                _newObject.OrderPlanningDate = Convert.ToDateTime(dr["OrderPlanningDate"]);
            
            if (!Convert.IsDBNull(dr["CheckNormals"]))            
                _newObject.CheckNormals = Convert.ToBoolean(dr["CheckNormals"]);            
            
            return _newObject;
        }
    }
}