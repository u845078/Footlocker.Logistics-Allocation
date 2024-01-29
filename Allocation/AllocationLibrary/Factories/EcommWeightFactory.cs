using System;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class EcommWeightFactory
    {
        public EcommWeight Create(DataRow dr)
        {
            EcommWeight _newObject = new EcommWeight()
            {
                Division = Convert.ToString(dr["Division"]),
                Store = Convert.ToString(dr["Store"]),
                FOB = Convert.ToString(dr["FOB"]),
                Weight = Convert.ToDecimal(dr["Weight"])
            };

            return _newObject;
        }
    }
}
