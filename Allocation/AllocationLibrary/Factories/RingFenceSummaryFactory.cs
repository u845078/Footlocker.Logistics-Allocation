using System;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class RingFenceSummaryFactory
    {
        public RingFenceSummary Create(DataRow dr)
        {
            RingFenceSummary _newObject = new RingFenceSummary()
            {
                Sku = Convert.ToString(dr["Sku"]),
                Size = Convert.ToString(dr["Size"]),
                DC = Convert.ToString(dr["MFCode"]),
                PO = Convert.ToString(dr["PO"]),
                Qty = Convert.ToInt32(dr["qty"])
            };

            return _newObject;
        }
    }
}
