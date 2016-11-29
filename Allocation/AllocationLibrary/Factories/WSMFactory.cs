using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class WSMFactory
    {
        public WSM Create(DataRow dr)
        {
            WSM newObject = new WSM();
            newObject.RunDate = Convert.ToString(dr["RunDate"]);
            newObject.TargetProduct = Convert.ToString(dr["TargetProduct"]);
            newObject.TargetProductId = Convert.ToString(dr["TargetProduct_id"]);
            newObject.MatchProduct = Convert.ToString(dr["MatchProduct"]);
            newObject.MatchProductId = Convert.ToString(dr["MatchProduct_id"]);
            newObject.ProductWeight = Convert.ToString(dr["ProductWeight"]);
            newObject.MatchLocation = Convert.ToString(dr["MatchLocation"]);
            newObject.LocationWeight = Convert.ToString(dr["LocationWeight"]);
            newObject.FinalMatchWeight = Convert.ToString(dr["FinalMatchWeight"]);
            newObject.FinalMatchDemand = Convert.ToString(dr["FinalMatchDemand"]);
            newObject.LastCapturedDemand = Convert.ToString(dr["LastCapturedDemand"]);
            newObject.StatusCode = Convert.ToString(dr["Status_cd"]);

            return newObject;
        }
    }
}
