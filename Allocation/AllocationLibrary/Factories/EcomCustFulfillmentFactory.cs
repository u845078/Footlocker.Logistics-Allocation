using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Validation;

namespace Footlocker.Logistics.Allocation.Factories
{
    public static class EcomCustFulfillmentFactory
    {
        public static EcomCustomerFulfillmentXref CreateDBRec(EcomCustFulfillmentXref validationData, string currUser)
        {
            EcomCustomerFulfillmentXref outRec = new EcomCustomerFulfillmentXref
            {
                PostalCode = validationData.PostalCode, 
                StateCode = validationData.StateCode, 
                CountryCode = validationData.CountryCode, 
                FulfillmentCenterID = validationData.FulfillmentCenterID, 
                Division = validationData.Division,
                Store = validationData.Store,
                EffectiveFromDate = validationData.EffectiveFromDate, 
                EffectiveToDate = validationData.EffectiveToDate,
                LastModifiedDate = DateTime.Now,
                LastModifiedUser = currUser
            };

            return outRec;
        }
    }
}
