using System;
using System.Collections.Generic;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Models
{
    public class FOBPackOverrideModel
    {
        #region Initializations

        public FOBPackOverrideModel(FOBPackOverride domainObject, ICollection<Footlocker.Common.Department> departments)
        {
            DomainObject = domainObject;
            Departments = departments;
        }

        #endregion

        #region Public Properties
        
        public ICollection<Footlocker.Common.Department> Departments { get; private set; }

        public FOBPackOverride DomainObject { get; private set; }

        #endregion
    }
}