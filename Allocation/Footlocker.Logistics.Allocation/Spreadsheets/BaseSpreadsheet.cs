using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Aspose.Excel;
using Footlocker.Logistics.Allocation.Common;

namespace Footlocker.Logistics.Allocation.Spreadsheet
{
    abstract public class BaseSpreadsheet
    {
        public string templateFilename;
        public AppConfig config;
        readonly License license;
        public Excel excelDocument;
        public Worksheet currentSheet;

        public Excel GetTemplate()
        {
            FileStream file = new FileStream(config.AppPath + templateFilename, FileMode.Open, FileAccess.Read);

            byte[] data1 = new byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);
            return excelDocument;
        }

        public BaseSpreadsheet(AppConfig config)
        {
            this.config = config;

            license = new License();
            license.SetLicense(config.AsposeLicenseFile);
            excelDocument = new Excel();
        }
    }
}