using System.Collections.Generic;
using System.Linq;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;
using Aspose.Excel;
using Aspose.Cells;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class WSMExtract : ExportSpreadsheet
    {
        readonly QuantumDAO quantumDAO;

        public void WriteData(string sku, bool includeInvalidRecords)
        {
            List<WSM> wsmList;
            
            wsmList = quantumDAO.GetWSMextract(sku, includeInvalidRecords);

            if (wsmList.Count() == 0)
                errorMessage = string.Format("There was no data found for Sku {0}", sku);
            else
            {
                WriteHeaderRecord();

                foreach (WSM w in wsmList)
                {
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 0].PutValue(w.RunDate);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 1].PutValue(w.TargetProduct);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 2].PutValue(w.TargetProductId);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 3].PutValue(w.TargetLocation);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 4].PutValue(w.MatchProduct);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 5].PutValue(w.MatchProductId);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 6].PutValue(w.ProductWeight);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 7].PutValue(w.MatchLocation);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 8].PutValue(w.LocationWeight);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 9].PutValue(w.FinalMatchWeight);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 10].PutValue(w.FinalMatchDemand);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 11].PutValue(w.LastCapturedDemand);
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 12].PutValue(w.StatusCode);

                    currentRow++;

                    if (currentRow >= maxSpreadsheetRows)
                    {
                        AutofitColumns();

                        worksheetNum++;
                        WriteHeaderRecord();
                    }
                }

                AutofitColumns();
            }
        }

        public WSMExtract(AppConfig config, QuantumDAO dao) : base(config)
        {
            maxColumns = 13;

            columns.Add(0, "Run Date");
            columns.Add(1, "Target Product");
            columns.Add(2, "Target Product ID");
            columns.Add(3, "Target Location");
            columns.Add(4, "Match Product");
            columns.Add(5, "Match Product ID");
            columns.Add(6, "Product Weight");
            columns.Add(7, "Match Location");
            columns.Add(8, "Location Weight");
            columns.Add(9, "Final Match Weight");
            columns.Add(10, "Final Match Demand");
            columns.Add(11, "Last Captured Demand");
            columns.Add(12, "Status Code");

            quantumDAO = dao;
        }
    }
}