using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Entity;
using Footlocker.Logistics.Allocation.Services;

namespace Footlocker.Logistics.Allocation.Models
{
    public class NSSUpload
    {
        readonly public int MaxValues;
        readonly AllocationLibraryContext database;
        readonly List<DistributionCenter> DCs;
        public bool Valid 
        { 
            get
            {
                if (ErrorList.Count > 0)
                    return false;
                else
                    return true;
            }
        }
        public List<string> ErrorList;

        public string SubmittedDivision { get; set; }
        public string SubmittedStore { get; set; }
        public string SubmittedRank1 { get; set; }
        public string SubmittedRank2 { get; set; }
        public string SubmittedRank3 { get; set; }
        public string SubmittedRank4 { get; set; }
        public string SubmittedRank5 { get; set; }
        public string SubmittedRank6 { get; set; }

        public string SubmittedLeadtime1 { get; set; }
        public string SubmittedLeadtime2 { get; set; }
        public string SubmittedLeadtime3 { get; set; }
        public string SubmittedLeadtime4 { get; set; }
        public string SubmittedLeadtime5 { get; set; }
        public string SubmittedLeadtime6 { get; set; }

        public string Division { get; set; }
        public string Store { get; set; }

        public List<int> DCIDList { get; set; }
        public List<int> LeadtimeList { get; set; }

        public NSSUpload(AllocationLibraryContext db, List<DistributionCenter> dcList)
        {
            ErrorList = new List<string>();
            DCIDList = new List<int>();
            LeadtimeList = new List<int>();
            MaxValues = 6;
            database = db;
            DCs = dcList;
        }

        int ValidateLeadTime(string submittedLeadTime, string fieldName)
        {
            string NumberPattern = @"^[0-9]+$";
            int leadTime = -1;

            Regex NumberRegex = new Regex(NumberPattern);

            if (!string.IsNullOrEmpty(submittedLeadTime))
            {
                if (NumberRegex.IsMatch(submittedLeadTime))
                    leadTime = Convert.ToInt32(submittedLeadTime);
                else
                    ErrorList.Add(String.Format("Error - {0} does not look a valid number", fieldName));
            }

            return leadTime;
        }

        int ValidateDC(string submittedDC, string fieldName)
        {
            string TwoDigitPattern = @"^[0-9]{2}$";
            Regex TwoDigitRegex = new Regex(TwoDigitPattern);
            int DCID = -1;

            if (!string.IsNullOrEmpty(submittedDC))
            {
                if (TwoDigitRegex.IsMatch(submittedDC))
                {
                    if (DCs.Where(d => d.MFCode == submittedDC).Count() == 0)
                        ErrorList.Add(String.Format("Error - {0} is not a valid DC", fieldName));
                    else
                        DCID = DCs.Where(d => d.MFCode == submittedDC).FirstOrDefault().ID;
                }
                else
                    ErrorList.Add(String.Format("Error - {0} DC does not look valid", fieldName));
            }

            return DCID;
        }

        public void Validate()
        {
            string TwoDigitPattern = @"^[0-9]{2}$";
            string FiveDigitPattern = @"^[0-9]{5}$";
            
            Regex TwoDigitRegex = new Regex(TwoDigitPattern);
            Regex FiveDigitRegex = new Regex(FiveDigitPattern);

            if (TwoDigitRegex.IsMatch(SubmittedDivision))
                Division = SubmittedDivision;
            else
                ErrorList.Add("Error - Division does not look valid");

            if (SubmittedStore.Length < 5)
                SubmittedStore = SubmittedStore.PadLeft(5, '0');

            if (FiveDigitRegex.IsMatch(SubmittedStore))
                Store = SubmittedStore;
            else
                ErrorList.Add("Error - Store does not look valid");

            DCIDList.Add(ValidateDC(SubmittedRank1, "Rank 1"));
            DCIDList.Add(ValidateDC(SubmittedRank2, "Rank 2"));
            DCIDList.Add(ValidateDC(SubmittedRank3, "Rank 3"));
            DCIDList.Add(ValidateDC(SubmittedRank4, "Rank 4"));
            DCIDList.Add(ValidateDC(SubmittedRank5, "Rank 5"));
            DCIDList.Add(ValidateDC(SubmittedRank6, "Rank 6"));

            LeadtimeList.Add(ValidateLeadTime(SubmittedLeadtime1, "Leadtime 1"));
            LeadtimeList.Add(ValidateLeadTime(SubmittedLeadtime2, "Leadtime 2"));
            LeadtimeList.Add(ValidateLeadTime(SubmittedLeadtime3, "Leadtime 3"));
            LeadtimeList.Add(ValidateLeadTime(SubmittedLeadtime4, "Leadtime 4"));
            LeadtimeList.Add(ValidateLeadTime(SubmittedLeadtime5, "Leadtime 5"));
            LeadtimeList.Add(ValidateLeadTime(SubmittedLeadtime6, "Leadtime 6"));

            if (Valid)
            {
                int StoreCount = (from slt in database.StoreLeadTimes
                                  where slt.Division == Division &&
                                        slt.Store == Store
                                  select slt).Count();
                if (StoreCount == 0)
                    ErrorList.Add("The division and store is not already a part of NSS. You can only update via spreadsheet");
            }

            if (Valid)
            {
                for (int i = 0; i < MaxValues; i++)
                {
                    if (LeadtimeList[i] == -1 && DCIDList[i] != -1)
                        ErrorList.Add(String.Format("The lead time is empty for the DC ranked {0}", i));

                    if (LeadtimeList[i] != -1 && DCIDList[i] == -1)
                        ErrorList.Add(String.Format("The DC is empty for the lead time ranked {0}", i));
                }
            }
        }
    }
}
