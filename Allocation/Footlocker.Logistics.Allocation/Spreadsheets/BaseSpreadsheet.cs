using System.Drawing;
using System.IO;
using Aspose.Cells;
using Footlocker.Logistics.Allocation.Common;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    abstract public class BaseSpreadsheet
    {
        public string templateFilename;
        public AppConfig config;
        readonly License license;
        public Workbook excelDocument;
        public Worksheet currentSheet;
        public OoxmlSaveOptions SaveOptions;
        public Style headerStyle;
        public Style dateStyle;
        public Style errorStyle;

        public Workbook GetTemplate()
        {            
            excelDocument = new Workbook(System.Web.HttpContext.Current.Server.MapPath(templateFilename));

            return excelDocument;
        }

        public BaseSpreadsheet(AppConfig config)
        {
            this.config = config;

            license = new License();
            license.SetLicense(config.AsposeCellsLicenseFile);
            SaveOptions = new OoxmlSaveOptions(SaveFormat.Xlsx);
            excelDocument = new Workbook();
            
            headerStyle = excelDocument.CreateStyle();
            headerStyle.Font.IsBold = true;
            headerStyle.Font.Underline = FontUnderlineType.Single;

            dateStyle = excelDocument.CreateStyle();
            dateStyle.Number = 14;

            errorStyle = excelDocument.CreateStyle();
            errorStyle.Font.Color = Color.Red;
        }

        public void VerifyWritableDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public string GetDirectory(string fullFileName)
        {
            return fullFileName.Substring(0, fullFileName.LastIndexOf("\\") + 1);
        }
    }
}