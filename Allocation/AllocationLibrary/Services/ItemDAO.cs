using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Services
{
    public class ItemDAO
    {
        private readonly Database _database;
        Database _USdatabase;
        Database _Europedatabase;
        readonly string europeDivisions;
        readonly AllocationLibraryContext db = new AllocationLibraryContext();

        public ItemDAO(string europeDivisions)
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
            _USdatabase = DatabaseFactory.CreateDatabase("DB2PROD_DRIVER");
            _Europedatabase = DatabaseFactory.CreateDatabase("DB2EURP_DRIVER");
            this.europeDivisions = europeDivisions;
        }
        
        public void CreateItemMaster(string sku, int instance)
        {
            ConfigService service = new ConfigService();

            Database mfDatabase;
            mfDatabase = DatabaseFactory.CreateDatabase(service.GetValue(instance, "DB2_ENV"));

            DbCommand SQLCommandMF;
            string[] tokens = sku.Split('-');

            string SQLMF = "SELECT * FROM TC051007 ";
            SQLMF += "WHERE RETL_OPER_DIV_CD = ? and STK_DEPT_NUM = ? and STK_NUM = ? and STK_WC_NUM = ?";

            SQLCommandMF = mfDatabase.GetSqlStringCommand(SQLMF);
            mfDatabase.AddInParameter(SQLCommandMF, "@1", DbType.String, tokens[0]);
            mfDatabase.AddInParameter(SQLCommandMF, "@2", DbType.String, tokens[1]);
            mfDatabase.AddInParameter(SQLCommandMF, "@3", DbType.String, tokens[2]);
            mfDatabase.AddInParameter(SQLCommandMF, "@4", DbType.String, tokens[3]);

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

        public ItemMaster GetItem(long itemID)
        {
            return db.ItemMasters.Where(im => im.ID == itemID).FirstOrDefault();
        }

        public List<string> GetSKUSizes(string sku)
        {
            List<string> sizes = (from a in db.Sizes
                                  where a.Sku == sku
                                  select a.Size).OrderBy(p => p).ToList();
            return sizes;
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
            string divCountryCode;
            Microsoft.Practices.EnterpriseLibrary.Data.Database currentDB;
            DbCommand SQLCommand;
            decimal price = 0.0M;

            string[] tokens = sku.Split('-');
            div = tokens[0];
            dept = tokens[1];
            stock = tokens[2];

            divCountryCode = (from ad in db.AllocationDivisions
                              where ad.DivisionCode == div
                              select ad.DefaultCountryCode).FirstOrDefault();

            if (europeDivisions.Contains(div))
                currentDB = _Europedatabase;            
            else
                currentDB = _USdatabase;

            string SQL = "SELECT A.CURR_RETL_PRICE AS RETAIL ";
            SQL += "  FROM TCMPS016 A ";
            SQL += " WHERE A.RETL_OPER_DIV_CODE = ? and ";
            SQL += "  A.STK_DEPT_NUM = ? and ";
            SQL += "  A.STK_NUM = ? and ";
            SQL += "  A.PRICE_GROUP_CODE = '01' and ";
            SQL += "  A.COUNTRY_CODE = ? and ";
            SQL += "  A.RPA_EFF_DATE = (SELECT MAX(B.RPA_EFF_DATE) ";
            SQL += "     FROM TCMPS016 B";
            SQL += "  WHERE B.RETL_OPER_DIV_CODE =  A.RETL_OPER_DIV_CODE and ";
            SQL += "        B.COUNTRY_CODE       =  A.COUNTRY_CODE and ";
            SQL += "        B.PRICE_GROUP_CODE   =  A.PRICE_GROUP_CODE and ";
            SQL += "        B.STK_DEPT_NUM       =  A.STK_DEPT_NUM and ";
            SQL += "        B.STK_NUM            =  A.STK_NUM)";

            SQLCommand = currentDB.GetSqlStringCommand(SQL);
            currentDB.AddInParameter(SQLCommand, "@1", DbType.String, div);
            currentDB.AddInParameter(SQLCommand, "@2", DbType.String, dept);
            currentDB.AddInParameter(SQLCommand, "@3", DbType.String, stock);
            currentDB.AddInParameter(SQLCommand, "@4", DbType.String, divCountryCode);

            DataSet data;
            data = currentDB.ExecuteDataSet(SQLCommand);

            if (data.Tables.Count > 0)
            {
                DataRow dr = data.Tables[0].Rows[0];
                price = Convert.ToDecimal(dr["RETAIL"]);
            }

            return price;
        }
    }
}