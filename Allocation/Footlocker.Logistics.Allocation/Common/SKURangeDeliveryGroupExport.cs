using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Common
{
    public class SKURangeDeliveryGroupExport : ExportSpreadsheet
    {
        readonly RangePlanDetailDAO rangePlanDAO;
        public string SKU;
        public string DGName;

        public void WriteData(int deliveryGroupID)
        {
            List<BulkRange> rangePlanList;
            List<BulkRange> outputList;

            excelDocument = GetTemplate();

            // retrieve specific delivery group
            DeliveryGroup dg = config.db.DeliveryGroups.Where(d => d.ID == deliveryGroupID).FirstOrDefault();
            DGName = dg.Name;

            // retrieve sku for delivery group to feed into stored procedure
            SKU = config.db.RangePlans.Where(rp => rp.Id == dg.PlanID).Select(rp => rp.Sku).FirstOrDefault();

            rangePlanList = rangePlanDAO.GetBulkRangesForSku(SKU);

            outputList = rangePlanList.Where(q => q.DeliveryGroupName == DGName)
                                      .OrderBy(br => br.Division).ThenBy(br => br.Store).ThenBy(br => br.Size)
                                      .ToList();

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
            
            AutofitColumns();
        }

        public SKURangeDeliveryGroupExport(AppConfig config, RangePlanDetailDAO rangePlanDetailDAO) : base(config)
        {
            maxColumns = 13;

            templateFilename = config.RangeTemplate;
            rangePlanDAO = rangePlanDetailDAO;
        }
    }
}