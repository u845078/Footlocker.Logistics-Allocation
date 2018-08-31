
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class MainframeLinkFactory
    {
        public MainframeLink Create(DataRow dr)
        {
            MainframeLink _newObject = new MainframeLink();
            _newObject.Division = Convert.ToString(dr["RETL_OPER_DIV_CODE"]);
            _newObject.Store = Convert.ToString(dr["STR_NUM"]);
            _newObject.Caselot = Convert.ToString(dr["CASELOT_NUMBER"]);
            _newObject.SACC = Convert.ToString(dr["SACC_IND"]);
            _newObject.Lock = Convert.ToString(dr["LOCK_IND"]);
            _newObject.Warehouse = Convert.ToString(dr["WHSE_ID_NUM"]);
            try
            {
                _newObject.Qty = Convert.ToInt32(dr["XDOCK_INTRN_NUM"]);
            }
            catch { }
            
            return _newObject;
        }
    }
}