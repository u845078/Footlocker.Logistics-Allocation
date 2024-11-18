using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using Footlocker.Common.Services;
using System.Web;
using Footlocker.Logistics.Allocation.Common;
using System.IO;
using System.Linq;
using System.Data;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class SKUIDRecord
    {
        private int ASCIINumericThreshold { get { return 127; } }

        public string SKU { get; set; }
        public char RawSKUID1 { get; set; }
        public char RawSKUID2 { get; set; }
        public char RawSKUID3 { get; set; }
        public char RawSKUID4 { get; set; }
        public char RawSKUID5 { get; set; }
        public DateTime CurrentDate { get; set; }
        public string UserID { get; set; }

        public string SKUID1 
        {
            get 
            {
                return (RawSKUID1 > ASCIINumericThreshold) ? " " : RawSKUID1.ToString().ToUpper();
            }
        }
        public string SKUID2
        {
            get
            {
                return (RawSKUID2 > ASCIINumericThreshold) ? " " : RawSKUID2.ToString().ToUpper();
            }
        }
        public string SKUID3
        {
            get
            {
                return (RawSKUID3 > ASCIINumericThreshold) ? " " : RawSKUID3.ToString().ToUpper();
            }
        }
        public string SKUID4
        {
            get
            {
                return (RawSKUID4 > ASCIINumericThreshold) ? " " : RawSKUID4.ToString().ToUpper();
            }
        }
        public string SKUID5
        {
            get
            {
                return (RawSKUID5 > ASCIINumericThreshold) ? " " : RawSKUID5.ToString().ToUpper();
            }
        }

        public string Division
        {
            get
            {
                if (string.IsNullOrEmpty(SKU))
                    return string.Empty;
                else
                    return SKU.Split('-')[0].PadLeft(2, '0').Substring(0, 2).ToUpper(); 
            }
        }

        public string Department
        {
            get
            {
                if (string.IsNullOrEmpty(SKU))
                    return string.Empty;
                else
                    return SKU.Split('-')[1].PadLeft(2, '0').Substring(0, 2).ToUpper();
            }
        }

        public string Stock
        {
            get
            {
                if (string.IsNullOrEmpty(SKU))
                    return string.Empty;
                else
                    return SKU.Split('-')[2].PadLeft(5, '0').Substring(0, 5).ToUpper();
            }
        }

        public string WidthColor
        {
            get
            {
                if (string.IsNullOrEmpty(SKU))
                    return string.Empty;
                else
                    return SKU.Split('-')[3].PadLeft(2, '0').Substring(0, 2).ToUpper();
            }
        }

        public string GetRecordAsString()
        {
            string output = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}", Division, Department, Stock, WidthColor, SKUID1, SKUID2, SKUID3, SKUID4, SKUID5, CurrentDate.ToString("yyyy-MM-dd-HH.mm.ss.ffffff"), UserID, string.Empty.PadLeft(18, ' '));

            return output;
        }
    }

    public class SKUIDSpreadsheet : UploadSpreadsheet
    {
        string ftpFileName;
        string mainDivision;
        string authDivs;
        List<SKUIDRecord> validSKUIDs = new List<SKUIDRecord>();

        public void ValidateSheet()
        {
            if (!authDivs.Split(',').Contains(mainDivision))
                message = string.Format("Unauthorized division specified in spreadsheet, Division {0}. Please read instructions above for the authorized divisions.", mainDivision);
            else
                if (!config.currentUser.HasDivision(mainDivision))            
                    message = "You do not have permission to update this division.";            
        }

        private SKUIDRecord ParseRow(DataRow row)
        {
            SKUIDRecord record = new SKUIDRecord()
            {
                SKU = Convert.ToString(row[0]),
                RawSKUID1 = Convert.ToString(row[1]).PadLeft(1, ' ')[0],
                RawSKUID2 = Convert.ToString(row[2]).PadLeft(1, ' ')[0],
                RawSKUID3 = Convert.ToString(row[3]).PadLeft(1, ' ')[0],
                RawSKUID4 = Convert.ToString(row[4]).PadLeft(1, ' ')[0],
                RawSKUID5 = Convert.ToString(row[5]).PadLeft(1, ' ')[0],
                UserID = config.currentUser.NetworkID,
                CurrentDate = DateTime.Now
            };

            return record;
        }

        private void ValidateRec(SKUIDRecord record)
        {
            if (record.Division != mainDivision)
                message = "Spreadsheet must be for one division only.";
            else
            {
                if (record.SKUID1.Contains('|') || record.SKUID2.Contains('|') || record.SKUID3.Contains('|') || record.SKUID4.Contains('|') || record.SKUID5.Contains('|'))
                    message = "Spreadsheet must not contain any '|' characters as Sku Id values.";
            }
        }

        private void FTPFile()
        {
            int failCount = 1;
            string datasetName;
            bool completed = false;

            while (failCount < 5 && !completed)
            {
                try
                {
                    FTPService ftp = new FTPService(config.FTPServer, config.SKUIDFTPUserName, config.SKUIDFTPPassword);

                    if (config.EuropeDivisions.Contains(mainDivision))
                        datasetName = config.SKUIDDatasetEurope;
                    else
                        datasetName = config.SKUIDDataset;

                    ftp.FTPSToMainframe(ftpFileName, datasetName, 0, 0, config.SKUIDQuoteFTPCommand);
                    ftp.Disconnect();
                    completed = true;
                }
                catch (Exception ex)
                {
                    failCount++;
                    if (failCount == 5)
                    {
                        FLLogger logger = new FLLogger(config.LogFile);
                        logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                        message = ex.Message;
                    }
                }
            }
        }

        public void Save(HttpPostedFileBase attachment)
        {
            string filePath;
            TextWriter txtWrite;

            ftpFileName = string.Format("{0}_{1}.txt", config.SKUIDFile, DateTime.Now.ToString("yyyyMMdd_HHmmssffffff"));
            filePath = GetDirectory(ftpFileName);
            VerifyWritableDirectory(filePath);

            LoadAttachment(attachment.InputStream);
            if (!HasValidHeaderRow())
                message = "Incorrectly formatted or missing header row. Please correct and re-process.";
            else
            {
                int row = 1;
                mainDivision = Convert.ToString(worksheet.Cells[row, 0].Value).Substring(0, 2);

                try
                {
                    authDivs = configService.GetValue(1, "SKUID_UPLOAD_AUTHORIZED_DIVS");
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }

                if (string.IsNullOrEmpty(message))
                    ValidateSheet();

                if (string.IsNullOrEmpty(message))
                {
                    foreach (DataRow dataRow in excelData.Rows)
                    {
                        SKUIDRecord rec = ParseRow(dataRow);
                        ValidateRec(rec);

                        if (string.IsNullOrEmpty(message))
                            validSKUIDs.Add(rec);

                        row++;
                    }

                    if (string.IsNullOrEmpty(message))
                    {                        
                        txtWrite = new StreamWriter(ftpFileName);

                        foreach (SKUIDRecord rec in validSKUIDs)                        
                            txtWrite.WriteLine(rec.GetRecordAsString());                        

                        txtWrite.Flush();
                        txtWrite.Close();

                        if (config.EnableFTP)
                            FTPFile();

                        File.Delete(ftpFileName);
                    }
                }
            }
        }

        public SKUIDSpreadsheet(AppConfig config, ConfigService configService) : base(config, configService)
        {
            maxColumns = 6;
            headerRowNumber = 0;

            columns.Add(0, "SKU");
            columns.Add(1, "SKU ID Code 1");
            columns.Add(2, "SKU ID Code 2");
            columns.Add(3, "SKU ID Code 3");
            columns.Add(4, "SKU ID Code 4");
            columns.Add(5, "SKU ID Code 5");

            templateFilename = config.SKUIDUploadTemplate;
        }
    }
}