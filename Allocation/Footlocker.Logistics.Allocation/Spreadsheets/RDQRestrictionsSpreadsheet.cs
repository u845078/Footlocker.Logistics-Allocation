using Aspose.Cells;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class RDQRestrictionsSpreadsheet : UploadSpreadsheet
    {
        public List<RDQRestriction> validRecs = new List<RDQRestriction>();
        public List<Tuple<RDQRestriction, string>> errorList = new List<Tuple<RDQRestriction, string>>();
        readonly RDQDAO rdqDAO;

        private RDQRestriction ParseRow(DataRow row)
        {
            const string defaultValue = "N/A";

            RDQRestriction returnValue = new RDQRestriction()
            {
                Division = Convert.ToString(row[0]).Trim(),
                Department = Convert.ToString(row[1]).Trim(),
                Category = Convert.ToString(row[2]).Trim(),
                Brand = Convert.ToString(row[3]).Trim(),
                Vendor = Convert.ToString(row[4]).Trim(),
                SKU = Convert.ToString(row[5]).Trim(),
                RDQType = Convert.ToString(row[6]).Trim(),
                FromDate = Convert.ToDateTime(row[7]),   
                ToDate = Convert.ToDateTime(row[8]),
                FromDCCode = Convert.ToString(row[9]).Trim(),
                ToLeague = Convert.ToString(row[10]).Trim(),
                ToRegion = Convert.ToString(row[11]).Trim(),
                ToStore = Convert.ToString(row[12]).Trim(),
                ToDCCode = Convert.ToString(row[13]).Trim(),
                LastModifiedDate = DateTime.Now, 
                LastModifiedUser = config.currentUser.NetworkID
            };

            returnValue.Department = returnValue.Department.Equals(defaultValue) || string.IsNullOrEmpty(returnValue.Department) ? null : returnValue.Department;
            returnValue.Category = returnValue.Category.Equals(defaultValue) || string.IsNullOrEmpty(returnValue.Category) ? null : returnValue.Category;
            returnValue.Brand = returnValue.Brand.Equals(defaultValue) || string.IsNullOrEmpty(returnValue.Brand) ? null : returnValue.Brand;
            returnValue.FromDCCode = returnValue.FromDCCode.Equals(defaultValue) || string.IsNullOrEmpty(returnValue.FromDCCode) ? null : returnValue.FromDCCode;
            returnValue.ToDCCode = returnValue.ToDCCode.Equals(defaultValue) || string.IsNullOrEmpty(returnValue.ToDCCode) ? null : returnValue.ToDCCode;
            returnValue.RDQType = returnValue.RDQType.Equals(defaultValue) || string.IsNullOrEmpty(returnValue.RDQType) ? null : returnValue.RDQType;
            returnValue.Vendor = string.IsNullOrEmpty(returnValue.Vendor) ? null : returnValue.Vendor;
            returnValue.ToLeague = string.IsNullOrEmpty(returnValue.ToLeague) ? null : returnValue.ToLeague;
            returnValue.ToRegion = string.IsNullOrEmpty(returnValue.ToRegion) ? null : returnValue.ToRegion;
            returnValue.ToStore = string.IsNullOrEmpty(returnValue.ToStore) ? null : returnValue.ToStore;

            return returnValue;
        }

        private bool ValidateRow(RDQRestriction inputData, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(inputData.Division))
                errorMessage = "Division must be provided.";
            else
            {
                if (!config.currentUser.HasDivision(inputData.Division))
                    errorMessage = string.Format("You do not have permission for Division {0}", inputData.Division);            
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                if (inputData.FromDate == null || inputData.FromDate == DateTime.MinValue)
                    errorMessage = "The From Date is required.";
                else
                {
                    if (inputData.ToDate == null || inputData.ToDate == DateTime.MinValue)
                        errorMessage = "The To Date is required.";
                    else
                    {
                        if (inputData.FromDate > inputData.ToDate)
                            errorMessage = "The From Date is greater than the To Date";
                    }
                }
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                int existsCounter = 0;
                existsCounter += string.IsNullOrEmpty(inputData.ToLeague) ? 0 : 1;
                existsCounter += string.IsNullOrEmpty(inputData.ToRegion) ? 0 : 1;
                existsCounter += string.IsNullOrEmpty(inputData.ToStore) ? 0 : 1;
                existsCounter += string.IsNullOrEmpty(inputData.ToDCCode) ? 0 : 1;

                if (existsCounter > 1)                
                    errorMessage = "Cannot have more than one of the following properties populated: To League, To Region, To Store, To DC Code.";                
            }

            return string.IsNullOrEmpty(errorMessage);
        }

        private void ValidateList()
        {
            var dupList = validRecs.GroupBy(vr => new { vr.Division, vr.Department, vr.Category, vr.Brand, vr.Vendor, vr.SKU, vr.RDQType, vr.FromDCCode, vr.ToLeague, vr.ToRegion, vr.ToStore, vr.ToDCCode })
                                   .Where(vr => vr.Count() > 1)
                                   .Select(vr => new { DuplicateRF = vr.First(), Counter = vr.Count() })
                                   .ToList();

            foreach (var dup in dupList)
            {
                errorList.Add(Tuple.Create(dup.DuplicateRF, string.Format("The following row of data was duplicated in the spreadsheet {0} times.  Please provide unique rows of data.", dup.Counter)));

                validRecs.RemoveAll(vr => vr.Division == dup.DuplicateRF.Division &&
                                          vr.Department == dup.DuplicateRF.Department &&
                                          vr.Category == dup.DuplicateRF.Category &&
                                          vr.Brand == dup.DuplicateRF.Brand &&
                                          vr.Vendor == dup.DuplicateRF.Vendor &&
                                          vr.SKU == dup.DuplicateRF.SKU &&
                                          vr.RDQType == dup.DuplicateRF.RDQType &&
                                          vr.FromDCCode == dup.DuplicateRF.FromDCCode &&
                                          vr.ToLeague == dup.DuplicateRF.ToLeague &&
                                          vr.ToRegion == dup.DuplicateRF.ToRegion &&
                                          vr.ToStore == dup.DuplicateRF.ToStore &&
                                          vr.ToDCCode == dup.DuplicateRF.ToDCCode);
            }
        }

        private void ProcessBadData(List<RDQRestriction> badData, string errorMessage)
        {
            foreach (var bad in badData)
            {
                errorList.Add(Tuple.Create(bad, errorMessage));

                validRecs.RemoveAll(vr => vr.Division == bad.Division &&
                                          vr.Department == bad.Department &&
                                          vr.Category == bad.Category &&
                                          vr.Brand == bad.Brand &&
                                          vr.Vendor == bad.Vendor &&
                                          vr.SKU == bad.SKU &&
                                          vr.RDQType == bad.RDQType &&
                                          vr.FromDCCode == bad.FromDCCode &&
                                          vr.ToLeague == bad.ToLeague &&
                                          vr.ToRegion == bad.ToRegion &&
                                          vr.ToStore == bad.ToStore &&
                                          vr.ToDCCode == bad.ToDCCode);
            }
        }

        private void ValidateValues()
        {
            List<RDQRestriction> badData;

            List<RDQRestriction> dbRDQRestrictions = config.db.RDQRestrictions.ToList();

            var uniqueDivisions = validRecs.Select(pr => pr.Division).Distinct().ToList();
            List<ItemMaster> dbItemMasters = config.db.ItemMasters.Where(im => uniqueDivisions.Contains(im.Div)).ToList();

            List<RDQRestriction> dupRecs = validRecs.Where(pr => dbRDQRestrictions.Any(r => r.Division == pr.Division &&
                                                                                             (r.Department == null && string.IsNullOrEmpty(pr.Department) || r.Department == pr.Department) &&
                                                                                            ((r.Category == null && string.IsNullOrEmpty(pr.Category)) || r.Category == pr.Category) &&
                                                                                            ((r.Brand == null && string.IsNullOrEmpty(pr.Brand)) || r.Brand == pr.Brand) &&
                                                                                            ((r.SKU == null && string.IsNullOrEmpty(pr.SKU)) || r.SKU == pr.SKU) &&
                                                                                            ((r.RDQType == null && string.IsNullOrEmpty(pr.RDQType)) || (r.RDQType != null && r.RDQType.ToLower() == pr.RDQType.ToLower())) &&
                                                                                            ((r.Vendor == null && string.IsNullOrEmpty(pr.Vendor)) || r.Vendor == pr.Vendor) &&
                                                                                            ((r.FromDCCode == null && string.IsNullOrEmpty(pr.FromDCCode)) || r.FromDCCode == pr.FromDCCode) &&
                                                                                            ((r.ToDCCode == null && string.IsNullOrEmpty(pr.ToDCCode)) || r.ToDCCode == pr.ToDCCode) &&
                                                                                            ((r.ToLeague == null && string.IsNullOrEmpty(pr.ToLeague)) || r.ToLeague == pr.ToLeague) &&
                                                                                            ((r.ToRegion == null && string.IsNullOrEmpty(pr.ToRegion)) || r.ToRegion == pr.ToRegion) &&
                                                                                            ((r.ToStore == null && string.IsNullOrEmpty(pr.ToStore)) || r.ToStore == pr.ToStore)))
                    .ToList();

            ProcessBadData(dupRecs, "The combination provided already exists within the system.");

            // has valid combination ( div / department / category / brand )
            var uniqueCombos = validRecs.Select(pr => new { pr.Division, pr.Department, pr.Category, pr.Brand })
                                        .Distinct()
                                        .ToList();

            // division / department
            var divDeptCombos = uniqueCombos.Where(uc => !string.IsNullOrEmpty(uc.Division) &&
                                                         !string.IsNullOrEmpty(uc.Department) &&
                                                         string.IsNullOrEmpty(uc.Category) &&
                                                         string.IsNullOrEmpty(uc.Brand)).ToList();

            var invalidCombos = divDeptCombos.Where(uc => !dbItemMasters.Any(im => im.Div.Equals(uc.Division) &&
                                                                                   im.Dept.Equals(uc.Department))).ToList();

            badData = validRecs.Where(vr => invalidCombos.Any(ic => ic.Division == vr.Division &&
                                                                    ic.Department == vr.Department)).ToList();

            ProcessBadData(badData, "The division / department combination does not exist.");

            // division / department / category
            var divDeptCatCombos = uniqueCombos.Where(uc => !string.IsNullOrEmpty(uc.Division) &&
                                                            !string.IsNullOrEmpty(uc.Department) &&
                                                            !string.IsNullOrEmpty(uc.Category) &&
                                                            string.IsNullOrEmpty(uc.Brand)).ToList();

            invalidCombos = divDeptCatCombos.Where(uc => !dbItemMasters.Any(im => im.Div.Equals(uc.Division) &&
                                                                                  im.Dept.Equals(uc.Department) &&
                                                                                  im.Category == uc.Category)).ToList();

            badData = validRecs.Where(vr => invalidCombos.Any(ic => ic.Division == vr.Division &&
                                                                    ic.Department == vr.Department &&
                                                                    ic.Category == vr.Category)).ToList();

            ProcessBadData(badData, "The division / department / category combination does not exist.");

            // division / deparment / category / brand
            var divDeptCatBrandCombos = uniqueCombos.Where(uc => !string.IsNullOrEmpty(uc.Division) &&
                                                                 !string.IsNullOrEmpty(uc.Department) &&
                                                                 !string.IsNullOrEmpty(uc.Category) &&
                                                                 !string.IsNullOrEmpty(uc.Brand)).ToList();

            invalidCombos = divDeptCatBrandCombos.Where(uc => !dbItemMasters.Any(im => im.Div.Equals(uc.Division) &&
                                                                                       im.Dept.Equals(uc.Department) &&
                                                                                       im.Category == uc.Category &&
                                                                                       im.Brand == uc.Brand)).ToList();

            badData = validRecs.Where(vr => invalidCombos.Any(ic => ic.Division == vr.Division &&
                                                                    ic.Department == vr.Department &&
                                                                    ic.Category == vr.Category &&
                                                                    ic.Brand == vr.Brand)).ToList();

            ProcessBadData(badData, "The division / department / category / brand combination does not exist.");

            // division / brand combination exists
            var divBrandCombos = uniqueCombos.Where(uc => !string.IsNullOrEmpty(uc.Division) &&
                                                           string.IsNullOrEmpty(uc.Department) &&
                                                           string.IsNullOrEmpty(uc.Category) &&
                                                          !string.IsNullOrEmpty(uc.Brand)).ToList();

            invalidCombos = divBrandCombos.Where(uc => !dbItemMasters.Any(im => im.Div.Equals(uc.Division) &&
                                                                                im.Brand == uc.Brand)).ToList();

            badData = validRecs.Where(vr => invalidCombos.Any(ic => ic.Division == vr.Division &&
                                                                    ic.Brand == vr.Brand)).ToList();

            ProcessBadData(badData, "The division / brand combination does not exist.");

            // division / category combinations
            var divCatCombos = uniqueCombos.Where(uc => !string.IsNullOrEmpty(uc.Division) &&
                                                         string.IsNullOrEmpty(uc.Department) &&
                                                        !string.IsNullOrEmpty(uc.Category) &&
                                                         string.IsNullOrEmpty(uc.Brand)).ToList();

            invalidCombos = divCatCombos.Where(uc => !dbItemMasters.Any(im => im.Div.Equals(uc.Division) &&
                                                                              im.Category == uc.Category)).ToList();

            badData = validRecs.Where(vr => invalidCombos.Any(ic => ic.Division == vr.Division &&
                                                                    ic.Category == vr.Category)).ToList();

            ProcessBadData(badData, "The division / category combination does not exist.");

            // division / vendor combination exists
            var divVendorCombos = validRecs.Where(pr => !string.IsNullOrEmpty(pr.Vendor))
                                           .Select(pr => new { pr.Division, pr.Vendor })
                                           .Distinct();

            var invalidVendors = divVendorCombos.Where(vc => !dbItemMasters.Any(im => im.Div.Equals(vc.Division) &&
                                                                                      im.Vendor == vc.Vendor));

            badData = validRecs.Where(vr => invalidVendors.Any(ic => ic.Division == vr.Division &&
                                                                     ic.Vendor == vr.Vendor)).ToList();

            ProcessBadData(badData, "The division / vendor combination does not exist.");

            // SKU combination exists
            var allSKUData = validRecs.Where(pr => !string.IsNullOrEmpty(pr.SKU))
                                      .Select(pr => new { pr.Division, pr.Vendor, pr.Brand, pr.Category, pr.Department, pr.SKU })
                                      .Distinct()
                                      .ToList();

            var invalidSKUs = allSKUData.Where(s => !dbItemMasters.Any(im => im.Div.Equals(s.Division) &&
                                                                             (im.Vendor == s.Vendor || string.IsNullOrEmpty(s.Vendor)) &&
                                                                             (im.Brand == s.Brand || string.IsNullOrEmpty(s.Brand)) &&
                                                                             (im.Category == s.Category || string.IsNullOrEmpty(s.Category)) &&
                                                                             (im.Dept == s.Department || string.IsNullOrEmpty(s.Department)) &&
                                                                              im.MerchantSku == s.SKU))
                                        .ToList();

            badData = validRecs.Where(vr => invalidSKUs.Any(ic => ic.Division == vr.Division &&
                                                                  ic.Vendor == vr.Vendor &&
                                                                  ic.Brand == vr.Brand &&
                                                                  ic.Category == vr.Category &&
                                                                  ic.Department == vr.Department &&
                                                                  ic.SKU == vr.SKU)).ToList();

            ProcessBadData(badData, "The Div/Vendor/Brand/Category/Department combination does not exist for this SKU.");

            // rdq type exists
            var uniqueRDQTypes = validRecs.Where(pr => !string.IsNullOrEmpty(pr.RDQType))
                                          .Select(pr => pr.RDQType)
                                          .Distinct()
                                          .ToList();

            var invalidRDQTypes = uniqueRDQTypes.Where(urt => !config.db.RDQTypes.Any(rt => rt.RDQTypeName.Equals(urt))).ToList();

            badData = validRecs.Where(vr => invalidRDQTypes.Any(irt => irt == vr.RDQType)).ToList();

            ProcessBadData(badData, "The RDQType does not exist.");

            // from dc code exists
            var uniqueFromDCCodes = validRecs.Where(pr => !string.IsNullOrEmpty(pr.FromDCCode))
                                             .Select(pr => pr.FromDCCode)
                                             .Distinct()
                                             .ToList();

            var invalidFromDCCodes = uniqueFromDCCodes.Where(udc => !config.db.DistributionCenters.Any(dc => dc.MFCode.Equals(udc))).ToList();

            badData = validRecs.Where(vr => invalidFromDCCodes.Any(idc => idc == vr.FromDCCode)).ToList();

            ProcessBadData(badData, "The From DC Code is invalid.");

            // division / to league combination exists
            var uniqueToLeagueCombos = validRecs.Where(pr => !string.IsNullOrEmpty(pr.ToLeague))
                                                .Select(pr => new { pr.Division, pr.ToLeague })
                                                .Distinct()
                                                .ToList();

            var invalidToLeagueCombos = uniqueToLeagueCombos.Where(utl => !config.db.StoreLookups.Any(sl => sl.Division.Equals(utl.Division) &&
                                                                                                            sl.League.Equals(utl.ToLeague))).ToList();

            badData = validRecs.Where(vr => invalidToLeagueCombos.Any(idc => idc.Division == vr.Division &&
                                                                             idc.ToLeague == vr.ToLeague)).ToList();

            ProcessBadData(badData, "The division / to league combination does not exist.");

            // division / to region combination exists
            var uniqueToRegionCombos = validRecs.Where(pr => !string.IsNullOrEmpty(pr.ToRegion))
                                                .Select(pr => new { pr.Division, pr.ToRegion })
                                                .Distinct()
                                                .ToList();

            var invalidToRegionCombos = uniqueToRegionCombos.Where(utr => !config.db.StoreLookups.Any(sl => sl.Division.Equals(utr.Division) &&
                                                                                                            sl.Region.Equals(utr.ToRegion))).ToList();
            
            badData = validRecs.Where(vr => invalidToRegionCombos.Any(idc => idc.Division == vr.Division &&
                                                                             idc.ToRegion == vr.ToRegion)).ToList();

            ProcessBadData(badData, "The division / to region does not exist.");

            // division / to store combination exists
            var uniqueToStoreCombos = validRecs.Where(pr => !string.IsNullOrEmpty(pr.ToStore))
                                               .Select(pr => new { pr.Division, pr.ToStore })
                                               .Distinct()
                                               .ToList();

            var invalidToStoreCombos = uniqueToStoreCombos.Where(uts => !config.db.StoreLookups.Any(sl => sl.Division.Equals(uts.Division) &&
                                                                                                          sl.Store.Equals(uts.ToStore))).ToList();

            badData = validRecs.Where(vr => invalidToStoreCombos.Any(isc => isc.Division == vr.Division &&
                                                                            isc.ToStore == vr.ToStore)).ToList();

            ProcessBadData(badData, "The division / to store combination does not exist.");

            // to dc code exists
            var uniqueToDCCodes = validRecs.Where(pr => !string.IsNullOrEmpty(pr.ToDCCode))
                                           .Select(pr => pr.ToDCCode)
                                           .Distinct()
                                           .ToList();

            var invalidDCCodes = uniqueToDCCodes.Where(udc => !config.db.DistributionCenters.Any(dc => dc.MFCode.Equals(udc))).ToList();

            badData = validRecs.Where(vr => invalidDCCodes.Any(isc => isc == vr.ToDCCode)).ToList();

            ProcessBadData(badData, "The To DC Code does not exist.");
        }

        public void Save(HttpPostedFileBase attachment)
        {
            RDQRestriction uploadRec;

            LoadAttachment(attachment.InputStream);
            if (!HasValidHeaderRow())
                message = "Incorrectly formatted or missing header row. Please correct and re-process.";
            else
            {
                int row = 1;
                try
                {
                    foreach (DataRow dataRow in excelData.Rows)
                    {
                        uploadRec = ParseRow(dataRow);

                        if (!ValidateRow(uploadRec, out errorMessage))                        
                            errorList.Add(Tuple.Create(uploadRec, errorMessage));
                        else
                            validRecs.Add(uploadRec);

                        row++;
                    }

                    if (validRecs.Count > 0)
                        ValidateList();

                    if (validRecs.Count > 0)
                        ValidateValues();

                    if (validRecs.Count > 0)
                        rdqDAO.InsertRDQRestrictions(validRecs, config.currentUser.NetworkID);
                }
                catch (Exception ex)
                {
                    message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
                    FLLogger logger = new FLLogger(config.LogFile);
                    logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                }
            }
        }

        public Workbook GetErrors(List<Tuple<RDQRestriction, string>> errorList)
        {
            if (errorList != null)
            {
                excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (Tuple<RDQRestriction, string> p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.Item1.Division);
                    mySheet.Cells[row, 1].PutValue(p.Item1.Department);
                    mySheet.Cells[row, 2].PutValue(p.Item1.Category);
                    mySheet.Cells[row, 3].PutValue(p.Item1.Brand);
                    mySheet.Cells[row, 4].PutValue(p.Item1.Vendor);
                    mySheet.Cells[row, 5].PutValue(p.Item1.SKU);
                    mySheet.Cells[row, 6].PutValue(p.Item1.RDQType);

                    if (p.Item1.FromDate.Equals(DateTime.MinValue) || p.Item1.FromDate == null)
                        mySheet.Cells[row, 7].PutValue(string.Empty);                    
                    else
                    {
                        mySheet.Cells[row, 7].SetStyle(dateStyle);
                        mySheet.Cells[row, 7].PutValue(p.Item1.FromDate);
                    }

                    if (p.Item1.ToDate.Equals(DateTime.MinValue) || p.Item1.ToDate == null)
                        mySheet.Cells[row, 8].PutValue(string.Empty);
                    else
                    {
                        mySheet.Cells[row, 8].SetStyle(dateStyle);
                        mySheet.Cells[row, 8].PutValue(p.Item1.ToDate);
                    }

                    mySheet.Cells[row, 9].PutValue(p.Item1.FromDCCode);
                    mySheet.Cells[row, 10].PutValue(p.Item1.ToLeague);
                    mySheet.Cells[row, 11].PutValue(p.Item1.ToRegion);
                    mySheet.Cells[row, 12].PutValue(p.Item1.ToStore);
                    mySheet.Cells[row, 13].PutValue(p.Item1.ToDCCode);
                    mySheet.Cells[row, maxColumns].PutValue(p.Item2);
                    mySheet.Cells[row, maxColumns].SetStyle(errorStyle);
                    row++;
                }

                AutofitColumns();

                return excelDocument;
            }
            else
            {
                // if this message is hit that means there was an exception while processing that was not accounted for
                // check the log to see what the exception was
                message = "An unexpected error has occured.  Please try again or contact an administrator.";
                return null;
            }
        }

        public RDQRestrictionsSpreadsheet(AppConfig config, ConfigService configService, RDQDAO rdqDAO) : base(config, configService)
        {
            maxColumns = 14;

            columns.Add(0, "Division");
            columns.Add(1, "Department");
            columns.Add(2, "Category");
            columns.Add(3, "Brand");
            columns.Add(4, "Vendor");
            columns.Add(5, "SKU");
            columns.Add(6, "RDQ Type");
            columns.Add(7, "From Date");
            columns.Add(8, "To Date");
            columns.Add(9, "From DC Code");
            columns.Add(10, "To League");
            columns.Add(11, "To Region");
            columns.Add(12, "To Store");
            columns.Add(13, "To DC Code");

            templateFilename = config.RDQRestrictionsTemplate;
            this.rdqDAO = rdqDAO;
        }
    }
}