using System;
using System.Data;

namespace Footlocker.Logistics.Allocation.Models.Factories
{
    public class FamilyOfBusinessFactory
    {
        public FamilyOfBusiness Create(DataRow dr)
        {
            FamilyOfBusiness _newObject = new FamilyOfBusiness()
            {
                Code = Convert.ToString(dr["DeptGroup"]),
                Description = Convert.ToString(dr["Description"]),
                Division = Convert.ToString(dr["division"])
            };

            return _newObject;
        }
    }
}