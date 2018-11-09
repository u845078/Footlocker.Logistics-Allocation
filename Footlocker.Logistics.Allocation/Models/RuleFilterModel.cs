using System;
using System.Collections.Generic;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RuleFilterModel
    {
        #region Fields

        private Rule _ruleToAdd = null;
        private long _ruleSetID = 0;

        #endregion

        #region Initializations

        public RuleFilterModel(long ruleSetID)
            : this()
        {
            RuleSetID = ruleSetID;
        }

        public RuleFilterModel()
        { }

        #endregion

        #region Public Properties

        public Rule RuleToAdd
        {
            get
            {
                if (_ruleToAdd == null) { _ruleToAdd = new Rule(); }
                return _ruleToAdd;
            }
        }
        
        public long RuleSetID 
        {
            get
            {
                return _ruleSetID;
            }
            set
            {
                _ruleSetID = value;

                // Update the 'rule to add'
                RuleToAdd.RuleSetID = value;
            }
        }
        
        //public string GridType { get; set; }
        //public string RuleType { get; set; }

        #endregion
    }
}