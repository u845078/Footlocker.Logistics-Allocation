using System;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public static class RDQFactory
    {
        public static RDQ Create(DataRow dr)
        {
            RDQ _newObject = new RDQ()
            {
                ID = Convert.ToInt64(dr["ID"]),
                CreateDate = Convert.ToDateTime(dr["CreateDate"]),
                CreatedBy = Convert.ToString(dr["CreatedBy"]),
                DCID = Convert.ToInt32(dr["DCID"]),
                Division = Convert.ToString(dr["Division"]),
                ItemID = Convert.ToInt64(dr["ItemID"]),
                PO = Convert.ToString(dr["PO"]),
                Qty = Convert.ToInt32(dr["Qty"])
            };

            if (dr.Table.Columns.Contains("type"))            
                _newObject.Type = Convert.ToString(dr["type"]);            

            if (dr.Table.Columns.Contains("Size"))
                _newObject.Size = Convert.ToString(dr["Size"]);

            if (dr.Table.Columns.Contains("Sku"))
                _newObject.Sku = Convert.ToString(dr["Sku"]);
            
            if (dr.Table.Columns.Contains("Store"))
                _newObject.Store = Convert.ToString(dr["Store"]);
            
            if (dr.Table.Columns.Contains("Name"))
                _newObject.WarehouseName = Convert.ToString(dr["Name"]);
            
            if (dr.Table.Columns.Contains("DestinationType"))
                _newObject.DestinationType = Convert.ToString(dr["DestinationType"]);
            
            if (dr.Table.Columns.Contains("Status"))
                _newObject.Status = Convert.ToString(dr["Status"]);
            
            if (dr.Table.Columns.Contains("Category"))
                _newObject.Category = Convert.ToString(dr["Category"]);

            return _newObject;
        }

        /// <summary>
        /// Creates an RDQ "Final" which is a manipulated RDQ for the mainframe.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static RDQ CreateFinal(DataRow dr)
        {
            RDQ _newObject = new RDQ()
            {
                ID = Convert.ToInt64(dr["ID"]),
                CreateDate = Convert.ToDateTime(dr["CreateDate"]),
                CreatedBy = Convert.ToString(dr["CreatedBy"]),
                DCID = Convert.ToInt32(dr["DCID"]),
                Division = Convert.ToString(dr["Division"]),
                ItemID = Convert.ToInt64(dr["ItemID"]),
                PO = Convert.ToString(dr["PO"]),
                Qty = Convert.ToInt32(dr["Qty"]),
                UnitQty = Convert.ToInt32(dr["UnitQty"]),
                RecordType = Convert.ToString(dr["recordtype"]),
                DC = Convert.ToString(dr["DC"])
            };

			if (dr.Table.Columns.Contains("type"))
                _newObject.Type = Convert.ToString(dr["type"]);

            if (dr.Table.Columns.Contains("Size"))
                _newObject.Size = Convert.ToString(dr["Size"]);

            if (dr.Table.Columns.Contains("Sku"))
                _newObject.Sku = Convert.ToString(dr["Sku"]);

            if (dr.Table.Columns.Contains("Store"))
                _newObject.Store = Convert.ToString(dr["Store"]);

            if (dr.Table.Columns.Contains("Name"))
                _newObject.WarehouseName = Convert.ToString(dr["Name"]);

            if (dr.Table.Columns.Contains("DestinationType"))
                _newObject.DestinationType = Convert.ToString(dr["DestinationType"]);

            if (dr.Table.Columns.Contains("Status"))
                _newObject.Status = Convert.ToString(dr["Status"]);

            if (dr.Table.Columns.Contains("Category"))
                _newObject.Category = Convert.ToString(dr["Category"]);
            
            if (dr.Table.Columns.Contains("RDQRejectReasonCode"))
            {
                int reasonCode;
                if (int.TryParse(dr["RDQRejectReasonCode"].ToString(), out reasonCode))                
                    _newObject.RDQRejectedReasonCode = reasonCode;                
            }
            return _newObject;
        }

        public static RDQ CreateFromHeldRDQRow(DataRow dr)
        {
            RDQ _newObject = new RDQ()
            {
                ID = Convert.ToInt64(dr["ID"]),
                Division = Convert.ToString(dr["Division"]),
                Store = Convert.ToString(dr["Store"]),
                DCID = Convert.ToInt32(dr["DCID"]),
                WarehouseName = Convert.ToString(dr["Name"]),
                PO = Convert.ToString(dr["PO"]),
                ItemID = Convert.ToInt64(dr["ItemID"]),
                Sku = Convert.ToString(dr["Sku"]),
                Size = Convert.ToString(dr["Size"]),
                Qty = Convert.ToInt32(dr["Qty"]),
                UnitQty = Convert.ToInt32(dr["UnitQty"]),
                CreatedBy = Convert.ToString(dr["CreatedBy"]),
                CreateDate = Convert.ToDateTime(dr["CreateDate"]),
                Type = Convert.ToString(dr["type"]),
                DestinationType = Convert.ToString(dr["DestinationType"]),
                Status = Convert.ToString(dr["Status"]),                
                Department = Convert.ToString(dr["Dept"]),
                InstanceID = Convert.ToInt32(dr["InstanceID"])
            };

            if (!string.IsNullOrEmpty(dr["RDQRejectReasonCode"].ToString()))
                _newObject.RDQRejectedReasonCode = Convert.ToInt32(dr["RDQRejectReasonCode"]);

            return _newObject;
        }

        public static RDQ CreateFromRingFence(RingFence ringFence, RingFenceDetail ringFenceDetail, WebUser user)
        {
            RDQ newRDQ = new RDQ()
            {
                Sku = ringFence.Sku,
                Size = ringFenceDetail.Size,
                Qty = ringFenceDetail.Qty,
                Store = ringFence.Store,
                Type = "user",
                Status = "WEB PICK",
                PO = ringFenceDetail.PO,
                Division = ringFence.Division,
                DCID = ringFenceDetail.DCID,
                ItemID = ringFence.ItemID,
                CreatedBy = user.NetworkID,
                CreateDate = DateTime.Now,
                LastModifiedUser = user.NetworkID
            };

            if (!string.IsNullOrEmpty(ringFenceDetail.PO))
            {
                newRDQ.DestinationType = "CROSSDOCK";
                newRDQ.Status = "HOLD-XDC";
            }
            else            
                newRDQ.DestinationType = "WAREHOUSE";            

            return newRDQ;
        }
    }
}