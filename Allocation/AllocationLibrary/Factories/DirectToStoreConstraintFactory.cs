using System;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class DirectToStoreConstraintFactory
    {
        public DirectToStoreConstraint Create(DataRow dr)
        {
            DirectToStoreConstraint _newObject = new DirectToStoreConstraint()
            {
                ItemID = Convert.ToInt64(dr["ItemID"]),
                MaxQty = Convert.ToInt32(dr["MaxQty"]),
                Size = Convert.ToString(dr["Size"]),
                Sku = Convert.ToString(dr["Sku"]),
                StartDate = Convert.ToDateTime(dr["StartDate"]),
                CreateDate = Convert.ToDateTime(dr["CreateDate"]),
                CreatedBy = Convert.ToString(dr["CreatedBy"])
            };

            if (!Convert.IsDBNull(dr["EndDate"]))            
                _newObject.EndDate = Convert.ToDateTime(dr["EndDate"]);
            
            if (!Convert.IsDBNull(dr["Description"]))            
                _newObject.Description = Convert.ToString(dr["Description"]);            

            return _newObject;
        }
    }
}