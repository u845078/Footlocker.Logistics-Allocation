using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;
using System.Collections.Generic;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class StoreNSSExport : ExportExcelSpreadsheet
    {
        public StoreNSSExport(AppConfig config, NetworkZoneStoreDAO nssStoreDAO) : base(config)
        {
            maxColumns = 22;

            columns.Add(0, "Division");
            columns.Add(1, "Store");
            columns.Add(2, "Rank 1");
            columns.Add(3, "Rank 2");
            columns.Add(4, "Rank 3");
            columns.Add(5, "Rank 4");
            columns.Add(6, "Rank 5");
            columns.Add(7, "Rank 6");
            columns.Add(8, "Rank 7");
            columns.Add(9, "Rank 8");
            columns.Add(10, "Rank 9");
            columns.Add(11, "Rank 10");
            columns.Add(12, "Leadtime 1");
            columns.Add(13, "Leadtime 2");
            columns.Add(14, "Leadtime 3");
            columns.Add(15, "Leadtime 4");
            columns.Add(16, "Leadtime 5");
            columns.Add(17, "Leadtime 6");
            columns.Add(18, "Leadtime 7");
            columns.Add(19, "Leadtime 8");
            columns.Add(20, "Leadtime 9");
            columns.Add(21, "Leadtime 10");
        }
    }
}