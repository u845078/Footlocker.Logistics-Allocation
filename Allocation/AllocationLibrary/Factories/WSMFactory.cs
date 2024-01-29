using System;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class WSMFactory
    {
        public WSM Create(DataRow dr)
        {
            WSM newObject = new WSM()
            {
                RunDate = Convert.ToString(dr["RunDate"]),
                TargetProduct = Convert.ToString(dr["TargetProduct"]),
                TargetProductId = Convert.ToString(dr["TargetProduct_id"]),
                TargetLocation = Convert.ToString(dr["TargetLocation"]),
                MatchProduct = Convert.ToString(dr["MatchProduct"]),
                MatchProductId = Convert.ToString(dr["MatchProduct_id"]),
                ProductWeight = Convert.ToString(dr["ProductWeight"]),
                MatchLocation = Convert.ToString(dr["MatchLocation"]),
                LocationWeight = Convert.ToString(dr["LocationWeight"]),
                FinalMatchWeight = Convert.ToString(dr["FinalMatchWeight"]),
                FinalMatchDemand = Convert.ToString(dr["FinalMatchDemand"]),
                LastCapturedDemand = Convert.ToString(dr["LastCapturedDemand"]),
                StatusCode = Convert.ToString(dr["Status_cd"])
            };

            return newObject;
        }
    }
}
