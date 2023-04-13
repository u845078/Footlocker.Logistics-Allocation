using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Aspose.Excel;

namespace Footlocker.Logistics.Allocation.Common
{
    abstract public class ExportSpreadsheet
    {
        public AppConfig config;
        readonly License license;
        public string message = string.Empty;
        public Worksheet worksheet;
        public Dictionary<int, string> columns = new Dictionary<int, string>();
        public int maxColumns;
        public ConfigService configService;
        public string errorMessage;        
        public int headerRowNumber = 0;
        public int currentRow;
        public long recordCount;
        public Excel excelDocument;
        public int worksheetNum;

        public void WriteHeaderRecord()
        {
            currentRow = headerRowNumber;

            for (int i = 0; i < maxColumns; i++)            
                excelDocument.Worksheets[worksheetNum].Cells[headerRowNumber, i].PutValue(columns[i]);

            currentRow++;
        }

        protected ExportSpreadsheet(AppConfig config)
        {
            this.config = config;
            license = new License();
            license.SetLicense(config.AsposeLicenseFile);
            excelDocument = new Excel();

            worksheetNum = 0;
            currentRow = 0;
            recordCount = 0;
        }
    }
}