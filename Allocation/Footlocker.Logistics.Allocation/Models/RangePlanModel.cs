using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.ComponentModel;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RangePlanModel 
    {
        public RangePlanModel()
        { }

        //public RangePlanModel(RangePlan plan)
        //{
        //    foreach (PropertyInfo prop in plan.GetType().GetProperties())
        //    {
        //        if (prop.Name != "ItemMaster")
        //        {
        //            PropertyInfo prop2 = plan.GetType().GetProperty(prop.Name);
        //            prop2.SetValue(this, prop.GetValue(plan, null), null);
        //        }
        //        else
        //        {
        //            if (plan.ItemMaster != null)
        //            {
        //                this.ItemMaster = plan.ItemMaster;
        //            }
        //        }
        //    }
        //}

    }
}