using Footlocker.Logistics.Allocation.Services;
using System.Collections.Generic;
using Footlocker.Logistics.Allocation.Common;
using Aspose.Cells;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    abstract public class ExportSpreadsheet : BaseSpreadsheet
    {
        public string message = string.Empty;
        public Worksheet worksheet;
        public Dictionary<int, string> columns = new Dictionary<int, string>();
        public ConfigService configService;
        public string errorMessage;
        public int headerRowNumber = 0;
        public int currentRow;
        public long recordCount;
        public int maxSpreadsheetRows = 1048576;

        public void WriteHeaderRecord()
        {
            currentRow = headerRowNumber;

            if (worksheetNum > 0)
                excelDocument.Worksheets.Add();

            worksheet = excelDocument.Worksheets[worksheetNum];

            for (int i = 0; i < maxColumns; i++)
            {
                worksheet.Cells[headerRowNumber, i].PutValue(columns[i]);
                worksheet.Cells[headerRowNumber, i].SetStyle(headerStyle);
            }

            currentRow++;
        }

        protected ExportSpreadsheet(AppConfig config) : base(config)
        {
            worksheetNum = 0;
            currentRow = 0;
            recordCount = 0;
        }
    }
}