using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class EditStoreModel
    {
        private List<StoreLookupModel> _currentStores;

        public List<StoreLookupModel> CurrentStores
        {
            get { return _currentStores; }
            set { _currentStores = value; }
        }

        private List<StoreLookupModel> _remainingStores;


        public List<StoreLookupModel> RemainingStores
        {
            get { return _remainingStores; }
            set { _remainingStores = value; }
        }

        public List<StoreLookupModel> AllStores {
            get {
                List<StoreLookupModel> list = new List<StoreLookupModel>();
                list.AddRange(_currentStores);
                list.AddRange(_remainingStores);
                return list;
            }

            set { }
        }
        

        public RangePlan plan { get; set; }
        public string Message { get; set; }
    }
}