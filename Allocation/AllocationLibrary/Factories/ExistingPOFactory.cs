using System;
using System.Data;

namespace Footlocker.Logistics.Allocation.Models.Factories
{
    public class ExistingPOFactory
    {
        public ExistingPO Create(DataRow dr)
        {
            ExistingPO _newObject = new ExistingPO()
            {
                PO = Convert.ToString(dr["PO_NUM"]),
                Division = Convert.ToString(dr["RETL_OPER_DIV_CODE"]),
                Sku = string.Format("{0}-{1}-{2}-{3}", Convert.ToString(dr["RETL_OPER_DIV_CODE"]), Convert.ToString(dr["stk_dept_num"]), Convert.ToString(dr["stk_num"]), Convert.ToString(dr["WDTH_COLOR_NUM"])),
                ExpectedDeliveryDate = Convert.ToDateTime(dr["EXPECTED_DELV_DATE"]),
                Description = Convert.ToString(dr["GENL_STK_DESC"]),
                Retail = Convert.ToDecimal(dr["TOT_WC_RETL_AMT"]),
                Units = Convert.ToInt32(dr["qty"]),
                WarehouseNumber = Convert.ToString(dr["WHSE_ID_NUM"]),
                POStatusCode = Convert.ToString(dr["po_status_code"]),
                vendorNumber = Convert.ToString(dr["vend_num"]),
                createDate = Convert.ToDateTime(dr["crte_date"]),
                receivedQuantity = Convert.ToInt32(dr["receive_qty"])
            };

            if (!Convert.IsDBNull(dr["WHSE_ID_NUM"]))            
                _newObject.DirectToStore = Convert.ToString(dr["WHSE_ID_NUM"]).Trim().Length == 0;            
            else            
                _newObject.DirectToStore = false;

            return _newObject;
        }
    }
}