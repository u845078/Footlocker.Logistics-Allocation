
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
        AllocationLibraryContext db = new AllocationLibraryContext();

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
            string SQL = "dbo.[getUniqueRDQsForHoldXml]";

            string holdXml = Hold.ToXml(holds);

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@Xml", DbType.Xml, holdXml);

            DataSet data = new DataSet();
            SQLCommand.CommandTimeout = 0;
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
            int holds = 0;
            SQL = "dbo.[SetRDQStatus]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(rdqs.GetType());
            xs.Serialize(sw, rdqs);
            String xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@instanceID", DbType.String, instance);
            _database.AddInParameter(SQLCommand, "@rdqs", DbType.Xml, xout);
            SQLCommand.CommandTimeout = 0;
            //DataSet data = new DataSet();
            holds = (int)_database.ExecuteScalar(SQLCommand);

            //RDQFactory factory = new RDQFactory();
            
            //if (data.Tables.Count > 0)
            //{
            //    foreach (DataRow dr in data.Tables[0].Rows)
            //    {
            //        holds = Convert.ToInt32(dr[0]);
            //    }
            //}
            return holds;
        }

        public List<RDQ> ApplyCancelHoldsNew(List<RDQ> rdqs, string division, List<ItemMaster> uniqueItems, List<string> uniqueSkus, string userID)
        {
            List<RDQ> returnValue = new List<RDQ>();
            if (rdqs.Count > 0)
            {
                var cancelHolds = (from ch in db.CurrentActiveHolds
                                   where ch.Division == division &&
                                         ch.ReserveInventory == 0
                                   select ch).ToList();

                foreach (RDQ rdq in rdqs)
                {
                    var item = uniqueItems.Where(i => i.MerchantSku == rdq.Sku).FirstOrDefault();
                    bool holdExists = cancelHolds.Exists(ch => (ch.Department == item.Dept || string.IsNullOrEmpty(ch.Department)) &&
                                                               (ch.Category == item.Category || string.IsNullOrEmpty(ch.Category)) &&
                                                               (ch.Vendor == item.Vendor || string.IsNullOrEmpty(ch.Vendor)) &&
                                                               (ch.Brand == item.Brand || string.IsNullOrEmpty(ch.Brand)) &&
                                                               (ch.Team == item.TeamCode || string.IsNullOrEmpty(ch.Team)) &&
                                                               (ch.SKU == item.MerchantSku || string.IsNullOrEmpty(ch.SKU)) &&
                                                               (ch.Store == rdq.Store || string.IsNullOrEmpty(ch.Store)));

                    if (holdExists)
                    {
                        returnValue.Add(rdq);
                    }                    
                }

                var proxyCreated = db.Configuration.ProxyCreationEnabled;
                db.Configuration.ProxyCreationEnabled = false;
                List<RDQ> skuRDQs = (from r in db.RDQs
                                     where uniqueSkus.Contains(r.Sku)
                                     select r).ToList();
                db.Configuration.ProxyCreationEnabled = proxyCreated;

                List<RDQ> rdqUpdates = (from sr in skuRDQs
                                        join rv in returnValue
                                          on new { sr.Sku, sr.Store, sr.DCID, sr.Size, sr.Qty } equals new { rv.Sku, rv.Store, rv.DCID, rv.Size, rv.Qty }
                                        where sr.CreatedBy.ToUpper().Equals(userID.ToUpper())
                                        select sr).ToList();

                foreach (var ru in rdqUpdates)
                {
                    ru.Status = "REJECTED";
                    ru.RDQRejectedReasonCode = 13;
                }

                UpdateRDQs(rdqUpdates, userID);
            }

            return returnValue;
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

        public void InsertRDQs(List<RDQ> list, string user)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.InsertWebRDQs";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(list.GetType());
            xs.Serialize(sw, list);
            String xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@xmlData", DbType.Xml, xout);
            _database.AddInParameter(SQLCommand, "@user", DbType.String, user);

            _database.ExecuteNonQuery(SQLCommand);
        }

        public void UpdateRDQs(List<RDQ> list, string user)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.UpdateWebRDQs";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(list.GetType());
            xs.Serialize(sw, list);
            String xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@xmlData", DbType.Xml, xout);
            _database.AddInParameter(SQLCommand, "@user", DbType.String, user);

            _database.ExecuteNonQuery(SQLCommand);
        }
    }
}
