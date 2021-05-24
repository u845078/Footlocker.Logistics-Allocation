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

        public static EcomCustomerFulfillmentXref CreateUpdatedDBRec(EcomCustFulfillmentXref validationData, EcomCustomerFulfillmentXref dbRec, string currUser)
        {
            EcomCustomerFulfillmentXref updatedRec = dbRec;

            updatedRec.PostalCode = validationData.PostalCode;
            updatedRec.StateCode = validationData.StateCode;
            updatedRec.CountryCode = validationData.CountryCode;
            updatedRec.FulfillmentCenterID = validationData.FulfillmentCenterID;
            updatedRec.Division = validationData.Division;
            updatedRec.Store = validationData.Store;
            updatedRec.EffectiveFromDate = validationData.EffectiveFromDate;
            updatedRec.EffectiveToDate = validationData.EffectiveToDate;
            updatedRec.LastModifiedDate = DateTime.Now;
            updatedRec.LastModifiedUser = currUser;

            return updatedRec;
        }

        public static EcomCustFulfillmentXref CreateValidationRec(EcomCustomerFulfillmentXref dbRec)
        {
            EcomCustFulfillmentXref outRec = new EcomCustFulfillmentXref
            {
                PostalCode = dbRec.PostalCode,
                StateCode = dbRec.StateCode,
                CountryCode = dbRec.CountryCode,
                FulfillmentCenterID = dbRec.FulfillmentCenterID,
                Division = dbRec.Division,
                Store = dbRec.Store,
                EffectiveFromDate = dbRec.EffectiveFromDate,
                EffectiveToDate = dbRec.EffectiveToDate
            };

            return outRec;
        }
    }
}
