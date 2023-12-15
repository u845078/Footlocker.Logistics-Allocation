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

        public List<string> SubmittedRank { get; set; }
        public List<string> SubmittedLeadtime { get; set; }

        public string SubmittedDivision { get; set; }
        public string SubmittedStore { get; set; }

        public string Division { get; set; }
        public string Store { get; set; }

        public List<int> DCIDList { get; set; }
        public List<int> LeadtimeList { get; set; }

        public string ErrorMessage { get; set; }

        public NSSUpload()
        {
            MaxValues = 10;
            LeadtimeList = new List<int>();
            SubmittedRank = new List<string>();
            SubmittedLeadtime = new List<string>();
            DCIDList = new List<int>();
        }
    }
}
