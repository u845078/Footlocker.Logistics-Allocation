using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Models
{
    public class BulkRDQModel : RuleFilterModel
    {
        public int Instance { get; set; }
        public List<Instance> Instances { get; set; }
        public string Store { get; set; }
        public string Division { get; set; }
        public List<Division> Divisions { get; set; }
        public string Department { get; set; }
        public List<Department> Departments { get; set; }
        public string Category { get; set; }
        public string Sku { get; set; }
        public string PO { get; set; }
        public string Status { get; set; }
        public List<string> StatusList { get; set; }
        public bool HaveResults { get; set; }
        public string ShowStoreSelector { get; set; }
        private RuleModel _ruleModel;

        public RuleModel RuleModel
        {
            get { return _ruleModel; }
            set { _ruleModel = value; }
        }

        public bool SearchResult { get; set; }

        public List<RDQ> RDQResults { get; set; }
        public List<RDQGroup> RDQGroups { get; set; }
    }
}