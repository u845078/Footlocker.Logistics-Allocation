using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using Footlocker.Logistics.Allocation.Common;
using System.Web;
using Aspose.Excel;

namespace Footlocker.Logistics.Allocation.Spreadsheet
{
    abstract public class ExportSpreadsheet : BaseSpreadsheet
    {
        public string message = string.Empty;
        public Worksheet worksheet;
        public Dictionary<int, string> columns = new Dictionary<int, string>();
        public int maxColumns;
        public ConfigService configService;
        public string errorMessage;        
        public int headerRowNumber = 0;
        public int currentRow;
        public long recordCount;

        public int worksheetNum;

        public void WriteHeaderRecord()
        {
            currentRow = headerRowNumber;

            if (worksheetNum > 0)
                excelDocument.Worksheets.Add();

            for (int i = 0; i < maxColumns; i++)
            {
                excelDocument.Worksheets[worksheetNum].Cells[headerRowNumber, i].PutValue(columns[i]);
                excelDocument.Worksheets[worksheetNum].Cells[headerRowNumber, i].Style.Font.IsBold = true;
            }                          

            currentRow++;
        }

        public void AutofitColumns()
        {
            for (int i = 0; i < maxColumns; i++)
            {
                excelDocument.Worksheets[worksheetNum].AutoFitColumn(i);
            }
        }

        protected ExportSpreadsheet(AppConfig config) : base (config) 
        {            
            worksheetNum = 0;
            currentRow = 0;
            recordCount = 0;
        }
    }
}