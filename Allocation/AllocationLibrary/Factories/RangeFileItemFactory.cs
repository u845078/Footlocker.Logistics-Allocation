using System;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class RangeFileItemFactory
    {
        public RangeFileItem Create(IDataReader dr)
        {
            RangeFileItem _newObject = new RangeFileItem
            {
                ProductIdent = Convert.ToString(dr["ProductIdent"]),
                LocationTypeCode = Convert.ToString(dr["LocationTypeCode"]),
                LocationID = Convert.ToString(dr["LocationID"]),
                OnRangeDt = Convert.ToString(dr["OnRangeDt"]),
                OffRangeDt = Convert.ToString(dr["OffRangeDt1"]),
                TodayUnitCost = Convert.ToString(dr["TodayUnitCost"]),
                TodayUnitRetail = Convert.ToString(dr["TodayUnitRetail"]),
                NonsellableQty = Convert.ToString(dr["Min"]),
                MaxStockQty = Convert.ToString(dr["Max"]),
                InitWklyDemand = Convert.ToString(dr["InitialDemand"]),
                Ranged = (dr["Range"] as int? == 1),
                Attribute1 = Convert.ToString(dr["Attribute1"]),
                Attribute2 = Convert.ToString(dr["Attribute2"]),
                Attribute3 = "0",
                Attribute4 = "0",
                Attribute5 = "0",
                Attribute6 = "0",
                Attribute7 = "0",
                Attribute8 = "0"
            };

            if (!Convert.IsDBNull(dr["Attribute_9"]))            
                _newObject.Attribute9 = Convert.ToString(dr["Attribute_9"]);            

            if (!Convert.IsDBNull(dr["Launch"]))
            {
                if (Convert.ToInt16(dr["Launch"]) == 1)                
                    _newObject.Attribute10 = "LAUNCH";                
                else                
                    _newObject.Attribute10 = "REGULAR";                
            }
            else            
                _newObject.Attribute10 = "REGULAR";            

            if (!Convert.IsDBNull(dr["FirstReceivableDt"]))            
                _newObject.FirstReceivableDt = Convert.ToString(dr["FirstReceivableDt"]);            

            if (!Convert.IsDBNull(dr["Attribute_15"]))            
                _newObject.Attribute15 = Convert.ToString(dr["Attribute_15"]);            

            if (!Convert.IsDBNull(dr["MinEndDate"]))            
                _newObject.MinEndDate = Convert.ToString(dr["MinEndDate"]);            

            if (!Convert.IsDBNull(dr["LearningTransitionCode"]))            
                _newObject.LearningTransitionCode = Convert.ToString(dr["LearningTransitionCode"]).Trim();            

            if (!Convert.IsDBNull(dr["MLD"]))            
                _newObject.MldInd = Convert.ToString(dr["MLD"]).Trim();            
            else
            {
                if (_newObject.Ranged)                
                    _newObject.MldInd = "Y";                
                else                
                    _newObject.MldInd = "N";                
            }

            return _newObject;
        }
    }
}
