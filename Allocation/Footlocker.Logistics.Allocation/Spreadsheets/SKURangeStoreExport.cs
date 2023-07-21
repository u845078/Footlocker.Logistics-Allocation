using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;
using System.Collections.Generic;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class SKURangeStoreExport : ExportSpreadsheet
    {
        readonly RangePlanDAO rangeDAO;

        public void WriteData(long planID)
        {
            List<StoreLookup> storeList;

            WriteHeaderRecord();

            storeList = rangeDAO.GetStoreLookupsForPlan(planID);

            foreach (StoreLookup store in storeList)
            {
                currentSheet = excelDocument.Worksheets[worksheetNum];

                currentSheet.Cells[currentRow, 0].PutValue(store.Division);
                currentSheet.Cells[currentRow, 1].PutValue(store.Store);
                currentSheet.Cells[currentRow, 2].PutValue(store.Region);
                currentSheet.Cells[currentRow, 3].PutValue(store.League);
                currentSheet.Cells[currentRow, 4].PutValue(store.Mall);
                currentSheet.Cells[currentRow, 5].PutValue(store.State);
                currentSheet.Cells[currentRow, 6].PutValue(store.City);
                currentSheet.Cells[currentRow, 7].PutValue(store.DBA);
                currentSheet.Cells[currentRow, 8].PutValue(store.StoreType);
                currentSheet.Cells[currentRow, 9].PutValue(store.Climate);
                currentSheet.Cells[currentRow, 10].PutValue(store.MarketArea);

                currentRow++;
                recordCount++;

                if (currentRow >= 65535)
                {
                    AutofitColumns();
                    worksheetNum++;
                    WriteHeaderRecord();
                }
            }

            AutofitColumns();
        }

        public SKURangeStoreExport(AppConfig config, RangePlanDAO rangePlanDAO) : base(config)
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

            rangeDAO = rangePlanDAO;
        }
    }
}