using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    /// <summary>
    /// Provides an object representation of a store seasonality group factory.
    /// </summary>
    public class StoreSeasonalityGroupFactory : IBiExtractFactory<StoreSeasonalityGroup>
    {
        /// <summary>
        /// Create a store seasonality group.
        /// </summary>
        /// <param name="reader">The data reader containing the store seasonality group's properties.</param>
        /// <returns>The new store seasonality group.</returns>
        public StoreSeasonalityGroup Create(IDataReader reader)
        {
            int id = Convert.ToInt32(reader["ID"]);
            string division = Convert.ToString(reader["Division"]);
            string name = Convert.ToString(reader["Name"]);
            string store = Convert.ToString(reader["Store"]);
            string createdBy = Convert.ToString(reader["CreatedBy"]);
            DateTime createDate = Convert.ToDateTime(reader["CreateDTTM"]);

            return new StoreSeasonalityGroup(id, division, store, name, createdBy, createDate);
        }
    }
}
