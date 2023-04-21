using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Models.Services;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Common
{
    public class SKURangeExport : ExportSpreadsheet
    {
        readonly RangePlanDetailDAO rangePlanDAO;

        public void WriteData(string sku)
        {
            List<BulkRange> outputList;

            excelDocument = GetTemplate();

            outputList = rangePlanDAO.GetBulkRangesForSku(sku);
            currentRow = 1;

            foreach (BulkRange range in outputList)
            {
                currentSheet = excelDocument.Worksheets[worksheetNum];

                currentSheet.Cells[currentRow, 0].PutValue(range.Division);
                currentSheet.Cells[currentRow, 1].PutValue(range.League);
                currentSheet.Cells[currentRow, 2].PutValue(range.Region);
                currentSheet.Cells[currentRow, 3].PutValue(range.Store);
                currentSheet.Cells[currentRow, 4].PutValue(range.Sku);
                currentSheet.Cells[currentRow, 5].PutValue(range.Size);
                currentSheet.Cells[currentRow, 6].PutValue(range.RangeStartDate);
                currentSheet.Cells[currentRow, 7].PutValue(range.DeliveryGroupName);
                currentSheet.Cells[currentRow, 8].PutValue(range.Min);
                currentSheet.Cells[currentRow, 9].PutValue(range.Max);
                currentSheet.Cells[currentRow, 10].PutValue(range.BaseDemand);
                currentSheet.Cells[currentRow, 11].PutValue(range.MinEndDaysOverride);
                currentSheet.Cells[currentRow, 12].PutValue(range.EndDate);

                currentRow++;
            }
        }

        public SKURangeExport(AppConfig config, RangePlanDetailDAO rangePlanDetailDAO) : base(config)
        {
            maxColumns = 13;

            templateFilename = config.RangeTemplate;
            rangePlanDAO = rangePlanDetailDAO;
        }
    }
}