using Footlocker.Logistics.Allocation.Factories;
using Footlocker.Logistics.Allocation.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Services
{
    public class LostSalesDAO
    {
         Database _database;

        public LostSalesDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        public List<LostSalesRequest> GetLostSales(string sku)
        {
            List<LostSalesRequest> list = new List<LostSalesRequest>();
            string SQL = "dbo.[GetLostSales]";
            var sqlCommand = Footlocker.Common.DatabaseService.GetStoredProcCommand(_database, SQL);
            _database.AddInParameter(sqlCommand, "@sku", DbType.String, sku);
            sqlCommand.CommandTimeout = 300;

            DataSet data = _database.ExecuteDataSet(sqlCommand);

            LostSalesFactory factory = new LostSalesFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    list.Add(factory.Create(dr));
                }
            }

            return list;
        }
    }
}
