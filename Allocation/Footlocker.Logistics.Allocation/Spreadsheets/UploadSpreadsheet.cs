using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Aspose.Excel;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;

namespace Footlocker.Logistics.Allocation.Spreadsheet
{
    abstract public class UploadSpreadsheet : BaseSpreadsheet
    {
        public string message = string.Empty;
        public Worksheet worksheet;
        public Dictionary<int, string> columns = new Dictionary<int, string>();
        public int maxColumns;
        public ConfigService configService;
        public string errorMessage;
        public int headerRowNumber = 0;

        public bool HasValidHeaderRow()
        {
            bool isValid = true;

            for (int i = 0; i < maxColumns; i++)
            {
                if (Convert.ToString(worksheet.Cells[headerRowNumber, i].Value) != columns[i])
                    isValid = false;
            }

            return isValid;
        }

        public bool HasDataOnRow(int row)
        {
            bool hasData = false;

            for (int i = 0; i < maxColumns; i++)
            {
                if (worksheet.Cells[row, i].Value != null)
                    hasData = true;
            }

            return hasData;
        }

        public void LoadAttachment(HttpPostedFileBase attachment)
        {
            Excel workbook = new Excel();
            byte[] data1 = new byte[attachment.InputStream.Length];
            attachment.InputStream.Read(data1, 0, data1.Length);
            attachment.InputStream.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            workbook.Open(memoryStream1);
            worksheet = workbook.Worksheets[0];
        }

        protected UploadSpreadsheet(AppConfig config, ConfigService configService) : base(config)
        {            
            this.configService = configService;
        }
    }
}