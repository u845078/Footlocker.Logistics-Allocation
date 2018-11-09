
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class EcommWeightFactory
    {
        public EcommWeight Create(DataRow dr)
        {
            EcommWeight _newObject = new EcommWeight();
            _newObject.Division = Convert.ToString(dr["Division"]);
            _newObject.Store = Convert.ToString(dr["Store"]);
            _newObject.FOB = Convert.ToString(dr["FOB"]);
            _newObject.Weight = Convert.ToDecimal(dr["Weight"]);

            return _newObject;
        }
    }
}
