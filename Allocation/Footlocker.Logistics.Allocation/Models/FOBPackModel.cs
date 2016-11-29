using System;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Models
{
    public class FOBPackModel
    {
        #region Fields

        private FOBPack _domainObject = null;

        #endregion

        #region Public Properties

        public FOBPack DomainObject
        {
            get
            {
                return _domainObject;
            }
            set
            {
                _domainObject = value;

                // Set Override Count
                OverrideCount = (_domainObject != null && _domainObject.Overrides != null) ?
                    _domainObject.Overrides.Count :
                    0;

                // Set Overriden Depts string
                OverridenDeptsString = (_domainObject != null && _domainObject.Overrides != null && _domainObject.Overrides.Any()) ?
                    String.Join(",", _domainObject.Overrides.Select(o => (o.FOBDept != null) ? o.FOBDept.Department : String.Empty)) :
                    String.Empty;

                // Clear out Overrides property as will cause circ ref when serializing
                _domainObject.Overrides = null;
            }
        }

        public int OverrideCount { get; set; }
        public string OverridenDeptsString { get; set; }

        #endregion
    }
}