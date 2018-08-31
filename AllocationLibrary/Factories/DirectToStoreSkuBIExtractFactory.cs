using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    /// <summary>
    /// Provides an object representation of a direct to store stock keeping unit business intelligence extract factory.
    /// </summary>
    public class DirectToStoreSkuBIExtractFactory : IBiExtractFactory<DirectToStoreSkuBIExtract>
    {
        /// <summary>
        /// Create a direct to store stock keeping unit business intelligence extract.
        /// </summary>
        /// <param name="reader">The data reader containing the direct to store stock keeping unit business intelligence extract's properties.</param>
        /// <returns>The new direct to store stock keeping unit business intelligence extract.</returns>
        public DirectToStoreSkuBIExtract Create(IDataReader reader)
        {
            string sku = Convert.ToString(reader["Sku"]);
            string vendor = Convert.ToString(reader["Vendor"]);
            DateTime startDate = Convert.ToDateTime(reader["StartDate"]);
            DateTime? endDate
                = Convert.IsDBNull(reader["EndDate"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["EndDate"]));
            int vendorPackQty = Convert.ToInt32(reader["VendorPackQty"]);
            string orderSun = Convert.ToString(reader["OrderSun"]);
            string orderMon = Convert.ToString(reader["OrderMon"]);
            string orderTue = Convert.ToString(reader["OrderTue"]);
            string orderWed = Convert.ToString(reader["OrderWed"]);
            string orderThur = Convert.ToString(reader["OrderThur"]);
            string orderFri = Convert.ToString(reader["OrderFri"]);
            string orderSat = Convert.ToString(reader["OrderSat"]);
            string createdBy = Convert.ToString(reader["CreatedBy"]);
            DateTime? createDate
                = Convert.IsDBNull(reader["CreateDTTM"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["CreateDTTM"]));

            return new DirectToStoreSkuBIExtract(sku, vendor, startDate, endDate, vendorPackQty, orderSun, orderMon
                , orderTue, orderWed, orderThur, orderFri, orderSat, createdBy, createDate);
        }
    }
}
