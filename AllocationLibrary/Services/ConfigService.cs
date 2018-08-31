// -----------------------------------------------------------------------
// <copyright file="ConfigService.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Footlocker.Logistics.Allocation.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Footlocker.Logistics.Allocation.Models;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ConfigService
    {
        AllocationLibraryContext db;

        public ConfigService()
        {
            db = new AllocationLibraryContext();
        }
    
        public Boolean IsTrue(int instanceid, string setting)
        {
            Config config = (from a in db.Configs
                        join b in db.ConfigParams on a.ParamID equals b.ParamID
                        where ((a.InstanceID == instanceid) && (b.Name == setting))
                        select a).FirstOrDefault();

            if (config == null)
            {
                throw new Exception("Configuration setting " + setting + " is not setup for instance " + instanceid);
            }
            return config.Value.ToLower() == "true";
        }

        public string GetValue(int instanceid, string setting)
        {
            Config config = (from a in db.Configs
                             join b in db.ConfigParams on a.ParamID equals b.ParamID
                             where ((a.InstanceID == instanceid) && (b.Name == setting))
                             select a).FirstOrDefault();

            if (config == null)
            {
                throw new Exception("Configuration setting " + setting + " is not setup for instance " + instanceid);
            }
            return config.Value;
        }

        public int GetIntValue(int instanceid, string setting)
        {
            Config config = (from a in db.Configs
                             join b in db.ConfigParams on a.ParamID equals b.ParamID
                             where ((a.InstanceID == instanceid) && (b.Name == setting))
                             select a).FirstOrDefault();

            if (config == null)
            {
                throw new Exception("Configuration setting " + setting + " is not setup for instance " + instanceid);
            }
            return Convert.ToInt32(config.Value);
        }
    }
}
