
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Models.Factories
{
    public class FamilyOfBusinessFactory
    {
        public FamilyOfBusiness Create(DataRow dr)
        {
            FamilyOfBusiness _newObject = new FamilyOfBusiness();
            _newObject.Code = Convert.ToString(dr["DeptGroup"]);
            _newObject.Description = Convert.ToString(dr["Description"]);
            _newObject.Division = Convert.ToString(dr["division"]);

            return _newObject;
        }
    }
}