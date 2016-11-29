
using System;
using System.Collections.Generic;
using System.Text;
using Footlocker.Logistics.Allocation.Models;
using System.Data;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class RuleFactory
    {
        public Footlocker.Logistics.Allocation.Models.Rule Create(DataRow dr)
        {
            Footlocker.Logistics.Allocation.Models.Rule _newObject = new Footlocker.Logistics.Allocation.Models.Rule();
            _newObject.ID = Convert.ToInt64(dr["ID"]);
            _newObject.RuleSetID = Convert.ToInt64(dr["RuleSetID"]);
            _newObject.Compare = Convert.ToString(dr["Compare"]);
            _newObject.Field = Convert.ToString(dr["Field"]);
            _newObject.Sort = Convert.ToInt32(dr["Sort"]);
            _newObject.Value = Convert.ToString(dr["Value"]);

            return _newObject;
        }
    }
}