using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;

namespace Footlocker.Logistics.Allocation.Services
{
    public class RangePlanDAO
    {
        Database _database;

        public RangePlanDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        public void DeleteRangePlan(long planID)
        {
            DbCommand SQLCommand;
            string SQL = "DeleteRangePlanData";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@planID", DbType.Int64, planID);
            _database.ExecuteNonQuery(SQLCommand);
        }
    }
}
