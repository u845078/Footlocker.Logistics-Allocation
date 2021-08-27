
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Factories;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Services
{
    public class ItemDAO
    {
        Database _database;
        AllocationLibraryContext db;

        public ItemDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
            db = new AllocationLibraryContext();
        }

        public long RetreiveOrCreateItemID(string SKU)
        {
            var itemlist = db.ItemMasters.Where(im => im.MerchantSku == SKU).ToList();

            if (itemlist.Count() > 0)
            {
                return itemlist.First().ID;
            }
            else
            {
                string div = SKU.Substring(0, 2);
                int instance = (from a in db.InstanceDivisions
                                where a.Division == div
                                select a.InstanceID).First();
                try
                {
                    CreateItemMaster(SKU, instance);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                return (from a in db.ItemMasters
                        where a.MerchantSku == SKU
                        select a.ID).First();
            }
        }

        public void CreateItemMaster(string sku, int instance)
        {
            ConfigService service = new ConfigService();
            string[] tokens = sku.Split('-');

            Database mfDatabase;
            mfDatabase = DatabaseFactory.CreateDatabase(service.GetValue(instance, "DB2_ENV"));

            DbCommand SQLCommandMF;
            string SQLMF = "SELECT * FROM TC051007 ";
            SQLMF += string.Format("WHERE RETL_OPER_DIV_CD = '{0}' and STK_DEPT_NUM = '{1}' and STK_NUM = '{2}' and STK_WC_NUM = '{3}'", 
                tokens[0], tokens[1], tokens[2], tokens[3]);

            SQLCommandMF = mfDatabase.GetSqlStringCommand(SQLMF);
            DataSet data = mfDatabase.ExecuteDataSet(SQLCommandMF);

            List<SizeObj> newSizes = new List<SizeObj>();
            SizeObj size;
            string temp;

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

                            size = new SizeObj()
                            {
                                Sku = sku,
                                InstanceID = instance,
                                Size = temp.Substring(0, 3)
                            };
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