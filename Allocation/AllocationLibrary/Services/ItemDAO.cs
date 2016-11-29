
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Factories;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Services
{
    public class ItemDAO
    {
        Database _database;

        public ItemDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");

        }

        
        public void CreateItemMaster(string sku, int instance)
        {
            ConfigService service = new ConfigService();

            Database mfDatabase;
            mfDatabase = DatabaseFactory.CreateDatabase(service.GetValue(instance, "DB2_ENV"));

           
            //SQLMF = "delete from " + System.Configuration.ConfigurationManager.AppSettings["DB2PREFIX"] + "TCQTM001 ";
            DbCommand SQLCommandMF;
            string SQLMF = "SELECT * ";
            SQLMF = SQLMF + " FROM TC051007 ";
            string[] tokens = sku.Split('-');
            SQLMF = SQLMF + " WHERE RETL_OPER_DIV_CD = '"+ tokens[0] +
                "' and STK_DEPT_NUM = '" + tokens[1] + "' and STK_NUM = '" + tokens[2] + 
                "' and STK_WC_NUM = '" + tokens[3] + "'";

            SQLCommandMF = mfDatabase.GetSqlStringCommand(SQLMF);
            DataSet data = mfDatabase.ExecuteDataSet(SQLCommandMF);

            List<SizeObj> newSizes = new List<SizeObj>();
            SizeObj size;
            string temp;
            string description="";
            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    for (int i = 5; i <= 22; i++)
                    {
                        temp = Convert.ToString(dr[i]);
                        if (temp.Substring(3) == "0")
                        {
                            if (temp.Substring(0, 3) == "999")
                            {
                                break;
                            }
                            size = new SizeObj();
                            size.Sku = sku;
                            size.InstanceID = instance;
                            size.Size = temp.Substring(0, 3);
                            newSizes.Add(size);
                        }
                    }
                    
                }
            }

            if (newSizes.Count > 0)
            {
                SizeDAO sizeDAO = new SizeDAO();
                sizeDAO.SaveList(newSizes);

                DbCommand SQLCommand;
                string SQL;
                SQL = "dbo.CreateItemMaster";

                int check = Footlocker.Common.SKUService.CalculateCheckDigit(sku.Replace("-", ""));
                string fullsku = sku.Substring(0, 12) + check + "-" + sku.Substring(12, 2);

                SQLCommand = _database.GetStoredProcCommand(SQL);
                _database.AddInParameter(SQLCommand, "@sku", DbType.String, fullsku);
                _database.AddInParameter(SQLCommand, "@merchantsku", DbType.String, sku);

                _database.ExecuteNonQuery(SQLCommand);

            }
            else
            {
                throw new Exception("There are no sizes on mainframe for this sku.");
            }
        }

        public void UpdateActiveARStatus()
        {
            DbCommand SQLCommand;
            string SQL;

            SQL = "[dbo].[UpdateActiveARStatus]";
            SQLCommand = _database.GetStoredProcCommand(SQL);

            _database.ExecuteNonQuery(SQLCommand);
        }
    }
}