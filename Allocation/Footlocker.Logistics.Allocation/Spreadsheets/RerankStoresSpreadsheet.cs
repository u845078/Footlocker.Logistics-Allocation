using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Drawing;
using Aspose.Cells;
using System.Text.RegularExpressions;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class RerankStoresSpreadsheet : UploadSpreadsheet
    {
        public List<NSSUpload> validData = new List<NSSUpload>();
        public List<NSSUpload> errorList = new List<NSSUpload>();
        List<DistributionCenter> DCs;
        private readonly RangePlanDetailDAO rangePlanDetailDAO;
        private readonly NetworkZoneStoreDAO networkZoneStoreDAO;

        private NSSUpload ParseRow(DataRow row)
        {
            NSSUpload newItem = new NSSUpload()
            {
                SubmittedDivision = row[0].ToString().PadLeft(2, '0'),
                SubmittedStore = row[1].ToString().PadLeft(5, '0')
            };

            for (int i = 0; i < newItem.MaxValues; i++)
            {
                newItem.SubmittedRank.Add(row[i + 2].ToString().Trim());
                newItem.SubmittedLeadtime.Add(row[i + newItem.MaxValues + 2].ToString().Trim());
            }

            return newItem;
        }

        private void ValidateRow(NSSUpload item)
        {
            string TwoDigitPattern = @"^[0-9]{2}$";
            string FiveDigitPattern = @"^[0-9]{5}$";
            string NumberPattern = @"^[0-9]+$";

            Regex TwoDigitRegex = new Regex(TwoDigitPattern);
            Regex FiveDigitRegex = new Regex(FiveDigitPattern);
            Regex NumberRegex = new Regex(NumberPattern);

            if (TwoDigitRegex.IsMatch(item.SubmittedDivision))
                item.Division = item.SubmittedDivision;
            else
                item.ErrorMessage = "Error - Division does not look valid";

            if (string.IsNullOrEmpty(item.ErrorMessage))
            {
                if (FiveDigitRegex.IsMatch(item.SubmittedStore))
                    item.Store = item.SubmittedStore;
                else
                    item.ErrorMessage = "Error - Store does not look valid";
            }

            if (string.IsNullOrEmpty(item.ErrorMessage))
            {                
                for (int i = 0; i < item.MaxValues; i++)
                {
                    if (!string.IsNullOrEmpty(item.SubmittedRank[i]))
                    {
                        if (TwoDigitRegex.IsMatch(item.SubmittedRank[i]))
                        {
                            if (DCs.Where(d => d.MFCode == item.SubmittedRank[i]).Count() == 0)
                                item.ErrorMessage = string.Format("Error - {0} is not a valid DC", item.SubmittedRank[i]);
                            else
                                item.DCIDList.Add(DCs.Where(d => d.MFCode == item.SubmittedRank[i]).FirstOrDefault().ID);
                        }
                        else
                            item.ErrorMessage = string.Format("Error - Rank {0} DC does not look valid", Convert.ToString(i + 1));
                    }
                    
                    if (string.IsNullOrEmpty(item.ErrorMessage))
                    {
                        if (!string.IsNullOrEmpty(item.SubmittedLeadtime[i]))
                        {
                            if (NumberRegex.IsMatch(item.SubmittedLeadtime[i]))
                                item.LeadtimeList.Add(Convert.ToInt32(item.SubmittedLeadtime[i]));
                            else
                                item.ErrorMessage = string.Format("Error - Leadtime {0} does not look a valid number", Convert.ToString(i + 1));
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(item.ErrorMessage))
            {
                int StoreCount = config.db.StoreLeadTimes.Where(slt => slt.Division == item.Division && slt.Store == item.Store).Count();

                if (StoreCount == 0)
                    item.ErrorMessage = "The division and store is not already a part of NSS. You can only update via spreadsheet";
            }

            if (string.IsNullOrEmpty(item.ErrorMessage))
            {
                if (item.LeadtimeList.Count != item.DCIDList.Count)
                    item.ErrorMessage = string.Format("The number of valid DCs ({0}) does not match the number of valid leadtimes ({1})", item.DCIDList.Count.ToString(), item.LeadtimeList.Count.ToString());
            }
        }

        private void RemoveStoreLeadTimes(string division, string store)
        {
            //delete stores current zone assignment
            NetworkZoneStore networkZoneStore = config.db.NetworkZoneStores.Where(nzs => nzs.Division == division && nzs.Store == store).FirstOrDefault();

            if (networkZoneStore != null)
            {
                int tz = networkZoneStore.ZoneID;

                config.db.NetworkZoneStores.Remove(networkZoneStore);

                int remainingStores = config.db.NetworkZoneStores.Where(nzs => nzs.ZoneID == tz).Count();

                if (remainingStores == 0)
                {
                    //delete the zone if it's empty now
                    NetworkZone delNZ = config.db.NetworkZones.Where(nz => nz.ID == tz).First();
                    config.db.NetworkZones.Remove(delNZ);
                }
            }

            List<StoreLeadTime> storeLeadTimes = config.db.StoreLeadTimes.Where(slt => slt.Division == division && slt.Store == store).ToList();

            foreach (StoreLeadTime slt in storeLeadTimes)
            {
                config.db.StoreLeadTimes.Remove(slt);
            }
        }

        private List<StoreLeadTime> BuildStoreLeadTimeRecords(NSSUpload uploadRec)
        {
            List<StoreLeadTime> newLeadtimes = new List<StoreLeadTime>();

            for (int i = 0; i < uploadRec.DCIDList.Count; i++)
            {
                StoreLeadTime slt = new StoreLeadTime
                {
                    Division = uploadRec.Division,
                    Store = uploadRec.Store
                };

                if (uploadRec.DCIDList[i] != -1)
                {
                    slt.DCID = uploadRec.DCIDList[i];
                    slt.LeadTime = uploadRec.LeadtimeList[i];
                    slt.Rank = i + 1;
                    slt.Active = true;
                    slt.CreateDate = DateTime.Now;
                    slt.CreatedBy = config.currentUser.NetworkID;

                    newLeadtimes.Add(slt);
                }
            }

            return newLeadtimes;
        }

        private void SetUploadZones(string division, string store)
        {
            int zoneid = networkZoneStoreDAO.GetZoneForStore(division, store);

            NetworkZoneStore zonestore;
            if (zoneid <= 0)            
                zoneid = networkZoneStoreDAO.CreateNewZone(division, store, config.currentUser.NetworkID);            

            zonestore = new NetworkZoneStore
            {
                Division = division,
                Store = store,
                ZoneID = zoneid
            };

            config.db.NetworkZoneStores.Add(zonestore);
            config.db.SaveChanges();
        }

        public void Save(HttpPostedFileBase file)
        {
            NSSUpload item;

            LoadAttachment(file.InputStream);
            if (!HasValidHeaderRow())
                message = "Incorrectly formatted or missing header row. Please correct and re-process.";
            else
            {
                int row = 1;

                try
                {
                    DCs = config.db.DistributionCenters.ToList();

                    foreach (DataRow dataRow in excelData.Rows)
                    {
                        item = ParseRow(dataRow);

                        ValidateRow(item);
                        if (!string.IsNullOrEmpty(item.ErrorMessage))                                                    
                            errorList.Add(item);                       
                        else
                            validData.Add(item);

                        row++;
                    }

                    if (validData.Count > 0)
                    {
                        foreach (NSSUpload rec in validData)
                        {
                            RemoveStoreLeadTimes(rec.Division, rec.Store);

                            foreach (StoreLeadTime slt in BuildStoreLeadTimeRecords(rec))
                            {
                                config.db.StoreLeadTimes.Add(slt);
                            }
                        }

                        config.db.SaveChanges();

                        foreach (NSSUpload rec in validData)
                        {
                            rangePlanDetailDAO.ReassignStartDates(rec.Division, rec.Store);
                            SetUploadZones(rec.Division, rec.Store);
                        }
                    }
                }
                catch (Exception ex)
                {
                    message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
                    FLLogger logger = new FLLogger(config.LogFile);
                    logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                }
            }
        }

        public Workbook GetErrors(List<NSSUpload> errorList)
        {
            if (errorList != null)
            {
                Workbook excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (NSSUpload p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.SubmittedDivision);
                    mySheet.Cells[row, 1].PutValue(p.SubmittedStore);

                    for (int i = 0; i < p.MaxValues; i++)
                    {
                        mySheet.Cells[row, i + 2].PutValue(p.SubmittedRank[i]);
                        mySheet.Cells[row, i + p.MaxValues + 2].PutValue(p.SubmittedLeadtime[i]);
                    }

                    mySheet.Cells[row, maxColumns].PutValue(p.ErrorMessage);

                    Style errorStyle = mySheet.Cells[row, maxColumns].GetStyle();
                    errorStyle.Font.Color = Color.Red;

                    mySheet.Cells[row, maxColumns].SetStyle(errorStyle);
                    row++;
                }

                mySheet.AutoFitColumn(maxColumns);

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

        public RerankStoresSpreadsheet(AppConfig config, ConfigService configService, RangePlanDetailDAO rangePlanDetailDAO, NetworkZoneStoreDAO networkZoneStoreDAO) : base(config, configService)
        {
            maxColumns = 22;
            headerRowNumber = 0;

            columns.Add(0, "Division");
            columns.Add(1, "Store");
            columns.Add(2, "Rank 1");
            columns.Add(3, "Rank 2");
            columns.Add(4, "Rank 3");
            columns.Add(5, "Rank 4");
            columns.Add(6, "Rank 5");
            columns.Add(7, "Rank 6");
            columns.Add(8, "Rank 7");
            columns.Add(9, "Rank 8");
            columns.Add(10, "Rank 9");
            columns.Add(11, "Rank 10");           
            columns.Add(12, "Leadtime 1");
            columns.Add(13, "Leadtime 2");
            columns.Add(14, "Leadtime 3");
            columns.Add(15, "Leadtime 4");
            columns.Add(16, "Leadtime 5");
            columns.Add(17, "Leadtime 6");
            columns.Add(18, "Leadtime 7");
            columns.Add(19, "Leadtime 8");
            columns.Add(20, "Leadtime 9");
            columns.Add(21, "Leadtime 10");

            templateFilename = config.RerankStoresTemplate;

            this.rangePlanDetailDAO = rangePlanDetailDAO;
            this.networkZoneStoreDAO = networkZoneStoreDAO;
        }
    }
}