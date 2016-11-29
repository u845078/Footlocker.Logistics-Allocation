
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class VendorGroupDetailFactory
    {
        public VendorGroupDetail Create(DataRow dr)
        {
            VendorGroupDetail _newObject = new VendorGroupDetail();
            _newObject.GroupID = Convert.ToInt32(dr["GroupID"]);
            _newObject.CreateDate = Convert.ToDateTime(dr["CreateDate"]);
            _newObject.CreatedBy = Convert.ToString(dr["CreatedBy"]);
            _newObject.VendorName = Convert.ToString(dr["VendorName"]);
            _newObject.VendorNumber = Convert.ToString(dr["VendorNumber"]);

            return _newObject;
        }
    }
}