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
        public Style headerDateStyle;
        public Style boxStyle;
        public int maxColumns;
        public int worksheetNum;

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

            headerDateStyle = excelDocument.CreateStyle();
            headerDateStyle.Font.IsBold = true;
            headerDateStyle.Font.Underline = FontUnderlineType.Single;
            headerDateStyle.Number = 14;

            errorStyle = excelDocument.CreateStyle();
            errorStyle.Font.Color = Color.Red;

            boxStyle = excelDocument.CreateStyle();
            boxStyle.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
            boxStyle.Borders[BorderType.BottomBorder].Color = Color.Black;
            boxStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
            boxStyle.Borders[BorderType.TopBorder].Color = Color.Black;
            boxStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
            boxStyle.Borders[BorderType.LeftBorder].Color = Color.Black;            
            boxStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
            boxStyle.Borders[BorderType.RightBorder].Color = Color.Black;
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

        public void AutofitColumns()
        {
            for (int i = 0; i <= maxColumns; i++)
            {
                excelDocument.Worksheets[worksheetNum].AutoFitColumn(i);
            }
        }
    }
}