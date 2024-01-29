using System.Collections.Generic;
using System.IO;
using Aspose.Cells;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;
using System.Data;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    abstract public class UploadSpreadsheet : BaseSpreadsheet
    {
        public string message = string.Empty;
        public Worksheet worksheet;
        public Dictionary<int, string> columns = new Dictionary<int, string>();
        public int maxRows;
        public ConfigService configService;
        public string errorMessage;
        public int headerRowNumber = 0;
        public DataTable excelData;

        public bool HasValidHeaderRow()
        {
            bool isValid = true;

            for (int i = 0; i < maxColumns; i++)
            {
                if (excelData.Columns[i].ColumnName != columns[i])
                    isValid = false;
            }

            return isValid;
        }

        public void LoadAttachment(Stream file)
        {
            excelDocument = new Workbook(file);
            worksheet = excelDocument.Worksheets[0];
            currentSheet = worksheet;
            maxRows = worksheet.Cells.MaxDataRow;

            ExportTableOptions tableOptions = new ExportTableOptions
            {
                SkipErrorValue = true,
                ExportColumnName = true,
                ExportAsString = true
            };

            excelData = worksheet.Cells.ExportDataTable(0, 0, maxRows + 1, maxColumns, tableOptions);
        }

        protected UploadSpreadsheet(AppConfig config, ConfigService configService) : base(config)
        {
            this.configService = configService;
        }
    }
}