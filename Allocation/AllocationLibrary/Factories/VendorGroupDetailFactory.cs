using System;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class VendorGroupDetailFactory
    {
        public VendorGroupDetail Create(DataRow dr)
        {
            VendorGroupDetail _newObject = new VendorGroupDetail()
            {
                GroupID = Convert.ToInt32(dr["GroupID"]),
                CreateDate = Convert.ToDateTime(dr["CreateDate"]),
                CreatedBy = Convert.ToString(dr["CreatedBy"]),
                VendorName = Convert.ToString(dr["VendorName"]),
                VendorNumber = Convert.ToString(dr["VendorNumber"])
            };

            return _newObject;
        }
    }
}