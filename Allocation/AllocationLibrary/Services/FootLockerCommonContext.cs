using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Footlocker.Logistics.Allocation.Models;
using System.Data.Entity.Infrastructure;
using System.Data.Common;
using System.Data.Objects;
using Footlocker.Logistics.Allocation.Models.Services;

namespace Footlocker.Logistics.Allocation.Services
{
    public class FootLockerCommonContext : DbContext
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public FootLockerCommonContext() : base("SQLDB.Footlocker_Common")
        {
            // Get the ObjectContext related to this DbContext
            var objectContext = (this as IObjectContextAdapter).ObjectContext;

            // Sets the command timeout for all the commands
            objectContext.CommandTimeout = 300;
        }
    }
}
