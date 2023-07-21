using Aspose.Excel;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Models.Services;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using Footlocker.Logistics.Allocation.Common;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class WebPickSpreadsheet : UploadSpreadsheet
    {
        List<RDQ> parsedRDQs = new List<RDQ>();
        public List<RDQ> validRDQs = new List<RDQ>();
        List<RDQ> ringFenceRDQs = new List<RDQ>();
        readonly List<string> webPickRoles;
        public List<Tuple<RDQ, string>> errorList = new List<Tuple<RDQ, string>>();

        private RDQ ParseUploadRow(int row)
        {
            RDQ returnValue = new RDQ
            {
                Sku = Convert.ToString(worksheet.Cells[row, 1].Value).Trim(),
                Size = Convert.ToString(worksheet.Cells[row, 2].Value).Trim(),
                PO = Convert.ToString(worksheet.Cells[row, 3].Value).Trim(),
                Qty = Convert.ToInt32(worksheet.Cells[row, 4].Value),
                DC = Convert.ToString(worksheet.Cells[row, 6].Value).Trim(),
                RingFencePickStore = Convert.ToString(worksheet.Cells[row, 7].Value).Trim()
            };

            returnValue.DC = string.IsNullOrEmpty(returnValue.DC) ? "" : returnValue.DC.PadLeft(2, '0');
            returnValue.RingFencePickStore = string.IsNullOrEmpty(returnValue.RingFencePickStore) ? "" : returnValue.RingFencePickStore.PadLeft(5, '0');

            if (!string.IsNullOrEmpty(returnValue.Sku))
            {
                returnValue.Division = returnValue.Sku.Substring(0, 2);
            }

            if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[row, 0].Value).Trim()))
                returnValue.Store = Convert.ToString(worksheet.Cells[row, 0].Value).Trim().PadLeft(5, '0');
            else
            {
                if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[row, 5].Value).Trim()))
                    returnValue.Store = Convert.ToString(worksheet.Cells[row, 5].Value).Trim().PadLeft(2, '0');
            }

            if (Convert.ToString(worksheet.Cells[row, 8].Value).Trim() == "Y")
                returnValue.Status = "E-PICK";
            else
                returnValue.Status = "WEB PICK";

            return returnValue;
        }

        private void SetErrorMessage(List<Tuple<RDQ, string>> errorList, RDQ errorRDQ, string newErrorMessage)
        {
            int tupleIndex = errorList.FindIndex(err => err.Item1.Equals(errorRDQ));
            if (tupleIndex > -1)            
                errorList[tupleIndex] = Tuple.Create(errorRDQ, string.Format("{0} {1}", errorList[tupleIndex].Item2, newErrorMessage));            
            else            
                errorList.Add(Tuple.Create(errorRDQ, newErrorMessage));            
        }

        private bool ValidateFile(List<RDQ> parsedRDQs, List<Tuple<RDQ, string>> errorList, out string errorMessage)
        {
            errorMessage = "";

            // remove all records that have a null or empty sku... we grab the division from this field so there
            // cannot be any empty skus before validating the file for only one division!
            parsedRDQs.Where(pr => string.IsNullOrEmpty(pr.Sku))
                      .ToList()
                      .ForEach(r =>
                      {
                          SetErrorMessage(errorList, r, "Sku must be provided.");
                          parsedRDQs.Remove(r);
                      });

            parsedRDQs.Where(pr => pr.Status == "E-PICK" && !string.IsNullOrEmpty(pr.PO))
                      .ToList()
                      .ForEach(r =>
                      {
                          SetErrorMessage(errorList, r, " You cannot do an E-Pick with a PO.");
                          parsedRDQs.Remove(r);
                      });

            if (parsedRDQs.Count > 0)
            {
                // 1) check to see if only one division exists
                if (parsedRDQs.Select(pr => pr.Division).Distinct().Count() > 1)
                {
                    errorMessage = "Process cancelled.  Spreadsheet may only contain one division.";
                    return false;
                }
                else
                {
                    bool canEPick = false;
                    bool canWebPick = true;

                    List<string> userRoles = config.currentUser.GetUserRoles(config.AppName);

                    // if they don't have any web pick roles, take out web pick
                    if (!userRoles.Intersect(webPickRoles).Any())
                        canWebPick = false;

                    if (userRoles.Contains("EPick"))
                        canEPick = true;

                    string division = parsedRDQs.Select(pr => pr.Division).FirstOrDefault();
                    // if division is null, then no sku was entered on the spreadsheet
                    if (division != null)
                    {
                        foreach (RDQ rdq in parsedRDQs)
                        {
                            if ((rdq.Status == "E-PICK") && !canEPick)
                            {
                                errorMessage = "You do not have permission to do E-Picks. Please remove the E-Pick request(s) and resubmit";
                                return false;
                            }

                            if ((rdq.Status == "WEB PICK") && !canWebPick)
                            {
                                errorMessage = "You do not have permission to do Web Picks. Please remove the Web Pick request(s) and resubmit";
                                return false;
                            }
                        }

                        // 2) check to see if user has permission for this division
                        if (!config.currentUser.HasDivision(config.AppName, division))
                        {
                            errorMessage = string.Format("You do not have permission for Division {0}.", division);
                            return false;
                        }
                        else
                        {
                            // 3) check to see if the parsedRDQs has any duplicates. I don't return false
                            //    for this case because I just remove duplicates and move on with processing
                            parsedRDQs.GroupBy(pr => new { pr.Store, pr.Sku, pr.Size, pr.PO, pr.Qty, pr.DC, pr.RingFencePickStore, pr.Status })
                                      .Where(pr => pr.Count() > 1)
                                      .Select(pr => new { DuplicateRDQs = pr.ToList(), Counter = pr.Count() })
                                      .ToList().ForEach(r =>
                                      {
                                          // set error message for first duplicate and the amount of times it was found in the file
                                          SetErrorMessage(errorList, r.DuplicateRDQs.FirstOrDefault(),
                                              string.Format("The following row of data was duplicated in the spreadsheet {0} times.  Please provide unique rows of data.", r.Counter));
                                          // delete all instances of the duplications from the parsedRDQs list
                                          r.DuplicateRDQs.ForEach(dr => parsedRDQs.Remove(dr));
                                      });
                        }
                    }
                    else
                    {
                        errorMessage = "Please provide a SKU in order to continue.";
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Validate the inventory for the DC rdqs.  If it is invalid,
        /// it will remove the rdq from dcRDQs and add it to the error list.
        /// otherwise it will add the rdq to the validRDQs list.
        /// </summary>
        /// <param name="dcRDQs">valid supplied dc rdqs that do not have an invalid sku</param>
        /// <param name="validRDQs">the list of valid rdqs to generate at the end of the process</param>
        /// <param name="errorList">error list to display problems for the uploaded rdqs</param>
        private void ValidateAvailableQuantityForDCRDQs(List<RDQ> dcRDQs, List<RDQ> validRDQs, List<Tuple<RDQ, string>> errorList)
        {
            // unique combinations to pass to mf call (Sku, Size, DC)
            List<Tuple<string, string, string>> uniqueCombos = new List<Tuple<string, string, string>>();

            // retrieve the unique combinations to be sent to mf call
            uniqueCombos = dcRDQs.Select(dr => Tuple.Create(dr.Sku, dr.Size, dr.DC)).Distinct().ToList();

            if (uniqueCombos.Count > 0)
            {
                WarehouseInventoryDAO warehouseInventoryDAO = new WarehouseInventoryDAO(null, null, config.EuropeDivisions);
                List<WarehouseInventoryDAO.WarehouseInventoryLookup> warehouseInventoryLookups = new List<WarehouseInventoryDAO.WarehouseInventoryLookup>();
                foreach (var uc in uniqueCombos)
                {
                    warehouseInventoryLookups.Add(new WarehouseInventoryDAO.WarehouseInventoryLookup
                    {
                        SKU = uc.Item1,
                        Size = uc.Item2,
                        DCCode = uc.Item3,
                        OnHandQuantity = 0
                    });
                }
                List<WarehouseInventory> details = warehouseInventoryDAO.GetSQLWarehouseInventory(warehouseInventoryLookups);
                //    // retrieve all available quantity for the specified combinations in one mf call
                RingFenceDAO rfDAO = new RingFenceDAO(config.EuropeDivisions);
                details = rfDAO.ReduceAvailableInventory(details);

                // rdqs that cannot be satisfied by current whse avail quantity
                var dcRDQsGroupedBySize = dcRDQs.Where(r => r.Status == "WEB PICK")
                                                .GroupBy(r => new { r.Division, r.Sku, r.Size, DC = r.DCID.Value.ToString() })                                                
                                                .Select(r => new { r.Key.Division, r.Key.Sku, r.Key.Size, r.Key.DC, Quantity = r.Sum(s => s.Qty) })
                                                .ToList();

                var invalidRDQsAndAvailableQty = (from r in dcRDQsGroupedBySize
                                                  join d in details on new { r.Sku, r.Size, r.DC }
                                                                 equals new { d.Sku, Size = d.size, DC = d.DistributionCenterID }
                                                  where r.Quantity > d.quantity
                                                  select Tuple.Create(r, d.quantity)).ToList();

                foreach (var r in invalidRDQsAndAvailableQty)
                {
                    var dcRDQsToDelete = dcRDQs.Where(ir => ir.Division.Equals(r.Item1.Division) &&
                                           ir.Sku.Equals(r.Item1.Sku) &&
                                           ir.Size.Equals(r.Item1.Size) &&
                                           ir.DCID.Value.ToString().Equals(r.Item1.DC) &&
                                           ir.Status.Equals("WEB PICK")).ToList();

                    dcRDQsToDelete.ForEach(rtd =>
                    {
                        SetErrorMessage(errorList, rtd
                            , string.Format("Not enough inventory available for all sizes.  Available inventory: {0}", r.Item2));
                        dcRDQs.Remove(rtd);
                    });
                }

                validRDQs.AddRange(dcRDQs);
            }
        }

        /// <summary>
        /// Validate the inventory for the future rdqs, or RDQs with a PO.  If it is invalid,
        /// it will remove the rdq from dcRDQs and add it to the error list.
        /// otherwise it will add the rdq to the validRDQs list.
        /// </summary>
        /// <param name="futureRDQs">valid PO rdqs that do not have an invalid sku</param>
        /// <param name="validRDQs">the list of valid rdqs to generate at the end of the process</param>
        /// <param name="errorList">error list to display problems for the uploaded rdqs</param>
        private void ValidateFutureQuantityForDCRDQs(List<RDQ> futureRDQs, List<RDQ> validRDQs, List<Tuple<RDQ, string>> errorList)
        {
            List<RDQ> invalidRDQs = new List<RDQ>();

            var uniqueCombos = futureRDQs.Select(fr => new { fr.Division, fr.Sku, fr.Size, fr.DC, fr.PO, PODiv = fr.PO + "-" + fr.Division })
                .Distinct()
                .ToList();

            if (uniqueCombos.Count > 0)
            {
                // Look for situations where the PO is not found
                var invalidPO = uniqueCombos.Where(uc => !config.db.LegacyFutureInventory.Any(lfi => lfi.InventoryID.Equals(uc.PODiv))).ToList();

                foreach (var ip in invalidPO)
                {
                    invalidRDQs.AddRange(futureRDQs.Where(fr => fr.Division == ip.Division &&
                                                          fr.Sku == ip.Sku &&
                                                          fr.Size == ip.Size &&
                                                          fr.DC == ip.DC &&
                                                          fr.PO == ip.PO).ToList());
                    uniqueCombos.Remove(ip);
                }

                invalidRDQs.ForEach(r => SetErrorMessage(errorList, r, "PO was not found in Future Inventory. If it was created today, try again tomorrow."));
                invalidRDQs.ForEach(r => futureRDQs.Remove(r));
                invalidRDQs.Clear();
                invalidPO.Clear();

                invalidPO = uniqueCombos.Where(uc => !config.db.LegacyFutureInventory.Any(lfi => lfi.Division.Equals(uc.Division) &&
                                                                                                 lfi.Sku.Equals(uc.Sku) &&
                                                                                                 lfi.Size.Equals(uc.Size) &&
                                                                                                 lfi.Store.Equals(uc.DC) &&
                                                                                                 lfi.InventoryID.Equals(uc.PODiv))).ToList();

                foreach (var ip in invalidPO)
                {
                    invalidRDQs.AddRange(futureRDQs.Where(fr => fr.Division == ip.Division &&
                                                          fr.Sku == ip.Sku &&
                                                          fr.Size == ip.Size &&
                                                          fr.DC == ip.DC &&
                                                          fr.PO == ip.PO).ToList());
                    uniqueCombos.Remove(ip);
                }

                invalidRDQs.ForEach(r => SetErrorMessage(errorList, r, "Did not find any future inventory for this specific SKU/Size/DC"));
                invalidRDQs.ForEach(r => futureRDQs.Remove(r));
                invalidRDQs.Clear();
                invalidPO.Clear();

                // Look for situations where the exact key is not found
                List<LegacyFutureInventory> futureInventory = new List<LegacyFutureInventory>();

                foreach (var uc in uniqueCombos)
                {
                    futureInventory.AddRange(config.db.LegacyFutureInventory.AsNoTracking()
                                                                     .Where(lfi => lfi.Division == uc.Division &&
                                                                                   lfi.Sku == uc.Sku &&
                                                                                   lfi.Size == uc.Size &&
                                                                                   lfi.InventoryID == uc.PODiv &&
                                                                                   lfi.LocNodeType == "WAREHOUSE")
                                                                     .ToList());
                }

                foreach (LegacyFutureInventory fi in futureInventory)
                {
                    InventoryReductions inventoryReductions = config.db.InventoryReductions.Where(ir => ir.PO == fi.PO && ir.Sku == fi.Sku && ir.Size == fi.Size).FirstOrDefault();

                    if (inventoryReductions != null)
                        fi.StockQty -= inventoryReductions.Qty;
                }

                // Look for cases where RDQ quantity > reduced future inventory
                foreach (RDQ r in futureRDQs)
                {
                    int futureQty = 0;
                    LegacyFutureInventory futureInvRec = futureInventory.Where(fi => fi.PO == r.PO && fi.Sku == r.Sku && fi.Size == r.Size).FirstOrDefault();

                    if (futureInvRec != null)
                        futureQty = futureInvRec.StockQty;
                    else
                        futureQty = 0;

                    if (r.Qty > futureQty)
                        invalidRDQs.Add(r);
                    else
                    {
                        // in this case, there is enough future inventory, but we are going to reduce future inventory to claim this RDQ
                        futureInvRec.StockQty -= r.Qty;
                    }
                }

                invalidRDQs.ForEach(r => SetErrorMessage(errorList, r, "Inventory requested is greater than remaining inventory"));
                invalidRDQs.ForEach(r => futureRDQs.Remove(r));
                invalidRDQs.Clear();

                validRDQs.AddRange(futureRDQs);
            }
        }

        private void ValidateParsedRDQs(List<RDQ> parsedRDQs, List<RDQ> validRDQs, List<Tuple<RDQ, string>> errorList,
                                int instanceID, List<string> uniqueSkus, List<ItemMaster> uniqueItems)
        {
            // 1) division/store combination is valid
            var uniqueDivStoreList = parsedRDQs
                .Where(pr => pr.Store.Length == 5)
                .Select(pr => new { pr.Division, pr.Store }).Distinct().ToList();

            var invalidDivStoreList = uniqueDivStoreList.Where(ds => !config.db.vValidStores.Any(vs => vs.Store.Equals(ds.Store) &&
                                                                                                       vs.Division.Equals(ds.Division))).ToList();

            var uniqueDestWarehouseList = parsedRDQs
                .Where(pr => pr.Store.Length == 2)
                .Select(pr => new { pr.Store }).Distinct().ToList();

            var invalidWarehouseList = uniqueDestWarehouseList.Where(dw => !config.db.DistributionCenters.Any(dc => dc.MFCode.Equals(dw.Store))).ToList();
            List<string> kafkaWarehouseList = (from dc in config.db.DistributionCenters
                                               where dc.TransmitRDQsToKafka
                                               select dc.MFCode).ToList();

            parsedRDQs.Where(pr => invalidDivStoreList.Contains(new { pr.Division, pr.Store }))
                      .ToList()
                      .ForEach(r => SetErrorMessage(errorList, r,
                          string.Format("The division and store combination {0}-{1} is not an existing or valid combination.", r.Division, r.Store)));

            parsedRDQs.Where(pr => invalidWarehouseList.Contains(new { pr.Store }))
                      .ToList()
                      .ForEach(r => SetErrorMessage(errorList, r, string.Format("The warehouse {0} does not exist.", r.Store)));

            // 2) quantity is greater than zero
            parsedRDQs.Where(pr => pr.Qty <= 0)
                      .ToList()
                      .ForEach(r =>
                      {
                          SetErrorMessage(errorList, r, "The quantity provided cannot be less than or equal to zero.");
                          parsedRDQs.Remove(r);
                      });

            // 3) sku provided is valid
            var invalidSkusList = uniqueSkus.Where(sku => !uniqueItems.Any(im => im.MerchantSku.Equals(sku) &&
                                                                                 im.InstanceID == instanceID)).ToList();
            parsedRDQs.Where(pr => invalidSkusList.Contains(pr.Sku))
                      .ToList()
                      .ForEach(r => SetErrorMessage(errorList, r, string.Format("Sku {0} is invalid.", r.Sku)));

            // 4) sku division is equal to store division in rdq (just use the sku's division instead of hitting db.itemmasters)
            var invalidSkuStoreDivisionList = parsedRDQs.Where(pr => !invalidSkusList.Any(isl => isl.Equals(pr)) && !pr.Division.Equals(pr.Sku.Split('-')[0])).ToList();
            foreach (var r in invalidSkuStoreDivisionList)
            {
                string invalidSkuStoreDivErrorMessage = string.Format(
                    "The division for both the sku and store must be the same.  Sku Division: {0}, Store Division: {1}."
                    , r.Division
                    , r.Sku.Split('-')[0]);
                SetErrorMessage(errorList, r, invalidSkuStoreDivErrorMessage);
            }

            // 5) size is the correct length (3 for size, 5 for caselot)
            var uniqueSizeList = parsedRDQs.Select(pr => pr.Size).Distinct().ToList();
            var invalidSizeList = uniqueSizeList.Where(sl => !sl.Length.Equals(3) && !sl.Length.Equals(5)).ToList();
            parsedRDQs.Where(pr => invalidSizeList.Contains(pr.Size))
                      .ToList()
                      .ForEach(r =>
                      {
                          SetErrorMessage(errorList, r
                          , string.Format("The size {0} has an incorrect format.", r.Size));
                          parsedRDQs.Remove(r);
                      });

            var uniqueBinSkuSizeList = parsedRDQs.Where(pr => pr.Size.Length.Equals(3)).Select(pr => new { pr.Sku, pr.Size }).Distinct().ToList();
            var uniqueCaselotSizeList = parsedRDQs.Where(pr => pr.Size.Length.Equals(5)).Select(pr => new { pr.Sku, pr.Size }).Distinct().ToList();
            // check for bin sizes
            var invalidBinSizes = uniqueBinSkuSizeList.Where(sl => !config.db.Sizes.Any(s => s.Sku.Equals(sl.Sku) &&
                                                                                      s.Size.Equals(sl.Size) &&
                                                                                      s.InstanceID == instanceID)).ToList();

            parsedRDQs.Where(pr => invalidBinSizes.Contains(new { pr.Sku, pr.Size }))
                      .ToList()
                      .ForEach(r =>
                      {
                          SetErrorMessage(errorList, r, string.Format("The bin size {0} could not be found in our system.", r.Size));
                          parsedRDQs.Remove(r);
                      });

            // check for caselot schedules (need to rework)
            var uniqueItemIDs = uniqueItems.Select(ui => ui.ID).ToList();

            var itemCaseLots = config.db.ItemPacks.Where(ip => uniqueItemIDs.Contains(ip.ItemID)).ToList();

            foreach (var ucs in uniqueCaselotSizeList)
            {
                var isValid = (from ip in itemCaseLots
                               join im in uniqueItems
                                 on ip.ItemID equals im.ID
                               where ip.Name.Equals(ucs.Size) &&
                                     im.MerchantSku.Equals(ucs.Sku)
                               select ip).Any();
                if (!isValid)
                {
                    parsedRDQs.Where(pr => pr.Sku.Equals(ucs.Sku) && pr.Size.Equals(ucs.Size)).ToList().ForEach(r =>
                    {
                        SetErrorMessage(errorList, r, string.Format("The caselot schedule {0} could not be found in our system.", r.Size));
                        parsedRDQs.Remove(r);
                    });
                }
            }

            // 5) check to see if there was a supplied DC or RingFence
            // none supplied
            var invalidRDQNoneSuppliedList = parsedRDQs.Where(pr => string.IsNullOrEmpty(pr.DC) && string.IsNullOrEmpty(pr.RingFencePickStore)).ToList();

            invalidRDQNoneSuppliedList.ForEach(r => SetErrorMessage(errorList, r, "You must supply either a DC or a Ring Fence Store to pick from."));

            // both supplied
            var invalidRDQBothSuppliedList = parsedRDQs.Where(pr => !string.IsNullOrEmpty(pr.DC) && !string.IsNullOrEmpty(pr.RingFencePickStore)).ToList();

            invalidRDQBothSuppliedList.ForEach(r => SetErrorMessage(errorList, r,
                "You can't supply both a DC and a RingFence Store to pick from.  It must be one or the other."));

            // Can't supply a ring fence store for E-pick
            var invalidRDQRFStoreEpickList = parsedRDQs.Where(pr => pr.Status == "E-PICK" &&
                                                                    !string.IsNullOrEmpty(pr.RingFencePickStore)).ToList();
            invalidRDQRFStoreEpickList.ForEach(r => SetErrorMessage(errorList, r,
                "You can't supply a ring fence store for an E-Pick. It can only go against the DC."));

            // Can only do E-picks for warehouses supporting Kafka feed
            var invalidRDQKafkaEpickList = parsedRDQs.Where(pr => (pr.Status == "E-PICK") &&
                                                                   !kafkaWarehouseList.Contains(pr.DC)).ToList();
            invalidRDQKafkaEpickList.ForEach(r => SetErrorMessage(errorList, r, "This DC does not yet support E-Picks from Allocation."));

            invalidRDQRFStoreEpickList.ForEach(r => SetErrorMessage(errorList, r, "You can't supply a ring fence store for an E-Pick. It can only go against the DC."));

            // retreive parsed rdqs that are not in any of the lists above
            var validSuppliedRDQs = parsedRDQs.Where(pr => !invalidRDQBothSuppliedList.Any(ir => ir.Equals(pr)) &&
                                                           !invalidRDQNoneSuppliedList.Any(ir => ir.Equals(pr)) &&
                                                           !invalidRDQRFStoreEpickList.Any(ir => ir.Equals(pr)) &&
                                                           !invalidRDQKafkaEpickList.Any(ir => ir.Equals(pr))).ToList();

            // validate distribution center id
            List<string> uniqueDCList = validSuppliedRDQs.Select(pr => pr.DC).Where(dc => !string.IsNullOrEmpty(dc)).Distinct().ToList();
            var invalidDCList = uniqueDCList.Where(dc => !config.db.DistributionCenters.Any(dist => dist.MFCode.Equals(dc))).ToList();
            validSuppliedRDQs.Where(pr => invalidDCList.Contains(pr.DC))
                .ToList()
                .ForEach(r => SetErrorMessage(errorList, r, "DC is invalid."));

            List<DistributionCenter> dcList = config.db.DistributionCenters.ToList();

            // make sure DC is okay for PO scenario
            var invalidDCRDQList = (from pr in validSuppliedRDQs
                                    join dc in dcList
                                     on pr.DC equals dc.MFCode
                                    where pr.Size.Length == 3 &&
                                          dc.Type == "CROSSDOCK"
                                    select pr).ToList();

            validSuppliedRDQs.Where(pr => invalidDCRDQList.Any(ir => ir.Equals(pr)))
                .ToList()
                .ForEach(r => SetErrorMessage(errorList, r, "A crossdock-only DC was used for bin product"));

            errorList.ForEach(er => validSuppliedRDQs.Remove(er.Item1));

            // populate dcid and add all remaining dcRDQs to validRDQs
            List<string> uniqueDCs = validSuppliedRDQs.Select(r => r.DC).Distinct().ToList();
            var dcs = config.db.DistributionCenters.Where(dist => uniqueDCs.Contains(dist.MFCode)).ToList();
            foreach (var r in validSuppliedRDQs)
            {
                // retrieve specific dc
                var dc = dcs.Where(d => d.MFCode.Equals(r.DC)).FirstOrDefault();
                if (dc != null)
                {
                    r.DCID = dc.ID;
                }
            }

            // validate the inventory for dc rdqs.  This is the final validation for dc rdqs, 
            // so if it is valid, add the rdq to the validrdqs list for later processing
            var dcRDQs = validSuppliedRDQs.Where(sr => !invalidDCList.Contains(sr.DC) &&
                                                       !string.IsNullOrEmpty(sr.DC) &&
                                                       !invalidSkusList.Contains(sr.Sku) &&
                                                       string.IsNullOrEmpty(sr.PO)).ToList();

            var futureRDQs = validSuppliedRDQs.Where(sr => !invalidDCList.Contains(sr.DC) &&
                                                           !string.IsNullOrEmpty(sr.DC) &&
                                                           !invalidSkusList.Contains(sr.Sku) &&
                                                           !string.IsNullOrEmpty(sr.PO)).ToList();

            ValidateAvailableQuantityForDCRDQs(dcRDQs, validRDQs, errorList);
            ValidateFutureQuantityForDCRDQs(futureRDQs, validRDQs, errorList);

            // -------------------------------------------------------------------- Ringfence rdq validation --------------------------------------------------------------------

            // retrieve parsed rdqs that are not in any of the lists above and are ringfence picks
            var validSuppliedRingFenceList = validSuppliedRDQs.Where(pr => !string.IsNullOrEmpty(pr.RingFencePickStore)).ToList();

            // filter down to the unique rf rdqs by division, store, sku. (how we uniquely can identify a ringfence record)
            var uniqueRFCombos = validSuppliedRingFenceList.Select(rf => new { rf.Division, Store = rf.RingFencePickStore, rf.Sku }).Distinct().ToList();

            // validate ringfence rdqs to ensure they have a respective ringfence
            var invalidRingFenceList = uniqueRFCombos.Where(rfc => !config.db.RingFences.Any(rf => rf.Division.Equals(rfc.Division) &&
                                                                                             rf.Store.Equals(rfc.Store) &&
                                                                                             rf.Sku.Equals(rfc.Sku) &&
                                                                                             (rf.EndDate == null || rf.EndDate >= DateTime.Now))).ToList();

            validSuppliedRingFenceList.Where(pr => invalidRingFenceList.Contains(new { pr.Division, Store = pr.RingFencePickStore, pr.Sku }))
                .ToList()
                .ForEach(r => {
                    validSuppliedRingFenceList.Remove(r);
                    SetErrorMessage(errorList, r, "No ringfences for the sku and pick store were found.");
                });

            if (validSuppliedRingFenceList.Count > 0)
            {
                // check to see if there is a detail record for the rdq
                var uniqueRFBySizeCombos = validSuppliedRingFenceList.Select(rf => new { rf.Division, Store = rf.RingFencePickStore, rf.Sku, rf.Size }).Distinct().ToList();

                List<RingFenceDetail> ringFenceDetails = new List<RingFenceDetail>();
                foreach (var urf in uniqueRFBySizeCombos)
                {
                    var ringfenceAndDetail = (from rf in config.db.RingFences
                                              join rfd in config.db.RingFenceDetails
                                                on rf.ID equals rfd.RingFenceID
                                              where urf.Division.Equals(rf.Division) &&
                                                    urf.Sku.Equals(rf.Sku) &&
                                                    urf.Store.Equals(rf.Store) &&
                                                    (rf.EndDate == null || rf.EndDate >= DateTime.Now) &&
                                                    urf.Size.Equals(rfd.Size) &&
                                                    rfd.ringFenceStatusCode.Equals("4") &&
                                                    rfd.PO.Equals(string.Empty)
                                              select new { RingFence = rf, RingFenceDetail = rfd }).FirstOrDefault();
                    if (ringfenceAndDetail != null)
                    {
                        int sizeQuantity = validSuppliedRingFenceList.Where(vsr => vsr.Division.Equals(ringfenceAndDetail.RingFence.Division) &&
                                                                                   vsr.RingFencePickStore.Equals(ringfenceAndDetail.RingFence.Store) &&
                                                                                   vsr.Sku.Equals(ringfenceAndDetail.RingFence.Sku) &&
                                                                                   vsr.Size.Equals(ringfenceAndDetail.RingFenceDetail.Size)).Sum(vsr => vsr.Qty);

                        if (sizeQuantity > ringfenceAndDetail.RingFenceDetail.Qty)
                        {
                            validSuppliedRingFenceList.Where(vsr => vsr.Division.Equals(urf.Division) &&
                                                                vsr.RingFencePickStore.Equals(urf.Store) &&
                                                                vsr.Sku.Equals(urf.Sku) &&
                                                                vsr.Size.Equals(urf.Size))
                                                      .ToList()
                                                      .ForEach(r =>
                                                      {
                                                          validSuppliedRingFenceList.Remove(r);
                                                          SetErrorMessage(errorList, r, string.Format(
                                                              "The ring fence detail quantity found cannot satisfy the provided quantity. RingFence Available Quantity: {0}"
                                                              , ringfenceAndDetail.RingFenceDetail.Qty));
                                                      });
                        }
                    }
                    else
                    {
                        validSuppliedRingFenceList.Where(vsr => vsr.Division.Equals(urf.Division) &&
                                                                vsr.RingFencePickStore.Equals(urf.Store) &&
                                                                vsr.Sku.Equals(urf.Sku) &&
                                                                vsr.Size.Equals(urf.Size))
                                                  .ToList()
                                                  .ForEach(r =>
                                                  {
                                                      validSuppliedRingFenceList.Remove(r);
                                                      SetErrorMessage(errorList, r, "No ring fence detail record for the store and size provided was found.");
                                                  });
                    }

                }

                // add remaining ringfence rdqs to validrdqs
                validRDQs.AddRange(validSuppliedRingFenceList);
            }
        }

        private void ReduceRingFenceQuantities(List<RDQ> ringFenceRDQs)
        {
            var uniqueRFBySizeCombos = ringFenceRDQs.GroupBy(rf => new { rf.Division, Store = rf.RingFencePickStore, rf.Sku, rf.Size })
                                                    .Select(rf => new { rf.Key.Division, rf.Key.Store, rf.Key.Sku, rf.Key.Size, Quantity = rf.Sum(s => s.Qty) })
                                                    .Distinct().ToList();

            foreach (var urf in uniqueRFBySizeCombos)
            {
                var ringFenceDetail = (from rf in config.db.RingFences
                                       join rfd in config.db.RingFenceDetails
                                         on rf.ID equals rfd.RingFenceID
                                       where rf.Sku.Equals(urf.Sku) &&
                                             rf.Division.Equals(urf.Division) &&
                                             rf.Store.Equals(urf.Store) &&
                                             rfd.Size.Equals(urf.Size) &&
                                             rfd.ActiveInd.Equals("1") &&
                                             rfd.ringFenceStatusCode.Equals("4") &&
                                             (rf.EndDate == null ||
                                              rf.EndDate >= DateTime.Now)
                                       select rfd).FirstOrDefault();

                if (ringFenceDetail != null)
                {
                    var ringFenceHeader = config.db.RingFences.Where(rf => rf.ID == ringFenceDetail.RingFenceID).FirstOrDefault();

                    // this may not be the correct answer if the size is a caselot, but that's fine because it will be recalculated in the SaveChanges().
                    // this is primarily done to get the ring fence record in the update cache.
                    ringFenceHeader.Qty -= urf.Quantity;
                    ringFenceDetail.Qty -= urf.Quantity;

                    if (ringFenceDetail.Qty.Equals(0))
                    {
                        config.db.RingFenceDetails.Remove(ringFenceDetail);
                    }

                    // populate dcid for ringfencerdqs
                    var rdqs = ringFenceRDQs.Where(rf => rf.Division.Equals(urf.Division) &&
                                                         rf.Sku.Equals(urf.Sku) &&
                                                         rf.RingFencePickStore.Equals(urf.Store) &&
                                                         rf.Size.Equals(urf.Size)).ToList();
                    foreach (var r in rdqs)
                    {
                        r.DCID = ringFenceDetail.DCID;
                    }
                }
            }
        }

        private void PopulateRDQProps(List<RDQ> validRDQs, DateTime controlDate)
        {
            List<LegacyFutureInventory> futureInventory = new List<LegacyFutureInventory>();

            //unique skus
            var uniqueSkus = validRDQs.Select(r => r.Sku).Distinct().ToList();
            var uniqueItemMaster = config.db.ItemMasters.Where(im => uniqueSkus.Contains(im.MerchantSku)).Select(im => new { ItemID = im.ID, Sku = im.MerchantSku }).ToList();
            var uniqueFutureCombo = validRDQs.Where(r => !string.IsNullOrEmpty(r.PO)).Select(fr => new { fr.Division, fr.Sku, fr.Size, fr.DC, fr.PO, PODiv = fr.PO + "-" + fr.Division })
                                           .Distinct()
                                           .ToList();

            foreach (var uc in uniqueFutureCombo)
            {
                futureInventory.AddRange(config.db.LegacyFutureInventory.Where(lfi => lfi.Division == uc.Division &&
                                                                               lfi.Sku == uc.Sku &&
                                                                               lfi.Size == uc.Size &&
                                                                               lfi.InventoryID == uc.PODiv &&
                                                                               lfi.LocNodeType == "WAREHOUSE")
                                                                 .ToList());
            }

            foreach (var r in validRDQs)
            {
                r.CreatedBy = config.currentUser.NetworkID;
                r.LastModifiedUser = config.currentUser.NetworkID;
                r.CreateDate = DateTime.Now;
                r.ItemID = uniqueItemMaster.Where(uim => uim.Sku.Equals(r.Sku)).Select(uim => uim.ItemID).FirstOrDefault();
                r.Type = "user";

                if (r.Status == "E-PICK")
                {
                    r.TransmitControlDate = controlDate;
                    if (r.Size.Length == 3)
                        r.RecordType = "1";
                    else
                        r.RecordType = "4";
                }

                if (!string.IsNullOrEmpty(r.PO))
                {
                    LegacyFutureInventory futureInv = futureInventory.Where(fi => fi.PO == r.PO && fi.Sku == r.Sku && fi.Size == r.Size).FirstOrDefault();

                    if (futureInv.ProductNodeType == "PRODUCT_PACK")
                    {
                        r.DestinationType = "CROSSDOCK";
                        r.Status = "PICK-XDC";
                    }
                    else
                    {
                        r.DestinationType = "WAREHOUSE";
                    }
                }
                else
                {
                    r.PO = "";
                    r.DestinationType = "WAREHOUSE";
                }
            }
        }

        private void ApplyHoldAndCancelHolds(List<RDQ> validRDQs, List<Tuple<RDQ, string>> errorList, int instanceID, string division,
                                     List<ItemMaster> uniqueItems, List<string> uniqueSkus)
        {
            RDQDAO rdqDAO = new RDQDAO();
            // grab first division of rdq since we already validated that only one division is being used
            if (validRDQs.Count > 0)
            {
                int holdCount = rdqDAO.ApplyHolds(validRDQs, instanceID);
                if (holdCount > 0)
                {
                    // insert blank rdq and error message
                    RDQ rdq = new RDQ();
                    errorList.Insert(0, Tuple.Create(rdq, string.Format("{0} on hold.  Please go to Release Held RDQs to view held RDQs.", holdCount)));
                }

                List<RDQ> rejectedRDQs = rdqDAO.ApplyCancelHoldsNew(validRDQs, division, uniqueItems, uniqueSkus, config.currentUser.NetworkID);
                if (rejectedRDQs.Count > 0)
                {
                    rejectedRDQs.ForEach(r =>
                    {
                        SetErrorMessage(errorList, r, "Rejected by cancel inventory hold.");
                        validRDQs.Remove(r);
                    });
                }
            }
        }

        public void Save(HttpPostedFileBase attachment)
        {           
            LoadAttachment(attachment);
            if (!HasValidHeaderRow())                
                message = "Upload failed: Incorrect header - please use template.";
            else
            {
                int row = 1;

                try
                {
                    // create a local list of type RDQ to store the values from the upload
                    while (HasDataOnRow(row))
                    {
                        parsedRDQs.Add(ParseUploadRow(row));
                        row++;
                    }

                    // continue processing only if there is at least 1 record to upload
                    if (parsedRDQs.Count > 0)
                    {
                        // file level validations - duplicates, only one division, permission for division
                        if (!ValidateFile(parsedRDQs, errorList, out message))
                        {
                            return;
                        }

                        if (parsedRDQs.Count > 0)
                        {
                            var divCode = parsedRDQs.First().Sku.Substring(0, 2);

                            int instanceID = configService.GetInstance(divCode);
                            DateTime controlDate = configService.GetControlDate(instanceID);

                            List<string> uniqueSkus = parsedRDQs.Select(pr => pr.Sku).Distinct().ToList();
                            List<ItemMaster> uniqueItems = config.db.ItemMasters.Where(im => im.InstanceID == instanceID && 
                                                                                                uniqueSkus.Contains(im.MerchantSku)).ToList();

                            ValidateParsedRDQs(parsedRDQs, validRDQs, errorList, instanceID, uniqueSkus, uniqueItems);

                            // reduce ring fence quantities for the ring fence rdqs
                            ringFenceRDQs = validRDQs.Where(vr => !string.IsNullOrEmpty(vr.RingFencePickStore)).ToList();
                            ReduceRingFenceQuantities(ringFenceRDQs);

                            // populate necessary properties of RDQs to save to db
                            PopulateRDQProps(validRDQs, controlDate);

                            RDQDAO rdqDAO = new RDQDAO();
                            rdqDAO.InsertRDQs(validRDQs, config.currentUser.NetworkID);

                            List<RDQ> databaseRDQs = new List<RDQ>();

                            foreach (RDQ rdq in validRDQs)
                            {
                                List<RDQ> tempRDQ = config.db.RDQs.AsNoTracking().Where(r => r.Division == rdq.Division &&
                                                                                        r.Store == rdq.Store &&
                                                                                        r.DCID == rdq.DCID &&
                                                                                        r.PO == rdq.PO &&
                                                                                        r.Sku == rdq.Sku &&
                                                                                        r.Size == rdq.Size &&
                                                                                        r.Status == rdq.Status).ToList();
                                foreach (RDQ tempQueryRDQ in tempRDQ)
                                {
                                    databaseRDQs.Add(new RDQ()
                                    {
                                        ID = tempQueryRDQ.ID,
                                        Sku = tempQueryRDQ.Sku,
                                        Store = tempQueryRDQ.Store
                                    });
                                }
                            }

                            // once the rdqs are saved to the db, apply holds and cancel holds
                            ApplyHoldAndCancelHolds(databaseRDQs, errorList, instanceID, divCode, uniqueItems, uniqueSkus);
                        }
                    }

                    // if errors occured, allow user to download them
                    if (errorList.Count > 0)
                    {
                        errorMessage = string.Format("{0} errors were found and {1} lines were processed successfully. <br />You can review the quantity details on the Release Held RDQ page."
                            , errorList.Count
                            , validRDQs.Count);
                    }
                }
                catch (Exception ex)
                {
                    message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
                    FLLogger logger = new FLLogger(config.LogFile);
                    logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                    errorList.Clear();
                }
            }    
        }

        public Excel GetErrors(List<Tuple<RDQ, string>> errorList)
        {
            if (errorList != null)
            {
                Excel excelDocument = new Excel();
                Worksheet mySheet = excelDocument.Worksheets[0];
                int col = 0;
                mySheet.Cells[0, col].PutValue("Store (#####)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Warehouse (##)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("SKU (##-##-#####-##)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Size Caselot (### or #####)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("PO (#)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Quantity (#)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Pick from DC (##)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Pick from Ring Fence Store (#####)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Pick Right Away? (Y/N)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Message");
                mySheet.Cells[0, col].Style.Font.IsBold = true;

                int row = 1;
                if (errorList != null && errorList.Count > 0)
                {
                    foreach (var error in errorList)
                    {
                        col = 0;
                        if (error.Item1.Store != null && error.Item1.Store.Length == 5)
                            mySheet.Cells[row, col].PutValue(error.Item1.Store);
                        else
                            mySheet.Cells[row, col].PutValue("");

                        col++;

                        if (error.Item1.Store != null && error.Item1.Store.Length == 2)
                            mySheet.Cells[row, col].PutValue(error.Item1.Store);
                        else
                            mySheet.Cells[row, col].PutValue("");

                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Sku);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Size);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.PO);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Qty);
                        col++;
                        // if the DC is not populated and the Distribution Center object is populated and this is a DC RDQ
                        if (error.Item1.DC == null && error.Item1.DistributionCenter != null && string.IsNullOrEmpty(error.Item1.RingFencePickStore))
                        {
                            mySheet.Cells[row, col].PutValue(error.Item1.DistributionCenter.MFCode);
                        }
                        else
                        {
                            mySheet.Cells[row, col].PutValue(error.Item1.DC);
                        }
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.RingFencePickStore);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Status == "WEB PICK" ? "N" : "Y");
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item2);
                        row++;
                    }

                    for (int i = 0; i < maxColumns + 1; i++)
                    {
                        mySheet.AutoFitColumn(i);
                    }
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


        public WebPickSpreadsheet(AppConfig config, List<string> userRoles, ConfigService configService) : base(config, configService)
        {
            maxColumns = 9;

            columns.Add(0, "Store");
            columns.Add(1, "SKU");
            columns.Add(2, "Size");
            columns.Add(3, "PO");
            columns.Add(4, "Quantity");
            columns.Add(5, "To Warehouse");
            columns.Add(6, "Pick from DC");
            columns.Add(7, "Pick from Ring Fence Store");
            columns.Add(8, "Pick Right Away?");

            webPickRoles = userRoles;
            templateFilename = config.WebPickTemplate;
        }
    }
}