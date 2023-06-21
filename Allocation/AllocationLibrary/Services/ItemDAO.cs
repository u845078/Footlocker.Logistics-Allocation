using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Factories;
using Footlocker.Logistics.Allocation.Models;
using System.Linq;
using System.Data.SqlClient;

namespace Footlocker.Logistics.Allocation.Services
{
    public class ItemDAO
    {
        private readonly Database _database;
        Database _USdatabase;
        Database _Europedatabase;
        readonly AllocationLibraryContext db = new AllocationLibraryContext();

        public ItemDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
            _USdatabase = DatabaseFactory.CreateDatabase("DB2PROD_DRIVER");
            _Europedatabase = DatabaseFactory.CreateDatabase("DB2EURP_DRIVER");
        }
        
        public void CreateItemMaster(string sku, int instance)
        {
            ConfigService service = new ConfigService();

            Database mfDatabase;
            mfDatabase = DatabaseFactory.CreateDatabase(service.GetValue(instance, "DB2_ENV"));

            DbCommand SQLCommandMF;
            string[] tokens = sku.Split('-');

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

        public long GetItemID(string sku)
        {
            return db.ItemMasters.Where(im => im.MerchantSku == sku).Select(im => im.ID).FirstOrDefault();
        }

        public bool DoValidSizesExist(string sku, string size)
        {
            int sizeCount = db.Sizes.Where(s => s.Sku == sku && s.Size == size).Count();

            sizeCount += (from a in db.ItemPacks
                          join b in db.ItemMasters
                            on a.ItemID equals b.ID
                         where b.MerchantSku == sku &&
                               a.Name == size
                     select a).Count();

            return sizeCount > 0;
        }

        public decimal GetLocalPrice(string sku)
        {
            string div;
            string dept;
            string stock;
            string color;
            Database currentDB;
            DbCommand SQLCommand;
            decimal price = 0.0M;

            string[] tokens = sku.Split('-');
            div = tokens[0];
            dept = tokens[1];
            stock = tokens[2];
            color = tokens[3];

            if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(div))
                currentDB = _Europedatabase;            
            else
                currentDB = _USdatabase;

            string SQL = "select stock_xvat_price from TCISR003 ";
            SQL += " where RETL_OPER_DIV_CODE = '" + div + "' and ";
            SQL += "  STK_DEPT_NUM = '" + dept + "' and ";
            SQL += "  STK_NUM = '" + stock + "' and ";
            SQL += "  STK_WDTH_COLOR_NUM = '" + color + "' ";

            SQLCommand = currentDB.GetSqlStringCommand(SQL);

            DataSet data;
            data = currentDB.ExecuteDataSet(SQLCommand);

            if (data.Tables.Count > 0)
            {
                DataRow dr = data.Tables[0].Rows[0];
                price = Convert.ToDecimal(dr["stock_xvat_price"]);
            }

            return price;
        }
    }
}