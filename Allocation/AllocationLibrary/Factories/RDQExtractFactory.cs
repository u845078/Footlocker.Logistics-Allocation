using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    /// <summary>
    /// Provides an object representation of a requested distribution quantity extract factory.
    /// </summary>
    public class RDQExtractFactory : IBiExtractFactory<RDQExtract>
    {
        /// <summary>
        /// Create a requested distribution quantity extract.
        /// </summary>
        /// <param name="reader">The data reader containing the requested distribution quantity extract's properties.</param>
        /// <returns>The new requested distribution quantity extract.</returns>
        public RDQExtract Create(IDataReader reader)
        {
            Int64 id = Convert.ToInt64(reader["ID"]);
            string division = Convert.ToString(reader["Division"]);
            string store = Convert.ToString(reader["ToStore"]);
            string dcid = Convert.ToString(reader["FromWarehouse"]);
            string po = Convert.ToString(reader["PO"]);
            string sku = Convert.ToString(reader["Sku"]);
            string size = Convert.ToString(reader["Size"]);
            Int32 qty = Convert.ToInt32(reader["Qty"]);
            Int32 binQty = Convert.ToInt32(reader["BinQty"]);
            Int32 caseQty = Convert.ToInt32(reader["CaseQty"]);
            string createdBy = Convert.ToString(reader["CreatedBy"]);
            DateTime? createDate
                = Convert.IsDBNull(reader["CreateDTTM"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["CreateDTTM"]));
            string type = Convert.ToString(reader["Type"]);
            string destinationType = Convert.ToString(reader["DestinationType"]);
            string status = Convert.ToString(reader["Status"]);
            string activeInd = Convert.ToString(reader["ActiveInd"]);
            DateTime? expectedShipDate
                = Convert.IsDBNull(reader["ExpectedShipDate"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["ExpectedShipDate"]));
            DateTime? expectedReceiptDate
                = Convert.IsDBNull(reader["ExpectedReceiptDate"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["ExpectedReceiptDate"]));
            Decimal needQty = Convert.ToDecimal(reader["NeedQty"]);
            Decimal targetQty = Convert.ToDecimal(reader["TargetQty"]);
            Decimal forecastQty = Convert.ToDecimal(reader["ForecastQty"]);
            Decimal? optimalQty = Convert.ToDecimal(reader["OptimalQty"]);
            Decimal userRequestedQty = Convert.ToDecimal(reader["UserRequestedQty"]);
            Decimal? requestedQty = Convert.ToDecimal(reader["RequestedQty"]);

            return new RDQExtract(id, division, store, sku, size, qty, binQty, caseQty, dcid, po, type, destinationType, status, createdBy
                , createDate, activeInd, expectedShipDate, expectedReceiptDate, needQty, targetQty, forecastQty, optimalQty, userRequestedQty
                , requestedQty);
        }
    }
}
