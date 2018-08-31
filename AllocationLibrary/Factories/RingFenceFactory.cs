using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    /// <summary>
    /// Provides an object representation of a ring fence factory.
    /// </summary>
    public class RingFenceFactory : IBiExtractFactory<RingFence>
    {
        /// <summary>
        /// Create a ring fence.
        /// </summary>
        /// <param name="reader">The data reader containing the reing fence's properties.</param>
        /// <returns>The new ring fence.</returns>
        public RingFence Create(IDataReader reader)
        {
            Int64 id = Convert.ToInt64(reader["ID"]);
            string division //= Convert.ToString(reader["Division"]);
                = Convert.IsDBNull(reader["Division"]) ? String.Empty
                    : Convert.ToString(reader["Division"]);
            string store //= Convert.ToString(reader["Store"]);
                = Convert.IsDBNull(reader["Store"]) ? String.Empty
                    : Convert.ToString(reader["Store"]);
            string sku //= Convert.ToString(reader["Sku"]);
                = Convert.IsDBNull(reader["Sku"]) ? String.Empty
                    : Convert.ToString(reader["Sku"]);
            string size //= Convert.ToString(reader["Size"]);
                = Convert.IsDBNull(reader["Size"]) ? String.Empty
                    : Convert.ToString(reader["Size"]);
            string po //= Convert.ToString(reader["PO"]);
                = Convert.IsDBNull(reader["PO"]) ? String.Empty
                    : Convert.ToString(reader["PO"]);
            Int32 dcid = Convert.ToInt32(reader["DCID"]);
            Int32 qty = Convert.ToInt32(reader["Qty"]);
            Int32 binQty = Convert.ToInt32(reader["BinQty"]);
            Int32 caseQty = Convert.ToInt32(reader["CaseQty"]);
            DateTime startDate = Convert.ToDateTime(reader["StartDate"]);
            DateTime? endDate
                = Convert.IsDBNull(reader["EndDate"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["EndDate"]));
            string createdBy = Convert.ToString(reader["CreatedBy"]);
            DateTime? createDate
                = Convert.IsDBNull(reader["CreateDTTM"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["CreateDTTM"]));

            return new RingFence(id, division, store, sku, size, po, dcid, binQty, caseQty, qty, startDate, endDate, createdBy, createDate
                , 0L, null, null, 1);
        }
    }
}
