namespace Footlocker.Logistics.Allocation.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Footlocker.Logistics.Allocation.Models;

    public class ConfigService
    {
        private readonly AllocationLibraryContext db;

        public ConfigService()
        {
            db = new AllocationLibraryContext();
        }
    
        //public bool IsTrue(int instanceid, string setting)
        //{
        //    Config config = (from a in db.Configs
        //                     join b in db.ConfigParams 
        //                     on a.ParamID equals b.ParamID 
        //                     where (a.InstanceID == instanceid) && 
        //                     (b.Name == setting) 
        //                     select a).FirstOrDefault();

        //    if (config == null)
        //    {
        //        throw new Exception(string.Format("Configuration setting {0} is not setup for instance {1}", setting, instanceid.ToString()));
        //    }

        //    return config.Value.ToLower() == "true";
        //}

        public string GetValue(int instanceid, string setting)
        {
            Config config = (from a in db.Configs
                             join b in db.ConfigParams 
                             on a.ParamID equals b.ParamID
                             where (a.InstanceID == instanceid) && 
                             (b.Name == setting)
                             select a).FirstOrDefault();

            if (config == null)
            {
                throw new Exception(string.Format("Configuration setting {0} is not setup for instance {1}", setting, instanceid.ToString()));
            }
            return config.Value;
        }

        //public int GetIntValue(int instanceid, string setting)
        //{
        //    Config config = (from a in db.Configs
        //                     join b in db.ConfigParams on a.ParamID equals b.ParamID
        //                     where (a.InstanceID == instanceid) && (b.Name == setting)
        //                     select a).FirstOrDefault();

        //    if (config == null)
        //    {
        //        throw new Exception(string.Format("Configuration setting {0} is not setup for instance {1}", setting, instanceid.ToString()));
        //    }
        //    return Convert.ToInt32(config.Value);
        //}

        public int GetInstance(string division)
        {
            int instance = (from a in db.InstanceDivisions
                            where a.Division == division
                            select a.InstanceID).First();

            return instance;
        }

        public DateTime GetControlDate(int instance)
        {
            DateTime controlDate = (from cd in db.ControlDates
                                    where cd.InstanceID == instance
                                    select cd.RunDate).FirstOrDefault();
            return controlDate;
        }

        public DateTime GetControlDate(string division)
        {
            int instanceID = GetInstance(division);
            return GetControlDate(instanceID);
        }

        public string GetCPID(string sku)
        {
            string cpidString;

            cpidString =  db.CPSkuSizesXrefs.Where(cp => cp.LegacySku == sku)
                                            .Select(cp => cp.CPID)
                                            .FirstOrDefault();

            if (cpidString == null)
                cpidString = "";

            return cpidString;
        }

        public string GetDivisionalCurrencyCode(string division)
        {
            string flCountryCode;
            string currencyCode;

            flCountryCode = db.AllocationDivisions.Where(ad => ad.DivisionCode == division).Select(ad => ad.DefaultCountryCode).FirstOrDefault();
            currencyCode = db.FLCountryCodes.Where(cc => cc.CountryCode == flCountryCode).Select(cc => cc.ISOCurrencyCode).FirstOrDefault();

            return currencyCode;
        }
    }
}
