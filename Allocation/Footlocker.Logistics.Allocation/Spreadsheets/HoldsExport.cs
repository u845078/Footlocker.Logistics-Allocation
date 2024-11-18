using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using System.Collections.Generic;
using System.Linq;
using Telerik.Web.Mvc;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class HoldsExport : ExportSpreadsheet
    {
        public void WriteData(GridCommand settings, string duration)
        {
            WriteHeaderRecord();

            if (string.IsNullOrEmpty(duration))
                duration = "All";

            List<Division> userDivs = config.currentUser.GetUserDivisions();
            List<Hold> holdsList = config.db.Holds.Where(h => h.Duration == duration || duration == "All").ToList();

            IQueryable<Hold> holds = (from a in holdsList
                                      join d in userDivs 
                                      on a.Division equals d.DivCode
                                      select a).AsQueryable();

            if (settings.FilterDescriptors.Any())
                holds = holds.ApplyFilters(settings.FilterDescriptors);

            foreach (Hold h in holds)
            {
                currentSheet = excelDocument.Worksheets[worksheetNum];

                currentSheet.Cells[currentRow, 0].PutValue(h.Division);
                currentSheet.Cells[currentRow, 1].PutValue(h.Store);
                currentSheet.Cells[currentRow, 2].PutValue(h.Level);
                currentSheet.Cells[currentRow, 3].PutValue(h.Value);
                currentSheet.Cells[currentRow, 4].PutValue(h.StartDate);
                currentSheet.Cells[currentRow, 4].SetStyle(dateStyle);

                currentSheet.Cells[currentRow, 5].PutValue(h.EndDate);
                currentSheet.Cells[currentRow, 5].SetStyle(dateStyle);

                currentSheet.Cells[currentRow, 6].PutValue(h.Duration);
                currentSheet.Cells[currentRow, 7].PutValue(h.HoldType);
                currentSheet.Cells[currentRow, 8].PutValue(h.Comments);

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


        public HoldsExport(AppConfig config) : base(config)
        {
            maxColumns = 9;

            columns.Add(0, "Division");
            columns.Add(1, "Store");
            columns.Add(2, "Level");
            columns.Add(3, "Value");
            columns.Add(4, "Start Date");
            columns.Add(5, "End Date");
            columns.Add(6, "Duration");
            columns.Add(7, "Hold Type");
            columns.Add(8, "Comments");
        }
    }
}