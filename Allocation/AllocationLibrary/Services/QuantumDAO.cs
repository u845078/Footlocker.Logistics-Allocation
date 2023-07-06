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

        public LostSalesRequest GetLostSales(string sku)
        {
            LostSalesRequest lostSalesRequest = new LostSalesRequest();
            string SQL = "dbo.[GetLostSales]";
            var sqlCommand = Footlocker.Common.DatabaseService.GetStoredProcCommand(_database, SQL);
            _database.AddInParameter(sqlCommand, "@sku", DbType.String, sku);
            _database.AddOutParameter(sqlCommand, "@beginDate", DbType.String, 10);
            _database.AddOutParameter(sqlCommand, "@endDate", DbType.String, 10);
            _database.AddOutParameter(sqlCommand, "@weekEndDateIndex", DbType.String, 10);
            sqlCommand.CommandTimeout = 300;

            DataSet data = _database.ExecuteDataSet(sqlCommand);

            lostSalesRequest.BeginDate = Convert.ToDateTime(_database.GetParameterValue(sqlCommand, "@beginDate"));
            lostSalesRequest.EndDate = Convert.ToDateTime(_database.GetParameterValue(sqlCommand, "@endDate"));
            lostSalesRequest.WeeklySalesEndIndex = Convert.ToInt16(_database.GetParameterValue(sqlCommand, "@weekEndDateIndex"));

            LostSalesFactory lostSalesFactory = new LostSalesFactory();
            List<DataRow> tempList; //temporary list to store enough data for a single LostSalesInstance
            String product_id; //variable to check each row's product id
            String location_id; //variable to check each row's location id

            //check to see if there is a table returned and that the table has at least one value
            if (data.Tables.Count > 0 && data.Tables[0].Rows.Count > 0)
            {
                tempList = new List<DataRow>();
                //assign variable to first row's product_id
                product_id = Convert.ToString(data.Tables[0].Rows[0]["PRODUCT_ID"]);
                //assign variable to first row's location_id
                location_id = Convert.ToString(data.Tables[0].Rows[0]["LOCATION_ID"]); 
                LostSalesInstance lsi = new LostSalesInstance(); 

                for (int i = 0; i < data.Tables[0].Rows.Count; i++)
                {
                    var dataRow = data.Tables[0].Rows[i];

                    //check to see if it is the last row in the result set, if so add to temp list and send to LostSalesFactory
                    if (i == data.Tables[0].Rows.Count - 1)
                    {
                        tempList.Add(dataRow);

                        //create a LostSalesRequest which represents a single row in the excel sheet and add to the returned list
                        lsi = lostSalesFactory.Create(tempList);
                        lostSalesRequest.LostSales.Add(lsi);
                    }
                    //check to make sure the row has the same product and location id, if so add to temp list
                    else if ((Convert.ToString(dataRow["PRODUCT_ID"]) == product_id) && (Convert.ToString(dataRow["LOCATION_ID"]) == location_id)) 
                    {
                        tempList.Add(dataRow);
                    }
                    //check to see if product_id OR location_id has changed, if so reassign id's and send the temp list to the LostSalesFactory
                    else if ((Convert.ToString(dataRow["PRODUCT_ID"]) != product_id) || (Convert.ToString(dataRow["LOCATION_ID"]) != location_id))
                    {
                        //reassign product_id and location_id
                        product_id = Convert.ToString(dataRow["PRODUCT_ID"]);
                        location_id = Convert.ToString(dataRow["LOCATION_ID"]);

                        //create a LostSalesRequest which represents a single row in the excel sheet and add to the returned list
                        lsi = lostSalesFactory.Create(tempList);
                        lostSalesRequest.LostSales.Add(lsi);

                        //clear temporary list and add the first element of the next product_id
                        tempList.Clear();
                        tempList.Add(dataRow);
                    } 
                }   
            }
            return lostSalesRequest;
        }

        // public List<WSM> GetWSM(string sku)
        //{
        //    List<WSM> wsmList = new List<WSM>();
        //    string SQL = "dbo.[GetWSM]";
        //    var sqlCommand = Footlocker.Common.DatabaseService.GetStoredProcCommand(_database, SQL);
        //    _database.AddInParameter(sqlCommand, "@sku", DbType.String, sku);
        //    sqlCommand.CommandTimeout = 300;

        //    DataSet data = _database.ExecuteDataSet(sqlCommand);

        //    WSMFactory wsmFactory = new WSMFactory();

        //    if (data.Tables.Count > 0)
        //    {
        //        foreach (DataRow dr in data.Tables[0].Rows)
        //        {
        //            wsmList.Add(wsmFactory.Create(dr));
        //        }
        //    }

        //    return wsmList;
        //}

        public List<WSM> GetWSMextract(string sku, bool includeinvalidrecords)
        {
            List<WSM> wsmList = new List<WSM>();
            string SQL = "dbo.[GetWSMextract]";
            var sqlCommand = Footlocker.Common.DatabaseService.GetStoredProcCommand(_database, SQL);
            _database.AddInParameter(sqlCommand, "@sku", DbType.String, sku);
            _database.AddInParameter(sqlCommand, "@includeinvalidrecords", DbType.Boolean, includeinvalidrecords);
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
