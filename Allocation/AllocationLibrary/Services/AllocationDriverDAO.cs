
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;

namespace Footlocker.Logistics.Allocation.Services
{
    public class AllocationDriverDAO
    {
        Database _database;
        Database _USdatabase;
        Database _Europedatabase;
        string _prefix;

        public AllocationDriverDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
            _USdatabase = DatabaseFactory.CreateDatabase("DB2PROD_DRIVER");
            _Europedatabase = DatabaseFactory.CreateDatabase("DB2EURP_DRIVER");
            //_prefix = "DB2TEST.";
            _prefix = System.Configuration.ConfigurationManager.AppSettings["DB2PREFIX_DRIVER"];
        }
        
        public void Save(AllocationDriver objectToSave, string user)
        {
            if (System.Configuration.ConfigurationManager.AppSettings["UPDATE_MF"] != "FALSE")
            {
                Database db;

                if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(objectToSave.Division))
                {
                    db = _Europedatabase;
                }
                else
                {
                    db = _USdatabase;
                }

                DbCommand SQLCommandMF;
                string SQLMF;
                try
                {
                    SQLMF = "insert into " + _prefix + "TCQTM001 values (";
                    SQLMF = SQLMF + "'" + objectToSave.Division + "', ";
                    SQLMF = SQLMF + "'" + objectToSave.Department + "', ";
                    SQLMF = SQLMF + "'" + objectToSave.ConvertDate.ToString("yyyy-MM-dd") + "', ";
                    SQLMF = SQLMF + "'" + objectToSave.AllocateDate.ToString("yyyy-MM-dd") + "', ";
                    SQLMF = SQLMF + "'" + user + "', ";
                    SQLMF = SQLMF + "'" + DateTime.Now.ToString("yyyy-MM-dd") + "') ";

                    SQLCommandMF = db.GetSqlStringCommand(SQLMF);
                    db.ExecuteNonQuery(SQLCommandMF);
                }
                catch (Exception ex)
                {
                    SQLMF = "update " + _prefix + "TCQTM001 set ";
                    SQLMF = SQLMF + " convert_date = '" + objectToSave.ConvertDate.ToString("yyyy-MM-dd") + "', ";
                    SQLMF = SQLMF + " allocate_date = '" + objectToSave.AllocateDate.ToString("yyyy-MM-dd") + "', ";
                    SQLMF = SQLMF + " user_name = '" + user + "', ";
                    SQLMF = SQLMF + " create_date = '" + DateTime.Now.ToString("yyyy-MM-dd") + "' ";
                    SQLMF = SQLMF + " where retl_oper_div_code = '" + objectToSave.Division + "' ";
                    SQLMF = SQLMF + " and stk_dept_num = '" + objectToSave.Department + "' ";

                    SQLCommandMF = db.GetSqlStringCommand(SQLMF);
                    db.ExecuteNonQuery(SQLCommandMF);

                }
            }

            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.SaveAllocationDriver";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@Division", DbType.String, objectToSave.Division);
            _database.AddInParameter(SQLCommand, "@Department", DbType.String, objectToSave.Department);
            _database.AddInParameter(SQLCommand, "@AllocateDate", DbType.DateTime, objectToSave.AllocateDate);
            _database.AddInParameter(SQLCommand, "@ConvertDate", DbType.DateTime, objectToSave.ConvertDate);
            _database.AddInParameter(SQLCommand, "@OrderPlanningDate", DbType.DateTime, objectToSave.OrderPlanningDate);
            _database.AddInParameter(SQLCommand, "@CheckNormals", DbType.Boolean, objectToSave.CheckNormals);
            _database.AddInParameter(SQLCommand, "@CHANGE_BY", DbType.String, user);

            _database.ExecuteNonQuery(SQLCommand);
        }

        
        public List<AllocationDriver> GetAllocationDriverList(string division)
        {
            List<AllocationDriver> _que;
            _que = new List<AllocationDriver>();

            DbCommand SQLCommand;
            string SQL = "dbo.GetAllocationDrivers";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@Division", DbType.String, division);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            AllocationDriverFactory factory = new AllocationDriverFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }

        public List<AllocationDriver> GetAllocationDriversForInstance(int instance)
        {
            List<AllocationDriver> _que;
            _que = new List<AllocationDriver>();

            DbCommand SQLCommand;
            string SQL = "dbo.GetAllocationDriversForInstance";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@instance", DbType.Int32, instance);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            AllocationDriverFactory factory = new AllocationDriverFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }

        public List<AllocationDriver> GetActiveAllocationDriversForInstance(int instance)
        {
            List<AllocationDriver> _que;
            _que = new List<AllocationDriver>();

            DbCommand SQLCommand;
            string SQL = "dbo.GetActiveAllocationDriversForInstance";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@instance", DbType.Int32, instance);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            AllocationDriverFactory factory = new AllocationDriverFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }


        public AllocationDriver GetAllocationDriver(string div, string dept)
        {

            DbCommand SQLCommand;
            string SQL = "dbo.GetAllocationDriver";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@dept", DbType.String, dept);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            AllocationDriverFactory factory = new AllocationDriverFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    return factory.Create(dr);
                }
            }
            return null;
        }

        
        public void DeleteAllocationDriver(string div, string dept)
        {
            DbCommand SQLCommandMF;
            string SQLMF;

            Database db;

            if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(div))
            {
                db = _Europedatabase;
            }
            else
            {
                db = _USdatabase;
            }

            //SQLMF = "delete from " + System.Configuration.ConfigurationManager.AppSettings["DB2PREFIX"] + "TCQTM001 ";
            SQLMF = "delete from " + _prefix + "TCQTM001 ";
            SQLMF = SQLMF + " where retl_oper_div_code = '" + div + "' ";
            SQLMF = SQLMF + " and stk_dept_num = '" + dept + "' ";
            SQLCommandMF = db.GetSqlStringCommand(SQLMF);
            db.ExecuteNonQuery(SQLCommandMF);

            DbCommand SQLCommand;
            string SQL;
            SQL = "dbo.DeleteAllocationDriver";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@dept", DbType.String, dept);

            _database.ExecuteNonQuery(SQLCommand);
        }
    }
}