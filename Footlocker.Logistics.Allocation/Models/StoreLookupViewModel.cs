using System;
using System.Collections.Generic;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreLookupViewModel
    {
        #region Fields

        private StoreLookup _entity = null;

        #endregion

        #region Initializations

        public StoreLookupViewModel() { }

        public StoreLookupViewModel(StoreLookup entity)
        {
            Entity = entity;
        }

        #endregion

        #region Public Properties

        public StoreLookup Entity { get; set; }

        public IList<ConceptType> ConceptTypes { get; set; }

        public IList<CustomerType> CustomerTypes { get; set; }

        public IList<PriorityType> PriorityTypes { get; set; }

        public IList<StrategyType> StrategyTypes { get; set; }

        #endregion
    }
}