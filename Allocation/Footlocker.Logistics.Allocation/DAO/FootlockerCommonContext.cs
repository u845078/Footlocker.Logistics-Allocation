using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Footlocker.Logistics.Allocation.DAO
{
    public class FootlockerCommonContext : DbContext
    {
        public FootlockerCommonContext() : base("name=SQLDB.Footlocker_Common")
        {
        }
    }
}