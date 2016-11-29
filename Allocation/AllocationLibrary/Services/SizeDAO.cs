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
    public class SizeDAO
    {
        Database _database;

        public SizeDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }


        public List<string> GetSizes(string sku)
        {
            List<string> _que;
            _que = new List<string>();

            DbCommand SQLCommand;
            string SQL = "dbo.[getSizes]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@sku", DbType.String, sku);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(Convert.ToString(dr[0]));
                }
            }
            return _que;
        }

        public void SaveList(List<SizeObj> list)
        {
            DbCommand SQLCommand;
            string SQL;

            SQL = "[dbo].[SaveSizesFromWeb]";
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
