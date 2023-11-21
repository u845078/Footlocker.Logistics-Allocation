using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Spreadsheets
{
    public class RuleStoreSpreadsheet : UploadSpreadsheet
    {
        readonly RuleDAO ruleDAO;
        public List<StoreBase> validStores = new List<StoreBase>();

        private StoreBase ParseRow(int row)
        {
            message = string.Empty;
            string rangeType;

            StoreBase returnValue = new StoreBase()
            {
                Division = Convert.ToString(worksheet.Cells[row, 0].Value).PadLeft(2, '0'),
                Store = Convert.ToString(worksheet.Cells[row, 1].Value).PadLeft(5, '0')
            };

            if (worksheet.Cells[row, 2].Value != null)
                rangeType = worksheet.Cells[row, 2].Value.ToString();
            else
                rangeType = "ALR";

            if (rangeType == "ALR" || rangeType == "OP")
                returnValue.RangeType = rangeType;

            //always default to "Both"
            if (string.IsNullOrEmpty(returnValue.RangeType))
                returnValue.RangeType = "Both";

            return returnValue;
        }


        public void Save(HttpPostedFileBase attachment, long ruleSetID)
        {
            //StoreBase uploadRec;
            //List<StoreLookupModel> StoresInRules = null;
            //bool checkStore = true;

            //LoadAttachment(attachment);
            //if (!HasValidHeaderRow())
            //    message = "Incorrectly formatted or missing header row. Please correct and re-process.";
            //else
            //{
            //    int row = 1;
            //    RuleSet rs = ruleDAO.GetRuleSet(ruleSetID);
                
            //    if (rs.Type == "SizeAlc")
            //    {
            //        StoresInRules = new List<StoreLookupModel>();
            //        foreach (StoreLookup l in from a in config.db.RangePlanDetails
            //                                  join b in config.db.StoreLookups
            //                                  on new { a.Division, a.Store } equals new { b.Division, b.Store }
            //                                  where a.ID == rs.PlanID
            //                                  select b)
            //        {
            //            StoresInRules.Add(new StoreLookupModel(l, rs.PlanID.Value, true));
            //        }

            //        //delete rules
            //        List<Rule> rules = config.db.Rules.Where(r => r.RuleSetID == rs.RuleSetID).ToList();

            //        foreach (Rule rule in rules)
            //        {
            //            ruleDAO.Delete(rule);
            //        }
            //    }
            //    else 
            //    if (rs.Type == "Delivery")
            //    {
            //        StoresInRules = GetStoresForRules(ruleSetID, true);
            //    }
            //    else
            //        checkStore = false;

            //    try
            //    {
            //        while (HasDataOnRow(row))
            //        {
            //            uploadRec = ParseRow(row);

            //            errorMessage = ValidateUploadValues(uploadRec);
            //            if (!string.IsNullOrEmpty(errorMessage))
            //            {
            //                message = string.Format("Row {0}: {1}", row, errorMessage);
            //                return;
            //            }
            //            else
            //                validStores.Add(uploadRec);

            //            row++;
            //        }

            //        if (validStores.Count > 0)
            //            ruleDAO.AddStoresToPlan(validStores, planID);

            //    }
            //    catch (Exception ex)
            //    {
            //        message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
            //        FLLogger logger = new FLLogger(config.LogFile);
            //        logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
            //    }
            //}
        }

        public RuleStoreSpreadsheet(AppConfig config, ConfigService configService, RuleDAO ruleDAO) : base(config, configService)
        {
            maxColumns = 2;
            headerRowNumber = 0;

            columns.Add(0, "Division");
            columns.Add(1, "Store");

            templateFilename = config.StoreTemplate;
            this.ruleDAO = ruleDAO;
        }
    }
}