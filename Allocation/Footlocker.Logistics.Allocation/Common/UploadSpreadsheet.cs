using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Aspose.Excel;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Services;

namespace Footlocker.Logistics.Allocation.Common
{
    abstract public class UploadSpreadsheet
    {
        public AppConfig config;
        readonly License license;
        public string message = string.Empty;
        public Worksheet worksheet;
        public Dictionary<int, string> columns = new Dictionary<int, string>();
        public int maxColumns;
        public ConfigService configService;
        public string errorMessage;
        public string templateFilename;

        public bool HasValidHeaderRow()
        {
            bool isValid = true;

            for (int i = 0; i < maxColumns; i++)
            {
                if (Convert.ToString(worksheet.Cells[0, i].Value) != columns[i])
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

        public Excel GetTemplate()
        {
            Excel excelDocument = new Excel();
            FileStream file = new FileStream(config.AppPath + templateFilename, FileMode.Open, FileAccess.Read);

            byte[] data1 = new byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);
            return excelDocument;
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

        protected UploadSpreadsheet(AppConfig config, ConfigService configService)
        {
            this.config = config;
            this.configService = configService;
            license = new License();
            license.SetLicense(config.AsposeLicenseFile);
        }
    }
}