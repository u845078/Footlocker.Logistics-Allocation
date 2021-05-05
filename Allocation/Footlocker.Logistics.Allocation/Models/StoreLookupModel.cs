using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Logistics.Allocation.Models;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [NotMapped]
    public class StoreLookupModel : StoreLookup
    {
        public StoreLookupModel() {}

        public StoreLookupModel(StoreLookup s)
        {
            foreach (PropertyInfo prop in s.GetType().GetProperties())
            {
                PropertyInfo prop2 = s.GetType().GetProperty(prop.Name);
                prop2.SetValue(this, prop.GetValue(s, null), null);
            }
        }

        public StoreLookupModel(StoreLookup s, Int64 currentPlan, Boolean inCurrentPlan)
        {
            //foreach (PropertyInfo prop in s.GetType().GetProperties())
            //{
            //    PropertyInfo prop2 = s.GetType().GetProperty(prop.Name);
            //    prop2.SetValue(this, prop.GetValue(s, null), null);
            //}
            this.AdHoc1 = s.AdHoc1;
            this.AdHoc2 = s.AdHoc2;
            this.AdHoc3 = s.AdHoc3;
            this.AdHoc4 = s.AdHoc4;
            this.AdHoc5 = s.AdHoc5;
            this.AdHoc6 = s.AdHoc6;
            this.AdHoc7 = s.AdHoc7;
            this.AdHoc8 = s.AdHoc8;
            this.AdHoc9 = s.AdHoc9;
            this.AdHoc10 = s.AdHoc10;
            this.AdHoc11 = s.AdHoc11;
            this.AdHoc12 = s.AdHoc12;
            this.City = s.City;
            this.Climate = s.Climate;
            //this.Concept = s.Concept;
            this.DBA = s.DBA;
            this.Division = s.Division;
            this.ExcludeStore = s.ExcludeStore;
            this.FirstReceipt = s.FirstReceipt;
            this.League = s.League;
            this.Mall = s.Mall;
            this.MarketArea = s.MarketArea;
            this.Region = s.Region;
            this.State = s.State;
            this.status = s.status;
            this.Store = s.Store;
            this.StoreType = s.StoreType;

            this.InCurrentPlan = inCurrentPlan;
            this.CurrentPlan = currentPlan;
        }

        public StoreLookupModel(StoreLookup s, Int64 currentPlan, Boolean inCurrentPlan, Boolean inSimilarRuleSet)
        {
            foreach (PropertyInfo prop in s.GetType().GetProperties())
            {
                PropertyInfo prop2 = s.GetType().GetProperty(prop.Name);
                prop2.SetValue(this, prop.GetValue(s, null), null);
            }
            this.InCurrentPlan = inCurrentPlan;
            this.CurrentPlan = currentPlan;
            this.InSimilarRuleSet = InSimilarRuleSet;
        }

        public Boolean InCurrentPlan { get; set; }
        public Int64 CurrentPlan { get; set; }

        public Boolean InSimilarRuleSet { get; set; }

        public Boolean InCurrentDeliveryGroup { get; set; }
    }
}