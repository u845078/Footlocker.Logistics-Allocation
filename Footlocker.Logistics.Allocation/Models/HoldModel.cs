using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Models
{
    public class HoldModel : RuleFilterModel
    {
        public HoldModel() { }

        public HoldModel(Hold h)
        {
            this.Hold = h;
        }

        public Hold Hold { get; set; }

        public DateTime OriginalStartDate { get; set; }

        public List<Division> Divisions { get; set; }

        public string ShowStoreSelector { get; set; }

        //public Int64 HoldRuleSetID { get; set; }

        //public Rule RuleToAdd { get; set; }

        //private List<Rule> _rules = new List<Rule>();
        //public List<Rule> Rules { get { return _rules; } set { _rules = value; } }

        private RuleModel _ruleModel;

        public RuleModel RuleModel
        {
            get { return _ruleModel; }
            set { _ruleModel = value; }
        }

        //private List<StoreLookupModel> _newStores = new List<StoreLookupModel>();
        //public List<StoreLookupModel> NewStores { get { return _newStores; } set { _newStores = value; } }
    }
}