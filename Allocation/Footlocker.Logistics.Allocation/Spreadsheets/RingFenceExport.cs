using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Models.Services;
using Footlocker.Logistics.Allocation.Common;
using System.Collections.Generic;
using System.Linq;
using Telerik.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class RingFenceExport : ExportSpreadsheet
    {
        readonly RingFenceDAO ringFenceDAO;

        public void WriteData(GridCommand settings)
        {
            WriteHeaderRecord();
            List<string> userDivisions = config.currentUser.GetUserDivList();
            IQueryable<ValidRingFence> ringFences = ringFenceDAO.GetValidRingFences(userDivisions);

            if (settings.FilterDescriptors.Any())            
                ringFences = ringFences.ApplyFilters(settings.FilterDescriptors);            

            foreach (ValidRingFence rfStore in ringFences)
            {
                currentSheet = excelDocument.Worksheets[worksheetNum];

                currentSheet.Cells[currentRow, 0].PutValue(rfStore.SKU);
                currentSheet.Cells[currentRow, 1].PutValue(rfStore.Size);
                currentSheet.Cells[currentRow, 2].PutValue(rfStore.Quantity);

                currentSheet.Cells[currentRow, 4].PutValue(rfStore.Store);
                currentSheet.Cells[currentRow, 5].PutValue(rfStore.RingFenceStatus.ringFenceStatusDesc);

                int totalQuantity = 0;
                if (rfStore.Size.Length > 3)
                {
                    int itemPackQty = (from i in config.db.ItemPacks
                                       where i.Name == rfStore.Size
                                       select i.TotalQty).FirstOrDefault();
                    totalQuantity = itemPackQty * rfStore.Quantity;
                }
                else
                    totalQuantity = rfStore.Quantity;

                currentSheet.Cells[currentRow, 6].PutValue(totalQuantity);
                currentSheet.Cells[currentRow, 7].PutValue(rfStore.StartDate.ToShortDateString());

                if (rfStore.EndDate.HasValue)
                    currentSheet.Cells[currentRow, 8].PutValue(rfStore.EndDate.Value.ToShortDateString());

                currentSheet.Cells[currentRow, 9].PutValue(rfStore.PO);

                if (rfStore.DistributionCenter != null)
                    currentSheet.Cells[currentRow, 10].PutValue(rfStore.DistributionCenter.MFCode);
                else
                    currentSheet.Cells[currentRow, 10].PutValue("");

                currentSheet.Cells[currentRow, 11].PutValue(rfStore.CreatedBy);
                currentSheet.Cells[currentRow, 12].PutValue(string.Format("{0} {1}", rfStore.CreateDate.ToShortDateString(),
                    rfStore.CreateDate.ToLongTimeString()));
                currentSheet.Cells[currentRow, 13].PutValue(rfStore.Comments);

                currentRow++;
                recordCount++;

                if (currentRow >= maxSpreadsheetRows)
                {
                    AutofitColumns();

                    worksheetNum++;
                    WriteHeaderRecord();
                }
            }

            AutofitColumns();
        }

        public RingFenceExport(AppConfig config, RingFenceDAO ringFenceDAO) : base(config)
        {
            maxColumns = 14;

            columns.Add(0, "SKU");
            columns.Add(1, "Size");
            columns.Add(2, "Pick Quantity");
            columns.Add(3, "");
            columns.Add(4, "Ring Fence Store");
            columns.Add(5, "Ring Fence Status");
            columns.Add(6, "Quantity");
            columns.Add(7, "Start Date");
            columns.Add(8, "End Date");
            columns.Add(9, "PO");
            columns.Add(10, "Distribution Center");
            columns.Add(11, "Created By");
            columns.Add(12, "Create Date");
            columns.Add(13, "Comments");

            this.ringFenceDAO = ringFenceDAO;
        }
    }
}