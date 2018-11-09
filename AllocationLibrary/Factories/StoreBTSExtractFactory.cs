using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    /// <summary>
    /// Provides an object representation of a store back to school extract factory.
    /// </summary>
    public class StoreBTSExtractFactory : IBiExtractFactory<StoreBTSExtract>
    {
        /// <summary>
        /// Create a store back to school extract.
        /// </summary>
        /// <param name="reader">The data reader containing the store back to school extract's properties.</param>
        /// <returns>The new store back to school extract.</returns>
        public StoreBTSExtract Create(IDataReader reader)
        {
            int id = Convert.ToInt32(reader["ID"]);
            string division = Convert.ToString(reader["Division"]);
            string store = Convert.ToString(reader["Store"]);
            string name = Convert.ToString(reader["Name"]);
            int count = Convert.ToInt32(reader["Count"]);
            int year = Convert.ToInt32(reader["Year"]);
            string tyLy = Convert.ToString(reader["TY/LY"]);
            string createdBy = Convert.ToString(reader["CreatedBy"]);
            DateTime createDate = Convert.ToDateTime(reader["CreateDTTM"]);
            int ty = Convert.ToInt32(reader["TY"]);

            return new StoreBTSExtract(id, division, store, name, year, tyLy, count, createdBy, createDate, ty);
        }
    }
}
