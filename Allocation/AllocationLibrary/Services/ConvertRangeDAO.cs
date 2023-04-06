
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

        public void SaveEcommRingFences(List<EcommRingFence> list, string user, bool accumulateQuantity = true)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.InsertEcommRingFences";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            SQLCommand = _database.GetStoredProcCommand(SQL);
            SQLCommand.CommandTimeout = 600;
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(list.GetType());
            xs.Serialize(sw, list);
            String xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@accumulateQuantity", DbType.Boolean, accumulateQuantity);
            _database.AddInParameter(SQLCommand, "@xmlDetails", DbType.Xml, xout);
            _database.AddInParameter(SQLCommand, "@user", DbType.String, user);

            _database.ExecuteNonQuery(SQLCommand);
        }

        public void PrepareEcommConversion(string division, string department)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.[PrepareEcommConversion]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            SQLCommand = _database.GetStoredProcCommand(SQL);
            SQLCommand.CommandTimeout = 600;

            _database.AddInParameter(SQLCommand, "@division", DbType.String, division);
            _database.AddInParameter(SQLCommand, "@department", DbType.String, department);

            _database.ExecuteNonQuery(SQLCommand);
        }
    }
}