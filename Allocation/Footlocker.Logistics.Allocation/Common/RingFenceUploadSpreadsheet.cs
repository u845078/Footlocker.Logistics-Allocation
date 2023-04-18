using Aspose.Excel;
using Footlocker.Common.Entities;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Models.Services;
using Footlocker.Logistics.Allocation.Services;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Drawing;
using System.Linq;
using System.Web;
using Telerik.Web.Mvc.Extensions;
using System.Web.Services.Description;

namespace Footlocker.Logistics.Allocation.Common
{
    public class RingFenceUploadSpreadsheet : UploadSpreadsheet
    {
        public List<RingFenceUploadModel> errorList = new List<RingFenceUploadModel>();
        public List<RingFenceUploadModel> validRingFences = new List<RingFenceUploadModel>();
        public List<RingFenceUploadModel> parsedRingFences = new List<RingFenceUploadModel>();
        public List<RingFenceUploadModel> warehouseRingFences = new List<RingFenceUploadModel>();
        public List<RingFenceUploadModel> futureWarehouseRingFences = new List<RingFenceUploadModel>();
        public List<RingFenceUploadModel> ecomRingFences = new List<RingFenceUploadModel>();
        public List<RingFenceUploadModel> explodingEcomRingFences = new List<RingFenceUploadModel>();
        public List<RingFenceUploadModel> allSizeRingFences = new List<RingFenceUploadModel>();
        readonly RingFenceDAO ringfenceDAO;
        readonly LegacyFutureInventoryDAO futureInventoryDAO;
        List<EcommWarehouse> ecommWarehouses;
        int successfulCount;
        public List<RingFenceUploadModel> warnings;
        public List<RingFenceUploadModel> errors;

        private RingFenceUploadModel ParseRow(int row)
        {
            RingFenceUploadModel returnValue = new RingFenceUploadModel()
            {
                Division = Convert.ToString(worksheet.Cells[row, 0].Value).Trim(),
                Store = Convert.ToString(worksheet.Cells[row, 1].Value).Trim(),
                SKU = Convert.ToString(worksheet.Cells[row, 2].Value).Trim(),
                EndDate = Convert.ToString(worksheet.Cells[row, 3].Value),
                PO = Convert.ToString(worksheet.Cells[row, 4].Value),
                Warehouse = Convert.ToString(worksheet.Cells[row, 5].Value).Trim().PadLeft(2, '0'),
                Size = Convert.ToString(worksheet.Cells[row, 6].Value).ToUpper(),
                QtyString = Convert.ToString(worksheet.Cells[row, 7].Value).ToUpper(),
                Comments = Convert.ToString(worksheet.Cells[row, 8].Value)
            };

            returnValue.Store = string.IsNullOrEmpty(returnValue.Store) ? "" : returnValue.Store.PadLeft(5, '0');
            
            if (returnValue.PO == null)
                returnValue.PO = "";

            return returnValue;
        }

        private bool ValidateRow(RingFenceUploadModel inputData, out string errorMessage)
        {
            errorMessage = string.Empty;
            bool canConvert;

            if (string.IsNullOrEmpty(inputData.Division))
                errorMessage = "Division must be provided ";

            if (string.IsNullOrEmpty(inputData.SKU))
                errorMessage += "Sku must be provided. ";
            else 
                if (inputData.Division != inputData.SKU.Substring(0, 2))
                    errorMessage += "The division entered does not match the Sku's division. ";

            if (string.IsNullOrEmpty(inputData.Size))
                errorMessage += "Size or caselot must be provided. ";
            else
                if (inputData.Size.Length != 3 && inputData.Size.Length != 5)
                    errorMessage += string.Format("The size {0} is non-existent or invalid. ", inputData.Size);

            if (inputData.PO != "")
            {
                if (inputData.PO.Length != 7)
                    errorMessage += "PO must be seven digits. ";
            }                

            if (inputData.QtyString == "ALL")
            {
                if (string.IsNullOrEmpty(inputData.PO) || inputData.Size != "ALL")
                    errorMessage += "When using ALL Quantity, Size must also be ALL and a PO must be given. ";
            }
            else
            {
                int tempInt;
                canConvert = int.TryParse(inputData.QtyString, out tempInt);

                if (canConvert)
                {
                    inputData.Quantity = tempInt;

                    if (inputData.Quantity <= 0)
                        errorMessage += "The quantity provided cannot be less than or equal to zero. ";
                }                    
                else
                    errorMessage += "Quantity is not ALL or numeric. ";
            }

            if (inputData.Size == "ALL")
                if (string.IsNullOrEmpty(inputData.PO) || inputData.QtyString != "ALL")
                    errorMessage += "When using ALL Size, Quantity must be ALL and a PO must be given. ";

            return string.IsNullOrEmpty(errorMessage);
        }

        private void ValidateList()
        {
            List<ValidStoreLookup> validStores;
            List<string> uniqueDivisions = parsedRingFences.Select(rf => rf.Division).Distinct().ToList();

            List<string> uniqueMFCodes = parsedRingFences.Select(rf => rf.Warehouse).Distinct().ToList();
            List<string> invalidDCList = uniqueMFCodes.Where(udc => !ringfenceDAO.distributionCenters.Any(dc => dc.MFCode == udc)).ToList();

            parsedRingFences.Where(rf => invalidDCList.Contains(rf.Warehouse)).ForEach(idc => idc.ErrorMessage = "DC is invalid");

            validStores = config.db.vValidStores.ToList();

            List<string> uniqueStores = parsedRingFences.Where(rf => rf.Store != "00800" && !string.IsNullOrEmpty(rf.Store))
                                                        .Select(rf => string.Format("{0}-{1}", rf.Division, rf.Store)).Distinct().ToList();
            List<string> invalidStoreList = uniqueStores.Where(us => !validStores.Any(s => string.Format("{0}-{1}", s.Division, s.Store) == us)).ToList();

            parsedRingFences.Where(rf => invalidStoreList.Contains(string.Format("{0}-{1}", rf.Division, rf.Store)))
                            .ForEach(ist => ist.ErrorMessage = string.Format("The division and store combination {0}-{1} is not an existing or valid combination.", ist.Division, ist.Store));

            List<string> invalidDivisions = uniqueDivisions.Where(div => !config.currentUser.GetUserDivList(config.AppName).Exists(ud => ud == div)).ToList();
            parsedRingFences.Where(rf => invalidDivisions.Contains(rf.Division))
                            .ForEach(rf => rf.ErrorMessage = string.Format("You do not have permission for Division {0}.", rf.Division));

            var dupList = parsedRingFences.GroupBy(pr => new { pr.Division, pr.Store, pr.SKU, pr.EndDate, pr.PO, pr.Warehouse, pr.Size })
                                          .Where(pr => pr.Count() > 1)
                                          .Select(pr => new { DuplicateRF = pr.First(), Counter = pr.Count() })
                                          .ToList();
            foreach (var dup in dupList)
            {
                parsedRingFences.Where(pr => pr.Division == dup.DuplicateRF.Division &&
                                             pr.Store == dup.DuplicateRF.Store &&
                                             pr.SKU == dup.DuplicateRF.SKU &&
                                             pr.EndDate == dup.DuplicateRF.EndDate &&
                                             pr.PO == dup.DuplicateRF.PO &&
                                             pr.Warehouse == dup.DuplicateRF.Warehouse &&
                                             pr.Size == dup.DuplicateRF.Size)
                    .ForEach(pr => pr.ErrorMessage = string.Format("The following row of data was duplicated in the spreadsheet {0} times.  Please provide unique rows of data.", dup.Counter));
            }

            List<string> uniquePOs = parsedRingFences.Where(rf => !string.IsNullOrEmpty(rf.PO)).Select(rf => rf.PO).Distinct().ToList();
            List<string> invalidPOList = uniquePOs.Where(up => !config.db.POs.Any(p => p.PO == up)).ToList();

            parsedRingFences.Where(rf => invalidPOList.Contains(rf.PO)).ForEach(ip => ip.ErrorMessage = "PO was not found in PO table");

            List<RingFenceUploadModel> poRingFences = parsedRingFences.Where(rf => string.IsNullOrEmpty(rf.ErrorMessage) &&
                                                                                   !string.IsNullOrEmpty(rf.PO)).ToList();
            foreach (RingFenceUploadModel poRF in poRingFences)
            {
                List<LegacyFutureInventory> poDetails = futureInventoryDAO.GetPOInventoryData(poRF.Division, poRF.PO);
                if (poDetails.Count == 0)
                    poRF.ErrorMessage = "PO information not found for division";
                else
                {
                    if (poDetails.Where(p => p.Sku == poRF.SKU).Count() == 0)
                        poRF.ErrorMessage = "Ring Fence SKU does not match the PO SKU";

                    if (poDetails.Where(p => p.Store == poRF.Warehouse).Count() == 0)
                        poRF.ErrorMessage = "Ring Fence Warehouse does not match the PO warehouse";
                }
            }
        }

        private void ValidateInventory()
        {
            List<Tuple<string, string, string>> uniqueCombos = new List<Tuple<string, string, string>>();

            // unique combos excluding ringfences with POs (available)
            uniqueCombos = warehouseRingFences.Select(pr => Tuple.Create(pr.SKU, pr.Size, pr.Warehouse)).Distinct().ToList();
            List<WarehouseInventory> details = ringfenceDAO.GetWarehouseAvailableNew(uniqueCombos);
            // reduce details
            details = ringfenceDAO.ReduceRingFenceQuantities(details);

            // remove all parsedRFs that do not have an associated mainframe warehouse inventory record.
            warehouseRingFences.Where(pr => !details.Any(d => d.Sku.Equals(pr.SKU) && d.size.Equals(pr.Size) && d.DistributionCenterID.Equals(pr.Warehouse)))
                               .ToList()
                               .ForEach(pr =>
                               {
                                   pr.ErrorMessage = "The Sku, Size, and DC combination could not be found within our system.";
                                   errorList.Add(pr);                                   
                                   validRingFences.Remove(pr);
                               });

            // unique non-future combos with summed quantity
            var uniqueRFsGrouped = warehouseRingFences.Where(pr => string.IsNullOrEmpty(pr.ErrorMessage))
                                                      .GroupBy(pr => new { Sku = pr.SKU, pr.Size, DC = pr.Warehouse })
                                                      .Select(pr => new { pr.Key.Sku, pr.Key.Size, pr.Key.DC, Quantity = pr.Sum(rf => rf.Quantity) })
                                                      .ToList();

            var invalidRingFenceQuantity = (from rf in uniqueRFsGrouped
                                            join d in details
                                              on new { rf.Sku, rf.Size, rf.DC } equals
                                                 new { d.Sku, Size = d.size, DC = d.DistributionCenterID }
                                            where rf.Quantity > d.availableQuantity
                                            select Tuple.Create(rf, d.availableQuantity)).ToList();

            foreach (var rf in invalidRingFenceQuantity)
            {
                var rfsToDelete = warehouseRingFences.Where(pr => pr.SKU == rf.Item1.Sku &&
                                                                  pr.Size == rf.Item1.Size &&
                                                                  pr.Warehouse == rf.Item1.DC).ToList();
                rfsToDelete.ForEach(drf =>
                {
                    drf.ErrorMessage = string.Format("Not enough inventory available for all sizes. Available inventory: {0}", rf.Item2);
                    errorList.Add(drf);
                    validRingFences.Remove(drf);
                });
            }

            // unique Sku, Size, PO, WhseID combos for future pos
            var uniqueFutureCombos = futureWarehouseRingFences.Select(pr => Tuple.Create(pr.SKU, pr.Size, pr.PO, pr.Warehouse)).Distinct().ToList();

            details = ringfenceDAO.GetFuturePOsNew(uniqueFutureCombos);
            // reduce details
            details = ringfenceDAO.ReduceRingFenceQuantities(details);

            // unique future combos with summed quantity
            var uniqueFutureCombosGrouped = futureWarehouseRingFences.GroupBy(pr => new { Sku = pr.SKU, pr.PO, pr.Size, DC = pr.Warehouse })
                                                                     .Select(pr => new { pr.Key.Sku, pr.Key.PO, pr.Key.Size, pr.Key.DC, Quantity = pr.Sum(rf => rf.Quantity) })
                                                                     .ToList();

            var nonExistentCombo = uniqueFutureCombosGrouped.Where(ucg => !details.Any(d => d.Sku == ucg.Sku &&
                                                                                            d.PO == ucg.PO &&
                                                                                            d.size == ucg.Size &&
                                                                                            d.DistributionCenterID == ucg.DC)).ToList();

            foreach (var nec in nonExistentCombo)
            {
                var futureRFsToDelete = futureWarehouseRingFences.Where(pr => pr.SKU == nec.Sku &&
                                                                              pr.PO == nec.PO &&
                                                                              pr.Size == nec.Size &&
                                                                              pr.Warehouse == nec.DC).ToList();

                futureRFsToDelete.ForEach(r =>
                {
                    r.ErrorMessage = "Could not find any quantity within the system";
                    errorList.Add(r);
                    validRingFences.Remove(r);
                });
            }

            // summed quantity > mainframe quantity (details)
            var invalidFutureCombos = (from r in uniqueFutureCombosGrouped
                                       join d in details
                                         on new { r.Sku, r.PO, r.Size, r.DC }
                                     equals new { d.Sku, d.PO, Size = d.size, DC = d.DistributionCenterID }
                                       where r.Quantity > d.availableQuantity
                                       select Tuple.Create(r, d.availableQuantity)).ToList();

            foreach (var r in invalidFutureCombos)
            {
                var futureRFsToDelete = futureWarehouseRingFences.Where(pr => pr.SKU == r.Item1.Sku &&
                                                                              pr.PO == r.Item1.PO &&
                                                                              pr.Size == r.Item1.Size &&
                                                                              pr.Warehouse == r.Item1.DC).ToList();

                futureRFsToDelete.ForEach(rftd =>
                {
                    rftd.ErrorMessage = string.Format("Not enough inventory on the PO for all sizes. Available inventory {0}", r.Item2);
                    errorList.Add(rftd);
                    validRingFences.Remove(rftd);
                });
            }
        }

        private void CreateOrUpdateRingFences(bool accumulateQuantity)
        {
            List<RingFenceDetail> ringFenceDetails = new List<RingFenceDetail>();
            // group ring fences by div, store, sku, and list of there details (which is just the upload model)
            var rfHeaders = validRingFences.GroupBy(vr => new { vr.Division, vr.Store, Sku = vr.SKU })
                                           .Select(vr => new
                                           {
                                               vr.Key.Division,
                                               vr.Key.Store,
                                               vr.Key.Sku,
                                               Details = vr.ToList()
                                           }).ToList();

            /* this is a messy linq query so allow me to explain.
             * At first we are joining our grouped ring fences together with the ringfence table to
             * see if there is an existing ringfence for this upload.  If it is found, we can't
             * make the assumption that there is not two or more records that will be returned.
             * Therefore, after selecting the initial division, store, sku, and detail records, we
             * group by division, store, and sku and then select the first record in the list that
             * we find (this is how the old process worked and I believe this needs revisited)
             */
            var existingHeaders = (from gr in rfHeaders
                                   join rf in config.db.RingFences
                                     on new { gr.Division, gr.Store, gr.Sku } equals
                                        new { rf.Division, rf.Store, rf.Sku }
                                   where rf.EndDate == null || rf.EndDate > DateTime.Now
                                   select new { RingFenceID = rf.ID, rf.Division, rf.Store, rf.Sku, gr.Details })
                                      .GroupBy(rf => new { rf.Division, rf.Store, rf.Sku })
                                      .Select(rf => new
                                      {
                                          rf.FirstOrDefault().RingFenceID,
                                          rf.Key.Division,
                                          rf.Key.Store,
                                          rf.Key.Sku,
                                          rf.FirstOrDefault().Details
                                      }).ToList();

            // reselect the existingHeaders to be in the format to use Except in the next statement (just excludes the ringfenceid we brought back)
            var reformattedHeaders = existingHeaders.Select(egr => new { egr.Division, egr.Store, egr.Sku, egr.Details }).ToList();
            var nonExistingHeaders = rfHeaders.Except(reformattedHeaders).ToList();

            // this section is for populating data from the database before we build out the non-existing ring fences.
            // I populate these lists to reduce the number of database calls for each and every new ring fence.
            var uniqueDivisions = validRingFences.Select(rf => rf.Division).Distinct().ToList();
            var divisionControlDateMapping = (from ud in uniqueDivisions
                                              join id in config.db.InstanceDivisions
                                                on ud equals id.Division
                                              join cd in config.db.ControlDates
                                                on id.InstanceID equals cd.InstanceID
                                              select new { id.Division, cd.RunDate }).ToList();

            var uniqueDCs = validRingFences.Select(vr => vr.Warehouse).Distinct().ToList();
            var dcidMapping = (from dcs in config.db.DistributionCenters.Where(dcs => uniqueDCs.Contains(dcs.MFCode))
                               select new { DCID = dcs.ID, dcs.MFCode }).ToList();

            var uniqueSkus = validRingFences.Select(vr => vr.SKU).Distinct().ToList();

            var skuItemIDMapping = (from im in config.db.ItemMasters.Where(im => uniqueSkus.Contains(im.MerchantSku))
                                    select new { Sku = im.MerchantSku, ItemID = im.ID }).ToList();

            List<Tuple<string, int>> uniqueCaselotNameQtys = new List<Tuple<string, int>>();
            List<string> uniqueCaselots = new List<string>();

            if (validRingFences.Any(vr => vr.Size.Length == 5))
            {
                // unique caselot schedule names
                uniqueCaselots = validRingFences.Where(rf => rf.Size.Length == 5)
                                                .Select(rf => rf.Size)
                                                .Distinct()
                                                .ToList();

                var itempacks = config.db.ItemPacks.Where(ip => uniqueCaselots.Contains(ip.Name)).ToList();
                uniqueCaselotNameQtys = itempacks.Select(ip => Tuple.Create(ip.Name, ip.TotalQty))
                                                 .Distinct()
                                                 .ToList();
            }

            config.db.Configuration.AutoDetectChangesEnabled = false;
            config.db.Configuration.LazyLoadingEnabled = false;
            config.db.Configuration.ProxyCreationEnabled = false;
            config.db.Configuration.ValidateOnSaveEnabled = false;

            // create ring fences and ring fence details for non-existing combinations
            foreach (var grf in nonExistingHeaders)
            {
                // populate header record
                RingFence rf = new RingFence()
                {
                    Division = grf.Division,
                    Store = grf.Store,
                    Sku = grf.Sku,                                        
                    CreateDate = DateTime.Now,
                    CreatedBy = config.currentUser.NetworkID,
                    LastModifiedDate = DateTime.Now,
                    LastModifiedUser = config.currentUser.NetworkID
                };

                if (string.IsNullOrEmpty(grf.Details.FirstOrDefault().EndDate))
                    rf.EndDate = null;
                else
                    rf.EndDate = DateTime.ParseExact(grf.Details.FirstOrDefault().EndDate, "d/M/yyyy", CultureInfo.InvariantCulture);

                rf.ItemID = skuItemIDMapping.Where(r => r.Sku == rf.Sku).Select(r => r.ItemID).FirstOrDefault();
                rf.StartDate = divisionControlDateMapping.Where(cd => cd.Division == rf.Division).Select(cd => cd.RunDate).FirstOrDefault().AddDays(1);
                rf.Comments = grf.Details.FirstOrDefault() == null ? "" : grf.Details.FirstOrDefault().Comments;
                rf.Type = ecommWarehouses.Any(ew => ew.Division == rf.Division && ew.Store == rf.Store) ? 2 : 1;
                rf.ringFenceDetails = new List<RingFenceDetail>();
                // traverse and generate detail records
                foreach (var detail in grf.Details)
                {
                    RingFenceDetail rfd = new RingFenceDetail()
                    {
                        DCID = dcidMapping.Where(dc => detail.Warehouse == dc.MFCode).Select(dc => dc.DCID).FirstOrDefault(),
                        PO = detail.PO,
                        Size = detail.Size,
                        Qty = detail.Quantity,
                        ringFenceStatusCode = !string.IsNullOrEmpty(detail.PO) ? "1" : "4",
                        ActiveInd = "1",
                        LastModifiedDate = DateTime.Now,
                        LastModifiedUser = config.currentUser.NetworkID
                    };

                    rf.ringFenceDetails.Add(rfd);
                }

                rf.Qty = ringfenceDAO.CalculateHeaderQty(uniqueCaselotNameQtys, rf.ringFenceDetails);
                config.db.RingFences.Add(rf);
            }

            // retrieve existing ringfences all at once, and then locally map it
            List<long> uniqueRFIDs = existingHeaders.Select(egr => egr.RingFenceID).Distinct().ToList();

            List<RingFence> existingRingFences = config.db.RingFences.Include("ringFenceDetails")
                                                                     .Include("ringFenceDetails.DistributionCenter")
                                                                     .Where(rf => uniqueRFIDs.Contains(rf.ID))
                                                                     .ToList();

            foreach (var erf in existingRingFences)
            {
                // retrieve specific groupedRF
                var groupedRF = existingHeaders.Where(egr => egr.RingFenceID == erf.ID).FirstOrDefault();
                if (groupedRF != null)
                {
                    // determine how many details are existent for the specified rf
                    var existingDetails = erf.ringFenceDetails.Where(rfd => groupedRF.Details.Any(d => rfd.Size == d.Size &&
                                                                                                       rfd.PO == d.PO &&
                                                                                                       rfd.DistributionCenter.MFCode == d.Warehouse)).ToList();
                    // update each detail
                    if (existingDetails.Count > 0)
                    {
                        foreach (var detail in existingDetails)
                        {
                            // retrieve specific detail
                            var newDetail = groupedRF.Details.Where(d => d.Size == detail.Size &&
                                                                         d.PO == detail.PO &&
                                                                         d.Warehouse == detail.DistributionCenter.MFCode).FirstOrDefault();

                            if (detail.ActiveInd == "1")
                            {
                                if (accumulateQuantity)
                                {
                                    detail.Qty += newDetail.Quantity;
                                    newDetail.ErrorMessage = "Warning: Already existed, accumulated to new value";
                                    errorList.Add(newDetail);                                    
                                }
                                else
                                {
                                    detail.Qty = newDetail.Quantity;
                                    newDetail.ErrorMessage = "Warning: Already existed, updated to upload value";
                                    errorList.Add(newDetail);                                    
                                }
                            }
                            else
                            {
                                detail.Qty = newDetail.Quantity;
                            }

                            // ensure detail is active now
                            detail.ActiveInd = "1";
                            detail.LastModifiedDate = DateTime.Now;
                            detail.LastModifiedUser = config.currentUser.NetworkID;
                            config.db.Entry(detail).State = EntityState.Modified;
                        }
                    }

                    // determine how many details are non-existent for the specified rf
                    var nonExistingDetails = groupedRF.Details.Where(d => !erf.ringFenceDetails.Any(rfd => rfd.Size == d.Size &&
                                                                                                           rfd.PO == d.PO &&
                                                                                                           rfd.ActiveInd == "1" &&
                                                                                                           rfd.DistributionCenter.MFCode == d.Warehouse)).ToList();

                    // create each detail
                    if (nonExistingDetails.Count > 0)
                    {
                        foreach (var neDetail in nonExistingDetails)
                        {
                            RingFenceDetail rfd = new RingFenceDetail()
                            {
                                RingFenceID = erf.ID,
                                PO = neDetail.PO,
                                Size = neDetail.Size,
                                Qty = neDetail.Quantity,
                                ActiveInd = "1",
                                LastModifiedDate = DateTime.Now,
                                LastModifiedUser = config.currentUser.NetworkID,
                                ringFenceStatusCode = !string.IsNullOrEmpty(neDetail.PO) ? "1" : "4",
                                DCID = dcidMapping.Where(dc => neDetail.Warehouse == dc.MFCode).Select(dc => dc.DCID).FirstOrDefault()
                            };
                            
                            erf.ringFenceDetails.Add(rfd);
                            config.db.Entry(rfd).State = EntityState.Added;
                        }
                    }
                }

                var endDate = groupedRF.Details.Select(d => d.EndDate).FirstOrDefault();
                erf.EndDate = Convert.ToDateTime(endDate);
                config.db.Entry(erf).State = EntityState.Modified;
                erf.Qty = ringfenceDAO.CalculateHeaderQty(uniqueCaselotNameQtys, erf.ringFenceDetails);
                erf.Comments = groupedRF.Details.FirstOrDefault().Comments;
            }

            // save the changes for the already existing ringfences
            config.db.SaveChangesBulk(config.currentUser.NetworkID);
        }

        private void ProcessAllSizeRingFences()
        {
            List<RingFenceUploadModel> expandedRFs = new List<RingFenceUploadModel>();

            foreach (RingFenceUploadModel rf in allSizeRingFences)
            {
                List<LegacyFutureInventory> poDetails = futureInventoryDAO.GetPOInventoryData(rf.Division, rf.PO).Where(p => p.Sku == rf.SKU).ToList();

                foreach (LegacyFutureInventory poRec in poDetails)
                {
                    expandedRFs.Add(new RingFenceUploadModel()
                    {
                        Division = rf.Division,
                        Store = rf.Store, 
                        SKU = rf.SKU,
                        EndDate = rf.EndDate, 
                        PO = rf.PO,
                        Warehouse = rf.Warehouse,
                        Size = poRec.Size, 
                        Quantity = poRec.StockQty,
                        Comments = rf.Comments
                    });
                }
            }

            validRingFences.AddRange(expandedRFs);
        }

        public void Save(HttpPostedFileBase attachment, bool accumulateQuantity)
        {
            RingFenceUploadModel uploadRec;
            List<EcommRingFence> ecomRFs = new List<EcommRingFence>();

            ecommWarehouses = config.db.EcommWarehouses.ToList();

            LoadAttachment(attachment);
            if (!HasValidHeaderRow())
                message = "Incorrectly formatted or missing header row. Please correct and re-process.";
            else
            {
                int row = 1;
                try
                {
                    while (HasDataOnRow(row))
                    {
                        uploadRec = ParseRow(row);                        

                        if (!ValidateRow(uploadRec, out errorMessage))
                        {
                            uploadRec.ErrorMessage = errorMessage;
                            errorList.Add(uploadRec);
                        }
                        else
                            parsedRingFences.Add(uploadRec);

                        row++;
                    }

                    ValidateList();

                    foreach (RingFenceUploadModel rf in parsedRingFences)
                    {
                        if (string.IsNullOrEmpty(rf.ErrorMessage))
                        {
                            if (rf.Store == "00800")
                                explodingEcomRingFences.Add(rf);
                            else
                            {
                                if (rf.Size == "ALL")
                                    allSizeRingFences.Add(rf);
                                else
                                {
                                    validRingFences.Add(rf);

                                    if (ecommWarehouses.Any(ew => ew.Division == rf.Division && ew.Store == rf.Store))
                                        ecomRingFences.Add(rf);
                                    else
                                    {
                                        if (string.IsNullOrEmpty(rf.PO))
                                            warehouseRingFences.Add(rf);
                                        else
                                            futureWarehouseRingFences.Add(rf);
                                    }
                                }
                            }                                
                        }                            
                        else
                            errorList.Add(rf);
                    }

                    ValidateInventory();

                    if (allSizeRingFences.Count > 0)
                        ProcessAllSizeRingFences();

                    if (validRingFences.Count > 0)
                    {
                        // creates or updates the valid ring fences
                        CreateOrUpdateRingFences(accumulateQuantity);
                    }

                    if (explodingEcomRingFences.Count > 0)
                    {                        
                        foreach (RingFenceUploadModel rf in explodingEcomRingFences)
                        {
                            ecomRFs.Add(new EcommRingFence(rf.SKU, rf.Size, rf.PO, rf.Quantity, rf.Comments));
                        }

                        // process ecom explosion ringfences                        
                        ringfenceDAO.SaveEcommRingFences(ecomRFs, config.currentUser.NetworkID, accumulateQuantity);
                    }

                    successfulCount = validRingFences.Count + ecomRFs.Count;
                    warnings = errorList.Where(el => el.ErrorMessage.StartsWith("Warning")).ToList();
                    errors = errorList.Except(warnings).ToList();
                }
                catch (Exception ex)
                {
                    message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
                    FLLogger logger = new FLLogger(config.LogFile);
                    logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                }
            }
        }

        public Excel GetErrors(List<RingFenceUploadModel> errorList)
        {
            if (errorList != null)
            {
                Excel excelDocument = GetTemplate();
                Worksheet mySheet = excelDocument.Worksheets[0];

                int row = 1;
                foreach (RingFenceUploadModel p in errorList)
                {
                    mySheet.Cells[row, 0].PutValue(p.Division);
                    mySheet.Cells[row, 1].PutValue(p.Store);
                    mySheet.Cells[row, 2].PutValue(p.SKU);
                    mySheet.Cells[row, 3].PutValue(p.EndDate);
                    mySheet.Cells[row, 4].PutValue(p.PO);
                    mySheet.Cells[row, 5].PutValue(p.Warehouse);
                    mySheet.Cells[row, 6].PutValue(p.Size);

                    if (p.QtyString == "ALL")
                        mySheet.Cells[row, 7].PutValue(p.QtyString);
                    else
                        mySheet.Cells[row, 7].PutValue(p.Quantity);

                    mySheet.Cells[row, 8].PutValue(p.Comments);
                    mySheet.Cells[row, 9].PutValue(p.ErrorMessage);
                    mySheet.Cells[row, 9].Style.Font.Color = Color.Red;
                    row++;
                }

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

        public RingFenceUploadSpreadsheet(AppConfig config, ConfigService configService, RingFenceDAO ringFenceDAO, 
                                          LegacyFutureInventoryDAO legacyFutureInventoryDAO) : base(config, configService)
        {
            maxColumns = 9;

            columns.Add(0, "Div");
            columns.Add(1, "Store");
            columns.Add(2, "SKU");
            columns.Add(3, "End Date");
            columns.Add(4, "PO");
            columns.Add(5, "Warehouse");
            columns.Add(6, "Size");
            columns.Add(7, "Qty");
            columns.Add(8, "Comments");

            templateFilename = config.RingFenceUploadTemplate;
            ringfenceDAO = ringFenceDAO;
            futureInventoryDAO = legacyFutureInventoryDAO;
        }
    }
}