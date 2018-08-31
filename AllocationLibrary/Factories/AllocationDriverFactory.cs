
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class AllocationDriverFactory
    {
        public AllocationDriver Create(DataRow dr)
        {
            AllocationDriver _newObject = new AllocationDriver();
            _newObject.Division = Convert.ToString(dr["Division"]);
            _newObject.Department = Convert.ToString(dr["Department"]);
            if (!(Convert.IsDBNull(dr["AllocateDate"])))
            {
                _newObject.AllocateDate = Convert.ToDateTime(dr["AllocateDate"]);
            }
            if (!(Convert.IsDBNull(dr["ConvertDate"])))
            {
                _newObject.ConvertDate = Convert.ToDateTime(dr["ConvertDate"]);
            }
            if (!(Convert.IsDBNull(dr["OrderPlanningDate"])))
            {
                _newObject.OrderPlanningDate = Convert.ToDateTime(dr["OrderPlanningDate"]);
            }
            _newObject.CreatedBy = Convert.ToString(dr["CreatedBy"]);
            _newObject.CreateDate = Convert.ToDateTime(dr["CreateDate"]);
            if (!(Convert.IsDBNull(dr["CheckNormals"])))
            {
                _newObject.CheckNormals = Convert.ToBoolean(dr["CheckNormals"]);
            }
            _newObject.StockedInMinihub = Convert.ToBoolean(dr["MinihubInd"]);
            return _newObject;
        }
    }
}