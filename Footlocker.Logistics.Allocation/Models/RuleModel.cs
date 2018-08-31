using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RuleModel : RuleFilterModel
    {
        //private List<string> _fieldOptions;

        //public List<string> FieldOptions
        //{
        //    get { return _fieldOptions; }
        //    set { _fieldOptions = value; }
        //}

        //private List<string> _compareOptions;

        //public List<string> CompareOptions
        //{
        //    get { return _compareOptions; }
        //    set { _compareOptions = value; }
        //}

        public Int64 PlanID { get; set; }

        public RangePlan Plan { get; set; }

        private List<Rule> _rules;

        public List<Rule> Rules
        {
            get { return _rules; }
            set { _rules = value; }
        }

        //public Rule RuleToAdd { get; set; }

        private List<StoreLookupModel> _currentStores;

        public List<StoreLookupModel> CurrentStores
        {
            get { return _currentStores; }
            set { _currentStores = value; }
        }

        private List<StoreLookupModel> _newStores;

        public List<StoreLookupModel> NewStores
        {
            get { return _newStores; }
            set { _newStores = value; }
        }

        public string Message { get; set; }

    }
}