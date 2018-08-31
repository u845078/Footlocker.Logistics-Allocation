
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Models.Factories
{
    public class ExistingPOFactory
    {
        public ExistingPO Create(DataRow dr)
        {
            ExistingPO _newObject = new ExistingPO();
            _newObject.PO = Convert.ToString(dr["PO_NUM"]);
            _newObject.Division = Convert.ToString(dr["RETL_OPER_DIV_CODE"]);
            _newObject.Sku = Convert.ToString(dr["RETL_OPER_DIV_CODE"]) + "-" +Convert.ToString(dr["stk_dept_num"]) + "-" +Convert.ToString(dr["stk_num"]) + "-" +Convert.ToString(dr["WDTH_COLOR_NUM"]);
            _newObject.ExpectedDeliveryDate = Convert.ToDateTime(dr["EXPECTED_DELV_DATE"]);
            _newObject.Description = Convert.ToString(dr["GENL_STK_DESC"]);
            _newObject.Retail = Convert.ToDecimal(dr["TOT_WC_RETL_AMT"]);
            _newObject.Units = Convert.ToInt32(dr["qty"]);
            _newObject.WarehouseNumber = Convert.ToString(dr["WHSE_ID_NUM"]);
            if (!Convert.IsDBNull(dr["WHSE_ID_NUM"]))
            {
                _newObject.DirectToStore = (Convert.ToString(dr["WHSE_ID_NUM"]).Trim().Length == 0);
            }
            else
            {
                _newObject.DirectToStore = false;
            }

            _newObject.POStatusCode = Convert.ToString(dr["po_status_code"]);
            _newObject.vendorNumber = Convert.ToString(dr["vend_num"]);
            _newObject.createDate = Convert.ToDateTime(dr["crte_date"]);
            _newObject.receivedQuantity = Convert.ToInt32(dr["receive_qty"]);

            return _newObject;
        }
    }
}