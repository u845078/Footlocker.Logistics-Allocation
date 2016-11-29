using System;
using System.Collections.Generic;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreBatchModel
    {
        public long RuleSetID { get; set; }
        public bool IsRestrictingToUnassignedCustomer { get; set; }
        public IList<StoreLookup> Stores { get; set; }
        public string NotificationMessage { get; set; }

        public int SelectedConceptTypeID { get; set; }
        public int SelectedCustomerTypeID { get; set; }
        public int SelectedPriorityTypeID { get; set; }
        public int SelectedStrategyTypeID { get; set; }

        public int SelectedExcludeStore { get; set; }
        public DateTime? FirstReceipt { get; set; }

        public IList<ConceptType> ConceptTypes { get; set; }
        public IList<CustomerType> CustomerTypes { get; set; }
        public IList<PriorityType> PriorityTypes { get; set; }
        public IList<StrategyType> StrategyTypes { get; set; }
    }
}