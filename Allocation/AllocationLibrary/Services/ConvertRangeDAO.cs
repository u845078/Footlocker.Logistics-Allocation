
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
            //if (instanceid == 3)
            //{
            //    Database europe = DatabaseFactory.CreateDatabase("DB2EURP");


            //    Footlocker.Logistics.Allocation.Services.AllocationLibraryContext db = new Allocation.Services.AllocationLibraryContext();
            //    List<EcommWarehouse> ecommStores = (from a in db.EcommWarehouses where a.Store != "00800" select a).ToList();                                                                                           

            //    DbCommand SQLCommandPrep;
            //    string SQLPrep;
            //    SQLPrep = "dbo.[TruncateNormals]";

            //    SQLCommandPrep = _database.GetStoredProcCommand(SQLPrep);
            //    SQLCommandPrep.CommandTimeout = 600;
            //    _database.AddInParameter(SQLCommandPrep, "@instanceID", DbType.Int64, instanceid);

            //    _database.ExecuteNonQuery(SQLCommandPrep);


            //    DbCommand SQLCommand;
            //    string SQL;
            //    SQL = "dbo.[GetConvertRanges]";

            //    SQLCommand = _database.GetStoredProcCommand(SQL);
            //    _database.AddInParameter(SQLCommand, "@instanceID", DbType.Int64, instanceid);

            //    DataSet data = _database.ExecuteDataSet(SQLCommand);

            //    if (data.Tables[0].Rows.Count > 0)
            //    {
            //        foreach (DataRow dr in data.Tables[0].Rows)
            //        {
            //            string sql = " SELECT distinct RETL_OPER_DIV_CODE, STK_DEPT_NUM";
            //            sql = sql + ", STK_NUM, STK_WDTH_COLOR_NUM, STR_NUM";
            //            sql = sql + " FROM TCCRS001 ";
            //            sql = sql + " WHERE BS_NORMAL_QTY > 0";
            //            sql = sql + " and RETL_OPER_DIV_CODE = '" + Convert.ToString(dr["Division"]) + "'";
            //            sql = sql + " and STK_DEPT_NUM = '" + Convert.ToString(dr["Department"]) + "'";

            //            DbCommand MFSQLCommand;
            //            MFSQLCommand = europe.GetSqlStringCommand(sql);
            //            DataSet mfData = europe.ExecuteDataSet(MFSQLCommand);
            //            List<LegacyNormal> normals = new List<LegacyNormal>();
            //            foreach (DataRow dr2 in mfData.Tables[0].Rows)
            //            {
            //                LegacyNormal normal = new LegacyNormal();
            //                normal.InstanceID = instanceid;
            //                normal.Division = Convert.ToString(dr2["RETL_OPER_DIV_CODE"]);
            //                string sku = Convert.ToString(dr2["RETL_OPER_DIV_CODE"]) + "-" +
            //                    Convert.ToString(dr2["STK_DEPT_NUM"]) + "-" +
            //                    Convert.ToString(dr2["STK_NUM"]) + "-" +
            //                    Convert.ToString(dr2["STK_WDTH_COLOR_NUM"]);

            //                normal.Sku = sku;

            //                if (Convert.ToString(dr2["STR_NUM"]) == "00800")
            //                {
            //                    foreach (EcommWarehouse w in ecommStores)
            //                    {
            //                        normal.Store = w.Store;
            //                        normals.Add(normal);
            //                        normal = new LegacyNormal();
            //                        normal.InstanceID = instanceid;
            //                        normal.Division = Convert.ToString(dr2["RETL_OPER_DIV_CODE"]);
            //                        normal.Sku = sku;
            //                    }
            //                }
            //                else
            //                {
            //                    normal.Store = Convert.ToString(dr2["STR_NUM"]);

            //                    normals.Add(normal);
            //                }
            //            }
            //            if (normals.Count > 0)
            //            {
            //                DbCommand SQLCommandInsert;
            //                string SQLInsert;
            //                SQLInsert = "dbo.[InsertNormals]";

            //                SQLCommandInsert = _database.GetStoredProcCommand(SQLInsert);
            //                StringWriter sw = new StringWriter();
            //                XmlSerializer xs = new XmlSerializer(normals.GetType());
            //                xs.Serialize(sw, normals);
            //                String xout = sw.ToString();

            //                _database.AddInParameter(SQLCommandInsert, "@xmlDetails", DbType.Xml, xout);
            //                _database.AddInParameter(SQLCommandInsert, "@instanceID", DbType.Int64, instanceid);

            //                _database.ExecuteNonQuery(SQLCommandInsert);

            //            }
            //        }
            //    }
            //}

            DbCommand SQLCommandFinal;
            string SQLFinal;

            //if (instanceid == 3)
            //{
            //    SQLFinal = "dbo.[ConvertRangesFromNormals]";
            //}
            //else
            //{
            SQLFinal = "dbo.[ConvertRanges]";
            //}
            SQLCommandFinal = _database.GetStoredProcCommand(SQLFinal);
            SQLCommandFinal.CommandTimeout = 0;
            _database.AddInParameter(SQLCommandFinal, "@instanceID", DbType.Int64, instanceid);

            _database.ExecuteNonQuery(SQLCommandFinal);
        }


        public void SaveEcommRingFences(List<EcommRingFence> list, string user)
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