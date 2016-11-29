
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;
using Footlocker.Logistics.Allocation.Models.Services;
using System.Linq;
using System.IO;
using System.Xml.Serialization;

namespace Footlocker.Logistics.Allocation.Services
{
    public class RDQDAO
    {
        Database _database;

        public RDQDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }


        public List<RDQ> GetRDQsForHolds(string div, string level, string value)
        {
            List<RDQ> _que;
            _que = new List<RDQ>();

            DbCommand SQLCommand;
            string SQL = "dbo.[getRDQsForHolds]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@level", DbType.String, level);
            _database.AddInParameter(SQLCommand, "@value", DbType.String, value);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            RDQFactory factory = new RDQFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }

        public List<RDQ> GetRDQsForHolds(string div, string store)
        {
            List<RDQ> _que;
            _que = new List<RDQ>();

            DbCommand SQLCommand;
            string SQL = "dbo.[getRDQsForHoldsStore]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@store", DbType.String, store);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            RDQFactory factory = new RDQFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }


        public List<RDQ> GetRDQsForHold(long holdID)
        {
            List<RDQ> _que;
            _que = new List<RDQ>();

            DbCommand SQLCommand;
            string SQL = "dbo.[getRDQsForHold]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@ID", DbType.String, holdID);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            RDQFactory factory = new RDQFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }

        /// <summary>
        /// Gets RDQs that are for this hold only, not including ones that are also on hold for another reason
        /// </summary>
        /// <param name="holdID"></param>
        /// <returns></returns>
        public List<RDQ> GetUniqueRDQsForHold(long holdID)
        {
            List<RDQ> _que;
            _que = new List<RDQ>();

            DbCommand SQLCommand;
            string SQL = "dbo.[getUniqueRDQsForHold]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@ID", DbType.String, holdID);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            RDQFactory factory = new RDQFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }

        /// <summary>
        /// Gets RDQs that are for this hold only, not including ones that are also on hold for another reason
        /// </summary>
        /// <param name="holds"></param>
        /// <returns></returns>
        public List<RDQ> GetUniqueRDQsForHold(List<Hold> holds)
        {
            List<RDQ> _que;
            _que = new List<RDQ>();

            DbCommand SQLCommand;
            string SQL = "dbo.[getUniqueRDQsForHold]";

            string holdXml = Hold.ToXml(holds);

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@Xml", DbType.Xml, holdXml);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            RDQFactory factory = new RDQFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }

        public Int32 GetWarehouseAvailable(string sku, string size, string warehouse)
        {
            RingFenceDAO dao = new RingFenceDAO();

            List<RingFenceDetail> list = dao.GetWarehouseAvailableCommon(sku, size, warehouse, null);

            return (from a in list where a.Size == size select a.AvailableQty).Sum();

            /*
            string stock, color, dept, div;
            string[] tokens = sku.Split('-');
            div = tokens[0];
            dept = tokens[1];
            stock = tokens[2];
            color = tokens[3];

            Database currDatabase = null;
            if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(div))
            {
                currDatabase = DatabaseFactory.CreateDatabase("DB2EURP");
            }
            else
            {
                currDatabase = DatabaseFactory.CreateDatabase("DB2PROD");
            }
            List<RingFenceDetail> _que;
            _que = new List<RingFenceDetail>();

            DbCommand SQLCommand;

            string SQL = "select ALLOCATABLE_BS_QTY from TC052002 ";
            SQL = SQL + " where ";
            SQL = SQL + "retl_oper_div_cd = '" + div + "' ";
            SQL = SQL + "and stk_dept_num = '" + dept + "' ";
            SQL = SQL + "and stk_num = '" + stock + "' ";
            SQL = SQL + "and stk_wc_num = '" + color + "' ";
            SQL = SQL + "and WHSE_ID_NUM = '" + warehouse + "' ";
            SQL = SQL + "and STK_SIZE_NUM = " + Convert.ToInt32(size);

            SQLCommand = currDatabase.GetSqlStringCommand(SQL);

            DataSet data = new DataSet();
            data = currDatabase.ExecuteDataSet(SQLCommand);

            if (data.Tables.Count > 0)
            {
                AllocationLibraryContext db = new AllocationLibraryContext();
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    return Convert.ToInt32(dr["ALLOCATABLE_BS_QTY"]);
                }
            }
            return 0;
             * */
        }

        /// <summary>
        /// Gets RDQs that are for this hold only, not including ones that are also on hold for another reason
        /// </summary>
        /// <param name="holdID"></param>
        /// <returns></returns>
        public List<RDQ> GetEcommRDQs(int instance)
        {
            List<RDQ> _que;
            _que = new List<RDQ>();

            DbCommand SQLCommand;
            string SQL = "dbo.[getEcommRDQs]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@instanceID", DbType.Int32, instance);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            RDQFactory factory = new RDQFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }

        
        public void DeleteCrossdockRDQs(string div, string store)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.DeleteCrossdockRDQs";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@store", DbType.String, store);

            _database.ExecuteNonQuery(SQLCommand);
        }


        public void ReleaseRDQs(List<RDQ> list, string user)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.ReleaseRDQs";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(list.GetType());
            xs.Serialize(sw, list);
            String xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@xmlDetails", DbType.Xml, xout);
            _database.AddInParameter(SQLCommand, "@user", DbType.String, user);

            _database.ExecuteNonQuery(SQLCommand);
        }

        public void DeleteRDQs(List<RDQ> list, string user)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.DeleteRDQs";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(list.GetType());
            xs.Serialize(sw, list);
            String xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@xmlDetails", DbType.Xml, xout);
            _database.AddInParameter(SQLCommand, "@user", DbType.String, user);

            _database.ExecuteNonQuery(SQLCommand);
        }

        
        public List<RDQ> GetRDQExtractForDate(int instance, DateTime controldate)
        {
            List<RDQ> _que;
            _que = new List<RDQ>();

            DbCommand SQLCommand;
            string SQL = "[dbo].[GetRDQExtractForDate]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@instanceID", DbType.String, instance);
            _database.AddInParameter(SQLCommand, "@date", DbType.String, controldate);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            RDQFactory factory = new RDQFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.CreateFinal(dr));
                }
            }
            return _que;
        }

        public List<RDQ> GetRDQExtractForSkuDate(string sku, DateTime controldate)
        {
            List<RDQ> _que;
            _que = new List<RDQ>();

            DbCommand SQLCommand;
            string SQL = "[dbo].[GetRDQExtractForSkuDate]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@sku", DbType.String, sku);
            _database.AddInParameter(SQLCommand, "@date", DbType.String, controldate);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            RDQFactory factory = new RDQFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.CreateFinal(dr));
                }
            }
            return _que;
        }
        
        public int ApplyHolds(List<RDQ> rdqs, int instance)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.[SetRDQStatus]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(rdqs.GetType());
            xs.Serialize(sw, rdqs);
            String xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@instanceID", DbType.String, instance);
            _database.AddInParameter(SQLCommand, "@rdqs", DbType.Xml, xout);
            //SQLCommand.CommandTimeout = 300;
            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            RDQFactory factory = new RDQFactory();
            int holds = 0;
            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    holds = Convert.ToInt32(dr[0]);
                }
            }
            return holds;
        }

        public int ApplyCancelHolds(List<RDQ> rdqs)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.CheckRDQCancelHolds";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(rdqs.GetType());
            xs.Serialize(sw, rdqs);
            String xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@rdqs", DbType.Xml, xout);
            //SQLCommand.CommandTimeout = 300;
            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            RDQFactory factory = new RDQFactory();
            int holds = 0;
            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    holds = Convert.ToInt32(dr[0]);
                }
            }
            return holds;
        }

    }
}
