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
            newObject.TargetLocation = Convert.ToString(dr["TargetLocation"]);
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

        public QuantumSeasonalityData CreateSeasonalData(DataRow dr)
        {
            QuantumSeasonalityData newObject = new QuantumSeasonalityData();
            newObject.locationFinalNodeID = Convert.ToString(dr["location_final_node_id"]);
            newObject.weekBeginDate = Convert.ToDateTime(dr["week_begin_dt"]);
            newObject.indexValue = Convert.ToSingle(dr["index_value"]);

            return newObject;
        }
    }
}
