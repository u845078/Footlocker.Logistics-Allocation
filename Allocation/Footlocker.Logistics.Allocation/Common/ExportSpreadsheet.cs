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
        public Excel excelDocument;

        public void WriteHeaderRecord()
        {
            excelDocument = new Excel();

            currentRow = headerRowNumber;

            for (int i = 0; i < maxColumns; i++)            
                excelDocument.Worksheets[0].Cells[headerRowNumber, i].PutValue(columns[i]);

            currentRow++;
        }

        protected ExportSpreadsheet(AppConfig config)
        {
            this.config = config;
            license = new License();
            license.SetLicense(config.AsposeLicenseFile);
        }
    }
}