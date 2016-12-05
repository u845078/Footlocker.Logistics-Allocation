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
        public String Store { get; set; }
        public string Division { get; set; }
        public List<Division> Divisions { get; set; }
        public String Department { get; set; }
        public List<Department> Departments { get; set; }
        public String Category { get; set; }
        public String Sku { get; set; }
        public String PO { get; set; }
        public String Status { get; set; }
        public List<String> StatusList { get; set; }
        public Boolean HaveResults { get; set; }
        public string ShowStoreSelector { get; set; }
        private RuleModel _ruleModel;

        public RuleModel RuleModel
        {
            get { return _ruleModel; }
            set { _ruleModel = value; }
        }

        public List<RDQ> Results { get; set; }
    }
}