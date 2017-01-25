
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;
using System.IO;
using System.Xml.Serialization;

namespace Footlocker.Logistics.Allocation.Services
{
    public class RangeFileItemDAO
    {
        Database _database;

        public RangeFileItemDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        
        public IDataReader GetRangeFileExtract(string div, string dept)
        {

            DbCommand SQLCommand;
            string SQL = "dbo.GetRangeExtract";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@dept", DbType.String, dept);
            SQLCommand.CommandTimeout = 0;

            return _database.ExecuteReader(SQLCommand);
        }

        public IDataReader GetRangeFileExtract(int instance)
        {

            DbCommand SQLCommand;
            string SQL = "dbo.GetRangeExtractInstance";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@instance", DbType.String, instance);
            SQLCommand.CommandTimeout = 0;

            return _database.ExecuteReader(SQLCommand);
        }

        public IDataReader GetRangeFileExtractDataReader(string div, string dept, string sku)
        {

            DbCommand SQLCommand;
            string SQL = "dbo.GetRangeExtract";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@dept", DbType.String, dept);
            _database.AddInParameter(SQLCommand, "@sku", DbType.String, sku);

            return _database.ExecuteReader(SQLCommand);
        }

        public List<RangeFileItem> GetRangeFileExtract(string div, string dept, string sku)
        {
            List<RangeFileItem> _que;
            _que = new List<RangeFileItem>();

            DbCommand SQLCommand;
            string SQL = "dbo.GetRangeExtract";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            SQLCommand.CommandTimeout = 6000;
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@dept", DbType.String, dept);
            _database.AddInParameter(SQLCommand, "@sku", DbType.String, sku);

            IDataReader datareader = _database.ExecuteReader(SQLCommand);

            RangeFileItemFactory factory = new RangeFileItemFactory();

            while (datareader.Read())
            {
                _que.Add(factory.Create(datareader));
            }
            return _que;
        }

        public IDataReader GetLegacyRangeFileExtract(int instance)
        {

            DbCommand SQLCommand;
            string SQL = "dbo.GetLegacyRangeExtract";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@instanceID", DbType.Int32, instance);
            SQLCommand.CommandTimeout = 0;

            return _database.ExecuteReader(SQLCommand);
        }        

        public void CreateRangeFromFile(string sku, string div, string store, string size, DateTime startdate, string plantype, string user)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.SaveRangeFileItem";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@sku", DbType.String, sku);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@store", DbType.String, store);
            _database.AddInParameter(SQLCommand, "@size", DbType.String, size);
            _database.AddInParameter(SQLCommand, "@start", DbType.DateTime, startdate);
            _database.AddInParameter(SQLCommand, "@plantype", DbType.String, plantype);
            _database.AddInParameter(SQLCommand, "@user", DbType.String, user);

            _database.ExecuteNonQuery(SQLCommand);
        }


        public void Save(List<RangeFileItem> list, Int32 instanceid)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.[SaveLegacyRange]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            SQLCommand.CommandTimeout = 300;
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(list.GetType());
            xs.Serialize(sw, list);
            String xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@xmlDetails", DbType.Xml, xout);
            _database.AddInParameter(SQLCommand, "@instanceID", DbType.Int32, instanceid);

            _database.ExecuteNonQuery(SQLCommand);
        }

        public void TruncateLegacyRange(Int32 instanceid)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.[TruncateLegacyRange]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            SQLCommand.CommandTimeout = 300;
            _database.AddInParameter(SQLCommand, "@instanceID", DbType.Int32, instanceid);

            _database.ExecuteNonQuery(SQLCommand);
        }

        public void BulkLoad(Int32 instanceid)
        {
            //using SQL server authentication, because bulk insert accross network doesn't work with integrated authentication
            Database bulkdatabase = Footlocker.Common.DatabaseService.GetSqlDatabase("AllocationBulk");
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.BulkLoadLegacyRange";

            SQLCommand = bulkdatabase.GetStoredProcCommand(SQL);
            bulkdatabase.AddInParameter(SQLCommand, "@instanceID", DbType.Int32, instanceid);
            //this is bulk loading millions of records, don't time out.
            SQLCommand.CommandTimeout = 0;
            bulkdatabase.ExecuteNonQuery(SQLCommand);
        }

        public void SetFirstReceiptDates(Int32 instanceid)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.[SetFirstReceiptDates]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@instanceID", DbType.Int32, instanceid);
            //this is bulk loading millions of records, don't time out.
            SQLCommand.CommandTimeout = 0;
            _database.ExecuteNonQuery(SQLCommand);
        }

        public void SetFirstReceiptDates(Int32 instanceid, string sku)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.[SetFirstReceiptDates]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@instanceID", DbType.Int32, instanceid);
            _database.AddInParameter(SQLCommand, "@sku", DbType.String, sku);

            _database.ExecuteNonQuery(SQLCommand);
        }
    }
}