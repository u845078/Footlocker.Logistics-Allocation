
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Services
{
    public class ProductTypeDAO
    {

        public ProductTypeDAO()
        {
        }

        
        public List<ProductType> GetProductTypeList(string division)
        {
            List<ProductType> _que;
            _que = new List<ProductType>();

            Database myDatabase;
            Division div = Footlocker.Common.DivisionService.GetDivision(division);
            myDatabase = DatabaseFactory.CreateDatabase(Convert.ToString(div.ConnectionName));

            DbCommand SQLCommand;
            string SQL = "select a.RETL_OPER_DIV_CODE,a.STK_DEPT_NUM, b.PRODUCT_TYP_CODE,b.PRODUCT_TYP_NAME,b.PRODUCT_TYP_ID from TCISR082 a, TCISR079 b";
            SQL = SQL + " where a.product_typ_id = b.product_typ_id";
            SQL = SQL + " and a.retl_oper_div_code = '" + division + "'";

            SQLCommand = myDatabase.GetSqlStringCommand(SQL);
            //_database.AddInParameter(SQLCommand, "@variable", DbType.String, variable);

            DataSet data = new DataSet();
            data = myDatabase.ExecuteDataSet(SQLCommand);

            ProductTypeFactory factory = new ProductTypeFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }

        public void UpdateList(List<ProductType> list)
        {

            Database myDatabase;
            if (list.Count > 0)
            {
                Division div = Footlocker.Common.DivisionService.GetDivision(list[0].Division);
                myDatabase = DatabaseFactory.CreateDatabase(Convert.ToString(div.ConnectionName));
            }
            else
            {
                myDatabase = DatabaseFactory.CreateDatabase("DB2PROD");
            }

            Footlocker.FLLogger log = new FLLogger("c:\\log\\allocationupload");
            DbCommand SQLCommand;
            string SQL = "update TCISR083 set PRODUCT_TYP_ID = ?";
            SQL = SQL + ",MODIFIED_BY_USERID ='ALLCUPL'";
            SQL = SQL + ",MODIFIED_ON_DTTM = CURRENT TIMESTAMP";
            SQL = SQL + " where RETL_OPER_DIV_CODE = ?";
            SQL = SQL + " and STK_DEPT_NUM = ?";
            SQL = SQL + " and STK_NUM = ?";

            DbCommand SQLCommand3;
            string SQL3 = "insert into TCISR083 (RETL_OPER_DIV_CODE,STK_DEPT_NUM,STK_NUM,STK_WDTH_COLOR_NUM,PRODUCT_TYP_ID,CREATED_BY_USERID,MODIFIED_BY_USERID,CREATED_ON_DTTM,MODIFIED_ON_DTTM ) ";
            SQL3 = SQL3 + "values (?,?,?,'',?,'ALLCUPL','ALLCUPL',CURRENT TIMESTAMP,CURRENT TIMESTAMP) ";

            DbCommand SQLCommand2;
            string SQL2 = "insert into TCISR084";
            SQL2 = SQL2 + " (RETL_OPER_DIV_CODE,STK_DEPT_NUM,STK_NUM,STK_WDTH_COLOR_NUM,MODIFIED_ON_DTTM,PRODUCT_TYP_ID,MODIFIED_BY_USERID)";
            SQL2 = SQL2 + " values ";
            SQL2 = SQL2 + " (?,?,?,'',CURRENT TIMESTAMP,?,'ALLCUPL')";

            log.Log("started loading " + list.Count, FLLogger.eLogMessageType.eInfo);
            foreach (ProductType p in list)
            {
                try
                {
                    SQLCommand3 = myDatabase.GetSqlStringCommand(SQL3);
                    myDatabase.AddInParameter(SQLCommand3, "@1", DbType.String, p.Division);
                    myDatabase.AddInParameter(SQLCommand3, "@2", DbType.String, p.Dept);
                    myDatabase.AddInParameter(SQLCommand3, "@3", DbType.String, p.StockNumber);
                    myDatabase.AddInParameter(SQLCommand3, "@4", DbType.Int32, p.ProductTypeID);

                    myDatabase.ExecuteNonQuery(SQLCommand3);
                }
                catch
                {
                    SQLCommand = myDatabase.GetSqlStringCommand(SQL);
                    myDatabase.AddInParameter(SQLCommand, "@1", DbType.Int32, p.ProductTypeID);
                    myDatabase.AddInParameter(SQLCommand, "@2", DbType.String, p.Division);
                    myDatabase.AddInParameter(SQLCommand, "@3", DbType.String, p.Dept);
                    myDatabase.AddInParameter(SQLCommand, "@4", DbType.String, p.StockNumber);

                    myDatabase.ExecuteNonQuery(SQLCommand);
                }
                SQLCommand2 = myDatabase.GetSqlStringCommand(SQL2);
                myDatabase.AddInParameter(SQLCommand2, "@1", DbType.String, p.Division);
                myDatabase.AddInParameter(SQLCommand2, "@2", DbType.String, p.Dept);
                myDatabase.AddInParameter(SQLCommand2, "@3", DbType.String, p.StockNumber);
                myDatabase.AddInParameter(SQLCommand2, "@4", DbType.Int32, p.ProductTypeID);

                myDatabase.ExecuteNonQuery(SQLCommand2);

            }
            log.Log("finished loading " + list.Count, FLLogger.eLogMessageType.eInfo);

        }
    }
}