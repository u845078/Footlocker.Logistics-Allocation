using System;
using System.Linq;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class LostSalesExtract : ExportSpreadsheet
    {
        readonly QuantumDAO quantumDAO;

        public void WriteData(string sku)
        {
            LostSalesRequest request;

            DateTime start; //Day 1 of the 14 day span to initialize excel headings
            double weeklySales = 0; //Variable to store prior week lost sales

            request = quantumDAO.GetLostSales(sku);

            if (request.LostSales.Count() == 0)
                errorMessage = string.Format("There was no data found for Sku {0}", sku);
            else
            {
                start = request.BeginDate;
                WriteHeaderRecord(start);

                foreach (LostSalesInstance ls in request.LostSales)
                {
                    //eliminate 'S' and 'division' from location id, then put in store location column
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 0].PutValue(ls.LocationId.Substring(3));

                    //add shoe size from product id to end of sku, then put in sku column
                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 1].PutValue(sku + ls.ProductId.Substring(7));

                    //sum weekly lost sales from daily lost sales array, then put in appropriate column
                    for (int i = 0; i < 7; i++)
                    {
                        weeklySales += ls.DailySales[request.WeeklySalesEndIndex - i];
                    }

                    excelDocument.Worksheets[worksheetNum].Cells[currentRow, 2].PutValue(weeklySales);
                    
                    //reset for the next lostsalesinstance
                    weeklySales = 0;

                    //put daily lost sales in the appropriate day column
                    for (int i = 3; i < maxColumns; i++)
                    {
                        excelDocument.Worksheets[worksheetNum].Cells[currentRow, i].PutValue(ls.DailySales[i]);
                    }

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

        public void WriteHeaderRecord(DateTime start)
        {
            currentRow = headerRowNumber;

            if (worksheetNum > 0)
                excelDocument.Worksheets.Add();

            for (int i = 0; i < 3; i++)
            {
                excelDocument.Worksheets[worksheetNum].Cells[headerRowNumber, i].PutValue(columns[i]);
                excelDocument.Worksheets[worksheetNum].Cells[headerRowNumber, i].SetStyle(headerStyle);
            }

            for (int i = 3; i < maxColumns; i++)
            {                
                excelDocument.Worksheets[worksheetNum].Cells[headerRowNumber, i].PutValue(start.AddDays(i - 3));
                excelDocument.Worksheets[worksheetNum].Cells[headerRowNumber, i].SetStyle(headerDateStyle);
            }

            currentRow++;
        }

        public LostSalesExtract(AppConfig config, QuantumDAO dao) : base(config)
        {
            maxColumns = 17;

            columns.Add(0, "Location Id");
            columns.Add(1, "SKU");
            columns.Add(2, "Prior Week Lost Sales");

            quantumDAO = dao;
        }
    }
}