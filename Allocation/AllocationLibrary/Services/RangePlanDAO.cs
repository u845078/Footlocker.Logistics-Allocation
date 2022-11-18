using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using Footlocker.Logistics.Allocation.Models;
using System.IO;
using System.Xml.Serialization;

namespace Footlocker.Logistics.Allocation.Services
{
    public class RangePlanDAO
    {
        public const int _DEFAULT_MAX_LEADTIME = 5;
        Database _database;
        private readonly AllocationLibraryContext db;

        public RangePlanDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
            db = new AllocationLibraryContext();
        }

        public void DeleteRangePlan(long planID)
        {
            DbCommand SQLCommand;
            string SQL = "DeleteRangePlanData";
            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@planID", DbType.Int64, planID);
            SQLCommand.CommandTimeout = 120;
            _database.ExecuteNonQuery(SQLCommand);
        }

        public long GetRangePlanID(string sku)
        {
            return db.RangePlans.Where(rp => rp.Sku == sku)
                .Select(rp => rp.Id).FirstOrDefault();
        }

        /// <summary>
        /// This will update Delivery Groups and associated RangePlans, RangePlanDetails records
        /// </summary>
        /// <param name="deliveryGroups"></param>
        public void UpdateDeliveryGroups(List<DeliveryGroup> deliveryGroups, string user)
        {
            DbCommand SQLCommand;
            string SQL = "dbo.UpdateDeliveryGroups";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(deliveryGroups.GetType());
            xs.Serialize(sw, deliveryGroups);
            string xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@xmlDetails", DbType.Xml, xout);
            _database.AddInParameter(SQLCommand, "@user", DbType.String, user);
            //SQLCommand.CommandTimeout = 300;

            _database.ExecuteNonQuery(SQLCommand);
        }
    }
}
