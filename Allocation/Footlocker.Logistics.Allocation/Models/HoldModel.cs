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


        private RuleModel _ruleModel;

        public RuleModel RuleModel
        {
            get { return _ruleModel; }
            set { _ruleModel = value; }
        }
    }
}