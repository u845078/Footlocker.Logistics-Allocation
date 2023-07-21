using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;
using System.Collections.Generic;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class StoreNSSExport : ExportSpreadsheet
    {
        public StoreNSSExport(AppConfig config, NetworkZoneStoreDAO nssStoreDAO) : base(config)
        {
            maxColumns = 11;

            columns.Add(0, "Div");
            columns.Add(1, "Store");
            columns.Add(2, "Region");
            columns.Add(3, "League");
            columns.Add(4, "Mall");
            columns.Add(5, "State");
            columns.Add(6, "City");
            columns.Add(7, "DBA");
            columns.Add(8, "StoreType");
            columns.Add(9, "Climate");
            columns.Add(10, "MarketArea");

        }
    }
}