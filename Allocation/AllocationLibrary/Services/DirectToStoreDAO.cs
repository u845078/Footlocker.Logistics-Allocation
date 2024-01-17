using System;
using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;
using System.Data.Common;
using System.Data;
using System.IO;
using System.Xml.Serialization;

namespace Footlocker.Logistics.Allocation.Services
{
    public class DirectToStoreDAO
    {
        readonly Database _database;
        readonly Database _databaseEurope;
        readonly Database _databaseAllocation;
        readonly string europeDivisions;

        public DirectToStoreDAO(string europeDivisions)
        {
            _database = DatabaseFactory.CreateDatabase("DB2PROD");
            _databaseEurope = DatabaseFactory.CreateDatabase("DB2EURP");
            _databaseAllocation = DatabaseFactory.CreateDatabase("AllocationContext");
            this.europeDivisions = europeDivisions;
        }

        public bool IsVendorValidForSku(string vendor, string sku)
        {
            Database currentDB;            
            DbCommand SQLCommand;
            string[] tokens = sku.Split('-');

            if (europeDivisions.Contains(tokens[0]))            
                currentDB = _databaseEurope;            
            else            
                currentDB = _database;            

            string SQL = "select count(*) as CNT from TC051026 ";
            SQL += " where VEND_NUM = ? and ";
            SQL += " RETL_OPER_DIV_CD = ? and ";
            SQL += " STK_DEPT_NUM = ? and ";
            SQL += " STK_NUM = ?";

            SQLCommand = currentDB.GetSqlStringCommand(SQL);
            currentDB.AddInParameter(SQLCommand, "@1", DbType.String, vendor.PadLeft(5, '0'));
            currentDB.AddInParameter(SQLCommand, "@2", DbType.String, tokens[0]);
            currentDB.AddInParameter(SQLCommand, "@3", DbType.String, tokens[1]);
            currentDB.AddInParameter(SQLCommand, "@4", DbType.String, tokens[2]);

            DataSet data;
            data = currentDB.ExecuteDataSet(SQLCommand);

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    if (Convert.ToInt32(dr["CNT"]) >= 1)                    
                        return true;                    
                }
            }
            return false;
        }

        public List<string> GetVendors(string sku)
        {
            Database currentDB;
            List<string> _que = new List<string>();

            DbCommand SQLCommand;
            string[] tokens = sku.Split('-');

            if (europeDivisions.Contains(tokens[0]))            
                currentDB = _databaseEurope;            
            else            
                currentDB = _database;            

            string SQL = "select DISTINCT VEND_NUM from TC051026 ";
            SQL += " where RETL_OPER_DIV_CD = ? and ";
            SQL += " STK_DEPT_NUM = ? and ";
            SQL += " STK_NUM = ?";

            SQLCommand = currentDB.GetSqlStringCommand(SQL);            
            currentDB.AddInParameter(SQLCommand, "@1", DbType.String, tokens[0]);
            currentDB.AddInParameter(SQLCommand, "@2", DbType.String, tokens[1]);
            currentDB.AddInParameter(SQLCommand, "@3", DbType.String, tokens[2]);

            DataSet data;
            data = currentDB.ExecuteDataSet(SQLCommand);

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(Convert.ToString(dr["VEND_NUM"]));
                }
            }
            return _que;
        }
        
        public List<DirectToStoreConstraint> GetDTSConstraintsOneSize()
        {
            List<DirectToStoreConstraint> _que;
            _que = new List<DirectToStoreConstraint>();

            DbCommand SQLCommand;
            string SQL = "dbo.[getDTSConstraintsOneSize]";

            SQLCommand = _databaseAllocation.GetStoredProcCommand(SQL);

            DataSet data;
            data = _databaseAllocation.ExecuteDataSet(SQLCommand);

            DirectToStoreConstraintFactory factory = new DirectToStoreConstraintFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    var constraint = factory.Create(dr);

                    constraint.VendorNumber = dr["VendorID"].ToString();
                    constraint.VendorDesc = dr["VendorDesc"].ToString();
                    constraint.VendorPackQty = Convert.ToInt32(dr["VendorPackQty"]);
                    constraint.OrderDays = dr["OrderDays"].ToString();
                    
                    _que.Add(constraint);
                }
            }
            return _que;
        }

        public void SaveARSkusUpload(List<DirectToStoreSku> list)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.[SaveARSkusUpload]";

            SQLCommand = _databaseAllocation.GetStoredProcCommand(SQL);
            SQLCommand.CommandTimeout = 300;
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(list.GetType());
            xs.Serialize(sw, list);
            string xout = sw.ToString();

            _databaseAllocation.AddInParameter(SQLCommand, "@xml", DbType.Xml, xout);
            _databaseAllocation.ExecuteNonQuery(SQLCommand);
        }

        public void SaveARConstraintsUpload(List<DirectToStoreConstraint> list)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.[SaveARConstraintsUpload]";

            SQLCommand = _databaseAllocation.GetStoredProcCommand(SQL);
            SQLCommand.CommandTimeout = 300;
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(list.GetType());
            xs.Serialize(sw, list);
            string xout = sw.ToString();

            _databaseAllocation.AddInParameter(SQLCommand, "@xml", DbType.Xml, xout);
            _databaseAllocation.ExecuteNonQuery(SQLCommand);
        }
    }
}