using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace Footlocker.Logistics.Allocation.Services
{
    public class SizeAllocationDAO
    {
        Database _database;

        public SizeAllocationDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }
        
        public List<SizeAllocation> GetSizeAllocationList(long planID)
        {
            List<SizeAllocation> _que;
            _que = new List<SizeAllocation>();

            DbCommand SQLCommand;
            string SQL = "dbo.[getSizeAllocation]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@plan", DbType.String, planID);

            DataSet data;
            data = _database.ExecuteDataSet(SQLCommand);

            SizeAllocationFactory factory = new SizeAllocationFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }


        
        public void Save(SizeAllocation sa, IEnumerable<StoreLookup> stores)
        {
            DbCommand SQLCommand;
            string SQL;

            if (stores != null)
            {
                SQL = "[dbo].[InsertGroupSizeAllocation]";
                SQLCommand = _database.GetStoredProcCommand(SQL);
                //TODO:  Add xml list of stores to affect
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode root = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(root);

                foreach (StoreLookup det in stores)
                {
                    root.AppendChild(det.ToXmlNode(root));
                }
                _database.AddInParameter(SQLCommand, "@xml", DbType.String, xmlDoc.InnerXml);
            }
            else
            {
                SQL = "[dbo].[InsertTotalSizeAllocation]";
                SQLCommand = _database.GetStoredProcCommand(SQL);
            }

            _database.AddInParameter(SQLCommand, "@plan", DbType.Int64, sa.PlanID);
            _database.AddInParameter(SQLCommand, "@size", DbType.String, sa.Size);
            _database.AddInParameter(SQLCommand, "@min", DbType.String, sa.Min);
            _database.AddInParameter(SQLCommand, "@max", DbType.String, sa.Max);
            _database.AddInParameter(SQLCommand, "@days", DbType.String, sa.Days);
            _database.AddInParameter(SQLCommand, "@demand", DbType.Decimal, sa.InitialDemand);
            _database.AddInParameter(SQLCommand, "@minEndDays", DbType.Int32, sa.MinEndDays);
            if (sa.Range)
            {
                _database.AddInParameter(SQLCommand, "@range", DbType.Int16, 1);
            }
            else
            {
                _database.AddInParameter(SQLCommand, "@range", DbType.Int16, 0);
            }


            _database.ExecuteNonQuery(SQLCommand);
        }

        public void SaveMin(SizeAllocation sa, IEnumerable<StoreLookup> stores)
        {
            DbCommand SQLCommand;
            string SQL;

            if (stores != null)
            {
                SQL = "[dbo].[InsertGroupSizeAllocationMin]";
                SQLCommand = _database.GetStoredProcCommand(SQL);
                //TODO:  Add xml list of stores to affect
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode root = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(root);

                foreach (StoreLookup det in stores)
                {
                    root.AppendChild(det.ToXmlNode(root));
                }
                _database.AddInParameter(SQLCommand, "@xml", DbType.String, xmlDoc.InnerXml);
            }
            else
            {
                SQL = "[dbo].[InsertTotalSizeAllocation]";
                SQLCommand = _database.GetStoredProcCommand(SQL);
            }

            _database.AddInParameter(SQLCommand, "@plan", DbType.Int64, sa.PlanID);
            _database.AddInParameter(SQLCommand, "@size", DbType.String, sa.Size);
            _database.AddInParameter(SQLCommand, "@min", DbType.String, sa.Min);

            _database.ExecuteNonQuery(SQLCommand);
        }

        public void SaveMax(SizeAllocation sa, IEnumerable<StoreLookup> stores)
        {
            DbCommand SQLCommand;
            string SQL;

            if (stores != null)
            {
                SQL = "[dbo].[InsertGroupSizeAllocationMax]";
                SQLCommand = _database.GetStoredProcCommand(SQL);
                //TODO:  Add xml list of stores to affect
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode root = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(root);

                foreach (StoreLookup det in stores)
                {
                    root.AppendChild(det.ToXmlNode(root));
                }
                _database.AddInParameter(SQLCommand, "@xml", DbType.String, xmlDoc.InnerXml);
            }
            else
            {
                SQL = "[dbo].[InsertTotalSizeAllocation]";
                SQLCommand = _database.GetStoredProcCommand(SQL);
            }

            _database.AddInParameter(SQLCommand, "@plan", DbType.Int64, sa.PlanID);
            _database.AddInParameter(SQLCommand, "@size", DbType.String, sa.Size);
            _database.AddInParameter(SQLCommand, "@max", DbType.String, sa.Max);

            _database.ExecuteNonQuery(SQLCommand);
        }

        public void SaveInitialDemand(SizeAllocation sa, IEnumerable<StoreLookup> stores)
        {
            DbCommand SQLCommand;
            string SQL;

            if (stores != null)
            {
                SQL = "[dbo].[InsertGroupSizeAllocationInitialDemand]";
                SQLCommand = _database.GetStoredProcCommand(SQL);
                //TODO:  Add xml list of stores to affect
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode root = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(root);

                foreach (StoreLookup det in stores)
                {
                    root.AppendChild(det.ToXmlNode(root));
                }
                _database.AddInParameter(SQLCommand, "@xml", DbType.String, xmlDoc.InnerXml);
            }
            else
            {
                SQL = "[dbo].[InsertTotalSizeAllocation]";
                SQLCommand = _database.GetStoredProcCommand(SQL);
            }

            _database.AddInParameter(SQLCommand, "@plan", DbType.Int64, sa.PlanID);
            _database.AddInParameter(SQLCommand, "@size", DbType.String, sa.Size);
            _database.AddInParameter(SQLCommand, "@demand", DbType.Decimal, sa.InitialDemand);


            _database.ExecuteNonQuery(SQLCommand);
        }

        public void SaveRangeFlag(SizeAllocation sa, IEnumerable<StoreLookup> stores)
        {
            DbCommand SQLCommand;
            string SQL;

            if (stores != null)
            {
                SQL = "[dbo].[InsertGroupSizeAllocationRangeFlag]";
                SQLCommand = _database.GetStoredProcCommand(SQL);
                //TODO:  Add xml list of stores to affect
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode root = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(root);

                foreach (StoreLookup det in stores)
                {
                    root.AppendChild(det.ToXmlNode(root));
                }
                _database.AddInParameter(SQLCommand, "@xml", DbType.String, xmlDoc.InnerXml);
            }
            else
            {
                SQL = "[dbo].[InsertTotalSizeAllocation]";
                SQLCommand = _database.GetStoredProcCommand(SQL);
            }

            _database.AddInParameter(SQLCommand, "@plan", DbType.Int64, sa.PlanID);
            _database.AddInParameter(SQLCommand, "@size", DbType.String, sa.Size);
            if (sa.Range)
            {
                _database.AddInParameter(SQLCommand, "@range", DbType.Int16, 1);
            }
            else
            {
                _database.AddInParameter(SQLCommand, "@range", DbType.Int16, 0);
            }


            _database.ExecuteNonQuery(SQLCommand);
        }

        public void SaveMinEndDays(SizeAllocation sa, IEnumerable<StoreLookup> stores)
        {
            DbCommand SQLCommand;
            string SQL;

            if (stores != null)
            {
                SQL = "[dbo].[InsertGroupSizeAllocationMinEndDate]";
                SQLCommand = _database.GetStoredProcCommand(SQL);
                //TODO:  Add xml list of stores to affect
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode root = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(root);

                foreach (StoreLookup det in stores)
                {
                    root.AppendChild(det.ToXmlNode(root));
                }
                _database.AddInParameter(SQLCommand, "@xml", DbType.String, xmlDoc.InnerXml);
            }
            else
            {
                SQL = "[dbo].[InsertTotalSizeAllocation]";
                SQLCommand = _database.GetStoredProcCommand(SQL);
            }

            _database.AddInParameter(SQLCommand, "@plan", DbType.Int64, sa.PlanID);
            _database.AddInParameter(SQLCommand, "@size", DbType.String, sa.Size);
            _database.AddInParameter(SQLCommand, "@minEndDays", DbType.Int32, sa.MinEndDays);


            _database.ExecuteNonQuery(SQLCommand);
        }


        public void SaveList(List<SizeAllocation> list)
        {
            DbCommand SQLCommand;
            string SQL;

            SQL = "[dbo].[InsertSizeAllocations]";
            SQLCommand = _database.GetStoredProcCommand(SQL);
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(list.GetType());
            xs.Serialize(sw, list);
            String xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@xml", DbType.Xml, xout);

            _database.ExecuteNonQuery(SQLCommand);
        }

    }
}
