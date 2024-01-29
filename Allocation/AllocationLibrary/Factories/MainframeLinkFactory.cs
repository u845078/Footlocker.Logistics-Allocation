using System;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class MainframeLinkFactory
    {
        public MainframeLink Create(DataRow dr)
        {
            MainframeLink _newObject = new MainframeLink()
            {
                Division = Convert.ToString(dr["RETL_OPER_DIV_CODE"]),
                Store = Convert.ToString(dr["STR_NUM"]),
                Caselot = Convert.ToString(dr["CASELOT_NUMBER"]),
                SACC = Convert.ToString(dr["SACC_IND"]),
                Lock = Convert.ToString(dr["LOCK_IND"]),
                Warehouse = Convert.ToString(dr["WHSE_ID_NUM"])
            };

            try
            {
                _newObject.Qty = Convert.ToInt32(dr["XDOCK_INTRN_NUM"]);
            }
            catch { }
            
            return _newObject;
        }
    }
}