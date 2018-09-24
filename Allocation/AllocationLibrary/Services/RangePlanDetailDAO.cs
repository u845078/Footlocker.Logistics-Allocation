
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.IO;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;
using System.Xml;
using System.Xml.Serialization;

namespace Footlocker.Logistics.Allocation.Services
{
    public class RangePlanDetailDAO
    {
        Database _database;

        public RangePlanDetailDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }


        public void AddStores(List<RangePlanDetail> objectsToSave)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "addStoresToRangePlan";

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode root = xmlDoc.CreateElement("Root");
            xmlDoc.AppendChild(root);

            foreach (RangePlanDetail det in objectsToSave)
            {
                root.AppendChild(det.ToXmlNode(root));
            }

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@xmldetails", DbType.String, xmlDoc.InnerXml);

            _database.ExecuteNonQuery(SQLCommand);
        }

        public void DeleteStoresForPlan(Int64 planID)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "deleteStoreRangeDetails";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@planID", DbType.Int64, planID);

            _database.ExecuteNonQuery(SQLCommand);
        }

        public static bool StoreInPlan(Int64 planID, string div, string store)
        {
            return true;
        }

        
        public List<DeliveryGroup> GetBadDeliveryGroups()
        {
            List<DeliveryGroup> _que;
            _que = new List<DeliveryGroup>();

            DbCommand SQLCommand;
            string SQL = "[GetBadDeliveryGroups]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            //_database.AddInParameter(SQLCommand, "@variable", DbType.String, variable);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);
          

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    DeliveryGroup dg = new DeliveryGroup();
                    dg.PlanID = Convert.ToInt64(dr["PlanID"]);
                    dg.RuleSetID = Convert.ToInt64(dr["RuleSetID"]);
                    dg.Name = Convert.ToString(dr["Name"]);
                    dg.StartDate = Convert.ToDateTime(dr["StartDate"]);
                    dg.EndDate = Convert.ToDateTime(dr["EndDate"]);

                    _que.Add(dg);
                }
            }
            return _que;
        }

        public List<BulkRange> BulkUpdateRange(List<BulkRange> list, string user)
        {
            List<BulkRange> _que;
            _que = new List<BulkRange>();

            DbCommand SQLCommand;
            string SQL = "[SaveBulkRanges]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@user", DbType.String, user);
            //_database.AddInParameter(SQLCommand, "@rangestartdate", DbType.DateTime, list[0].RangeStartDate);
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(list.GetType());
            xs.Serialize(sw, list);
            String xout = sw.ToString();
            _database.AddInParameter(SQLCommand, "@xmlDetails", DbType.Xml, xout);

            SQLCommand.CommandTimeout = 300;

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);


            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    BulkRange dg = new BulkRange();
                    dg.BaseDemand = Convert.ToString(dr["BaseDemand"]);
                    dg.Max = Convert.ToString(dr["Max"]);
                    dg.Min = Convert.ToString(dr["Min"]);
                    dg.MinEndDateOverride = Convert.ToString(dr["MinEndDateOverride"]);
                    dg.Range = Convert.ToString(dr["Range"]);
                    dg.RangeStartDate = Convert.ToString(dr["RangeStartDate"]);
                    dg.Size = Convert.ToString(dr["Size"]);
                    dg.Sku = Convert.ToString(dr["MerchantSku"]);
                    dg.Store = Convert.ToString(dr["Store"]);
                    dg.Division = Convert.ToString(dr["Division"]);
                    dg.Error = Convert.ToString(dr["Error"]);

                    _que.Add(dg);
                }
            }
            return _que;
        }

        public List<BulkRange> GetBulkRangesForSku(string sku)
        {
            List<BulkRange> _que;
            _que = new List<BulkRange>();

            DbCommand SQLCommand;
            string SQL = "[GetRangeForUpload]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@sku", DbType.String, sku);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);


            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    BulkRange dg = new BulkRange();
                    dg.BaseDemand = Convert.ToString(dr["BaseDemand"]);
                    dg.Max = Convert.ToString(dr["Max"]);
                    dg.Min = Convert.ToString(dr["Min"]);
                    dg.MinEndDateOverride = Convert.ToString(dr["MinEndDateOverride"]);
                    dg.Range = Convert.ToString(dr["Range"]);
                    dg.RangeStartDate = Convert.ToString(dr["RangeStartDate"]);
                    dg.Size = Convert.ToString(dr["Size"]);
                    dg.Sku = Convert.ToString(dr["MerchantSku"]);
                    dg.Store = Convert.ToString(dr["Store"]);
                    dg.Division = Convert.ToString(dr["Division"]);
                    dg.DeliveryGroupName = Convert.ToString(dr["deliverygroupname"]);
                    dg.League = Convert.ToString(dr["League"]);
                    dg.Region = Convert.ToString(dr["Region"]);
                    _que.Add(dg);
                }
            }
            return _que;
        }

        public void ReassignStartDates(string division, string store)
        {
            DbCommand SQLCommand;
            string SQL = "[ReassignStartDates]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@division", DbType.String, division);
            _database.AddInParameter(SQLCommand, "@store", DbType.String, store);

            _database.ExecuteNonQuery(SQLCommand);
        }

    }
}
