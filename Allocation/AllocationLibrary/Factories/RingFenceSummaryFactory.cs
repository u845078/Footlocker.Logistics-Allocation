
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class RingFenceSummaryFactory
    {
        public RingFenceSummary Create(DataRow dr)
        {
            RingFenceSummary _newObject = new RingFenceSummary();
            _newObject.Sku = Convert.ToString(dr["Sku"]);
            _newObject.Size = Convert.ToString(dr["Size"]);
            _newObject.DC = Convert.ToString(dr["MFCode"]);
            _newObject.PO = Convert.ToString(dr["PO"]);
            _newObject.Qty = Convert.ToInt32(dr["qty"]);

            return _newObject;
        }
    }
}
