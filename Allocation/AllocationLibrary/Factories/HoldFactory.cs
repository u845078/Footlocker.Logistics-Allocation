using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    /// <summary>
    /// Provides an object representation of a hold factory.
    /// </summary>
    public class HoldFactory : IBiExtractFactory<Hold>
    {
        /// <summary>
        /// Create a hold.
        /// </summary>
        /// <param name="reader">The data reader containing the hold's properties.</param>
        /// <returns>The new hold.</returns>
        public Hold Create(IDataReader reader)
        {
            Int64 id = Convert.ToInt64(reader["ID"]);
            string division = Convert.ToString(reader["Division"]);
            string store = Convert.ToString(reader["Store"]);
            string level = Convert.ToString(reader["Level"]);
            string value = Convert.ToString(reader["Value"]);
            DateTime startDate = Convert.ToDateTime(reader["StartDate"]);
            DateTime? endDate
                = Convert.IsDBNull(reader["EndDate"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["EndDate"]));
            char reserveInventoryChar = Convert.ToChar(reader["ReserveInventory"]);
            string comments = Convert.ToString(reader["Comments"]);
            string createdBy = Convert.ToString(reader["CreatedBy"]);
            DateTime? createDate
                = Convert.IsDBNull(reader["CreateDTTM"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["CreateDTTM"]));
            string duration = Convert.ToString(reader["Duration"]);
            string biSKU = Convert.ToString(reader["bisku"]);

            return new Hold(id, division, store, biSKU, level, value, startDate, endDate, reserveInventoryChar, duration
                , comments, createdBy, createDate);
        }
    }
}
