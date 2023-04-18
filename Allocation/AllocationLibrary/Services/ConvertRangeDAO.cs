using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Services
{
    public class ConvertRangeDAO
    {
        Database _database;

        public ConvertRangeDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        public void ConvertRanges(int instanceid)
        {
            DbCommand SQLCommandFinal;
            string SQLFinal;

            SQLFinal = "dbo.[ConvertRanges]";
            SQLCommandFinal = _database.GetStoredProcCommand(SQLFinal);
            SQLCommandFinal.CommandTimeout = 0;
            _database.AddInParameter(SQLCommandFinal, "@instanceID", DbType.Int64, instanceid);

            _database.ExecuteNonQuery(SQLCommandFinal);
        }

        public void PrepareEcommConversion(string division, string department)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.[PrepareEcommConversion]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            SQLCommand.CommandTimeout = 600;

            _database.AddInParameter(SQLCommand, "@division", DbType.String, division);
            _database.AddInParameter(SQLCommand, "@department", DbType.String, department);

            _database.ExecuteNonQuery(SQLCommand);
        }
    }
}