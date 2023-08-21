using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using Footlocker.Common.Services;
using System.Text.RegularExpressions;
using System.Web;
using Footlocker.Logistics.Allocation.Common;
using System.IO;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class ServiceTypeUploadData
    {
        public string SKU { get; set; }
        public string ServiceType { get; set; }
        public string EffectiveDateString { get; set; }
        public string Availability { get; set; }

        public string Division 
        { 
            get
            {
                if (!string.IsNullOrEmpty(SKU))
                    return SKU.Substring(0, 2);
                else
                    return "";
            } 
        }

        public string Department
        {
            get
            {
                if (!string.IsNullOrEmpty(SKU))
                {
                    string[] tokens = SKU.Split('-');
                    return tokens[1];
                }
                else
                    return "";
            }
        }

        public string StockNumber
        {
            get
            {
                if (!string.IsNullOrEmpty(SKU))
                {
                    string[] tokens = SKU.Split('-');
                    return tokens[2];
                }
                else
                    return "";
            }
        }

        public string Width
        {
            get
            {
                if (!string.IsNullOrEmpty(SKU))
                {
                    string[] tokens = SKU.Split('-');
                    return tokens[3];
                }
                else
                    return "";
            }
        }

        public string UserID { get; set; }

        public string GetServiceTypeDataString()
        {
            return string.Format("SRVTY{0}{1}{2}{3}{4}{5}{6}{7}{8}", Division, Department, StockNumber, Width.PadRight(30, ' '), EffectiveDateString, 
                ServiceType.PadRight(50, ' '), DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.ffffff"), UserID.PadRight(30, ' '), "".PadRight(9, ' '));
        }

        public string GetAvailabilityDataString()
        {
            return string.Format("SKUAV{0}{1}{2}{3}{4}{5}{6}{7}", Division, Department, StockNumber, Width.PadRight(30, ' '), 
                Availability.PadRight(60, ' '), DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.ffffff"), UserID.PadRight(30, ' '), "".PadRight(9, ' '));
        }
    }

    public class ServiceTypeSpreadsheet : UploadSpreadsheet
    {
        readonly MainframeDAO mainframeDAO;
        string mainDivision;
        string ftpFileName;

        private ServiceTypeUploadData ParseRow(int row)
        {
            message = string.Empty;

            ServiceTypeUploadData returnValue = new ServiceTypeUploadData()
            {
                SKU = Convert.ToString(worksheet.Cells[row, 0].Value),
                ServiceType = Convert.ToString(worksheet.Cells[row, 1].Value),
                EffectiveDateString = Convert.ToString(worksheet.Cells[row, 2].Value), 
                Availability = Convert.ToString(worksheet.Cells[row, 3].Value),
                UserID = config.currentUser.NetworkID
            };

            if (string.IsNullOrEmpty(returnValue.ServiceType))
                returnValue.ServiceType = " ";

            try
            {
                if (!string.IsNullOrEmpty(returnValue.EffectiveDateString))
                    returnValue.EffectiveDateString = Convert.ToDateTime(returnValue.EffectiveDateString).ToString("yyyy-MM-dd");
                else
                    returnValue.EffectiveDateString = DateTime.Now.ToString("yyyy-MM-dd");
            }
            catch
            { 
                errorMessage = string.Format("Effective Date {0} is not a valid date", returnValue.EffectiveDateString);
            }

            return returnValue;
        }

        private string ValidateUploadValues(ServiceTypeUploadData parsedRec)
        {
            string errorMessage = string.Empty;

            if (string.IsNullOrEmpty(parsedRec.SKU))
                errorMessage = "SKU was empty and is a required value";
            else
            {
                Regex levelExpression = new Regex(@"^\d{2}-\d{2}-\d{5}-\d{2}$");
                if (!levelExpression.IsMatch(parsedRec.SKU))
                    errorMessage = "SKU is in an invalid format. It must be ##-##-#####-##";
                else
                    if (!config.currentUser.HasDivision(config.AppName, parsedRec.Division))
                        errorMessage = string.Format("You do not have permission to update division {0}", parsedRec.Division);
            }

            return errorMessage;
        }

        private string ValidateList(List<ServiceTypeUploadData> parsedRows)
        {
            string errorMessage = string.Empty;
            foreach (ServiceTypeUploadData rec in parsedRows)
            {
                if (rec.Division != mainDivision)
                    errorMessage = "Spreadsheet must be for one division only";
            }

            return errorMessage;
        }

        public void Save(HttpPostedFileBase attachment)
        {
            List<ServiceTypeUploadData> validData = new List<ServiceTypeUploadData>();
            string availabilityCodes;
            ServiceTypeUploadData uploadRec;
            ftpFileName = string.Format("{0}_{1}.txt", config.SkuTypeFile, DateTime.Now.ToString("yyyyMMdd_HHmmssffffff"));
            TextWriter txtWrite;

            LoadAttachment(attachment);
            if (!HasValidHeaderRow())
                message = "Incorrectly formatted or missing header row. Please correct and re-process.";
            else
            {
                int row = 1;
                mainDivision = Convert.ToString(worksheet.Cells[row, 0].Value).Substring(0, 2);
                availabilityCodes = mainframeDAO.GetAvailabityCodes(mainDivision);

                try
                {
                    while (HasDataOnRow(row))
                    {
                        uploadRec = ParseRow(row);

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            message = string.Format("Row {0}: {1}", row, errorMessage);
                            return;
                        }                            

                        errorMessage = ValidateUploadValues(uploadRec);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            message = string.Format("Row {0}: {1}", row, errorMessage);
                            return;
                        }
                        else
                            validData.Add(uploadRec);

                        row++;
                    }

                    if (validData.Count > 0)
                        errorMessage = ValidateList(validData);

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        message = errorMessage;
                        return;
                    }

                    txtWrite = new StreamWriter(ftpFileName);

                    foreach (ServiceTypeUploadData rec in validData)
                    {
                        txtWrite.WriteLine(rec.GetServiceTypeDataString());

                        if (!string.IsNullOrEmpty(rec.Availability))
                        {
                            if (availabilityCodes.Contains(rec.Availability))
                                txtWrite.WriteLine(rec.GetAvailabilityDataString());
                        }
                    }
                    
                    txtWrite.Flush();
                    txtWrite.Close();

                    if (config.EnableFTP)                    
                        FTPFile();
                    
                    File.Delete(ftpFileName);
                }
                catch (Exception ex)
                {
                    message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
                    FLLogger logger = new FLLogger(config.LogFile);
                    logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                }
            }
        }

        private void FTPFile()
        {
            int failCount = 1;
            string datasetName;

            while (failCount < 5)
            {
                try
                {
                    FTPService ftp = new FTPService(config.FTPServer, config.FTPUserName, config.FTPPassword);

                    if (config.EuropeDivisions.Contains(mainDivision))
                        datasetName = config.SKUTypeDatasetEurope;
                    else
                        datasetName = config.SKUTypeDataset;

                    ftp.FTPSToMainframe(ftpFileName, datasetName, 0, 0, config.QuoteFTPCommand);
                    ftp.Disconnect();
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

        public ServiceTypeSpreadsheet(AppConfig config, ConfigService configService, MainframeDAO mfDAO) : base(config, configService)
        {
            maxColumns = 4;
            headerRowNumber = 0;

            columns.Add(0, "SKU");
            columns.Add(1, "Type");
            columns.Add(2, "Effective Date");
            columns.Add(3, "Availability");

            templateFilename = config.SkuTypeTemplate;
            mainframeDAO = mfDAO;
        }
    }
}