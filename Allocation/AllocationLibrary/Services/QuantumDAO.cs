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
    public class QuantumDAO
    {
         Database _database;

        public QuantumDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        public List<LostSalesRequest> GetLostSales(string sku)
        {
            List<LostSalesRequest> lostSalesList = new List<LostSalesRequest>();
            string SQL = "dbo.[GetLostSales]";
            var sqlCommand = Footlocker.Common.DatabaseService.GetStoredProcCommand(_database, SQL);
            _database.AddInParameter(sqlCommand, "@sku", DbType.String, sku);
            sqlCommand.CommandTimeout = 300;

            DataSet data = _database.ExecuteDataSet(sqlCommand);

            LostSalesFactory lostSalesFactory = new LostSalesFactory();

            if (data.Tables.Count > 0)
            {
                List<DataRow> tempList = new List<DataRow>(); //temporary list to store enough data for a single LostSalesRequest

                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    //lostSalesList.Add(lostSalesFactory.Create(dr)); :FOR USE WITH PIVOT STORED PROC
                    tempList.Add(dr);
                    if (tempList.Count == 16) //16 rows of the query is 14 consecutive days of daily sales as well as 2 total weekly sales
                    {
                        LostSalesRequest lsr = lostSalesFactory.Create(tempList);
                        if (lsr.DailySales.Count > 0) //do not add a LostSalesRequest with no daily values
                        {
                            lostSalesList.Add(lsr);
                        }
                        tempList.Clear(); //clear temporary list for next LostSalesRequest
                    }
                }
            }

            return lostSalesList;
        }

         public List<WSM> GetWSM(string sku)
        {
            List<WSM> wsmList = new List<WSM>();
            string SQL = "dbo.[GetWSM]";
            var sqlCommand = Footlocker.Common.DatabaseService.GetStoredProcCommand(_database, SQL);
            _database.AddInParameter(sqlCommand, "@sku", DbType.String, sku);
            sqlCommand.CommandTimeout = 300;

            DataSet data = _database.ExecuteDataSet(sqlCommand);

            WSMFactory wsmFactory = new WSMFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    wsmList.Add(wsmFactory.Create(dr));
                }
            }

            return wsmList;
        }
    }
}
