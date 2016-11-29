
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class RDQFactory
    {
        public RDQ Create(DataRow dr)
        {
            RDQ _newObject = new RDQ();
            _newObject.ID = Convert.ToInt64(dr["ID"]);
            _newObject.CreateDate = Convert.ToDateTime(dr["CreateDate"]);
            _newObject.CreatedBy = Convert.ToString(dr["CreatedBy"]);
            _newObject.DCID = Convert.ToInt32(dr["DCID"]);
            _newObject.Division = Convert.ToString(dr["Division"]);
            _newObject.ItemID = Convert.ToInt64(dr["ItemID"]);
            _newObject.PO = Convert.ToString(dr["PO"]);
            _newObject.Qty = Convert.ToInt32(dr["Qty"]);
            if (dr.Table.Columns.Contains("type"))
            {
                _newObject.Type = Convert.ToString(dr["type"]);
            }
            if (dr.Table.Columns.Contains("Size"))
            {
                _newObject.Size = Convert.ToString(dr["Size"]);
            }
            if (dr.Table.Columns.Contains("Sku"))
            {
                _newObject.Sku = Convert.ToString(dr["Sku"]);
            }
            if (dr.Table.Columns.Contains("Store"))
            {
                _newObject.Store = Convert.ToString(dr["Store"]);
            }
            if (dr.Table.Columns.Contains("Name"))
            {
                _newObject.WarehouseName = Convert.ToString(dr["Name"]);
            }
            if (dr.Table.Columns.Contains("DestinationType"))
            {
                _newObject.DestinationType = Convert.ToString(dr["DestinationType"]);
            }
            if (dr.Table.Columns.Contains("Status"))
            {
                _newObject.Status = Convert.ToString(dr["Status"]);
            }
            if (dr.Table.Columns.Contains("Category"))
            {
                _newObject.Category = Convert.ToString(dr["Category"]);
            }

            return _newObject;
        }

        /// <summary>
        /// Creates an RDQ "Final" which is a manipulated RDQ for the mainframe.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public RDQ CreateFinal(DataRow dr)
        {
            RDQ _newObject = new RDQ();
            _newObject.ID = Convert.ToInt64(dr["ID"]);
            _newObject.CreateDate = Convert.ToDateTime(dr["CreateDate"]);
            _newObject.CreatedBy = Convert.ToString(dr["CreatedBy"]);
            _newObject.DCID = Convert.ToInt32(dr["DCID"]);
            _newObject.Division = Convert.ToString(dr["Division"]);
            _newObject.ItemID = Convert.ToInt64(dr["ItemID"]);
            _newObject.PO = Convert.ToString(dr["PO"]);
            _newObject.Qty = Convert.ToInt32(dr["Qty"]);
            if (dr.Table.Columns.Contains("type"))
            {
                _newObject.Type = Convert.ToString(dr["type"]);
            }
            if (dr.Table.Columns.Contains("Size"))
            {
                _newObject.Size = Convert.ToString(dr["Size"]);
            }
            if (dr.Table.Columns.Contains("Sku"))
            {
                _newObject.Sku = Convert.ToString(dr["Sku"]);
            }
            if (dr.Table.Columns.Contains("Store"))
            {
                _newObject.Store = Convert.ToString(dr["Store"]);
            }
            if (dr.Table.Columns.Contains("Name"))
            {
                _newObject.WarehouseName = Convert.ToString(dr["Name"]);
            }
            if (dr.Table.Columns.Contains("DestinationType"))
            {
                _newObject.DestinationType = Convert.ToString(dr["DestinationType"]);
            }
            if (dr.Table.Columns.Contains("Status"))
            {
                _newObject.Status = Convert.ToString(dr["Status"]);
            }
            if (dr.Table.Columns.Contains("Category"))
            {
                _newObject.Category = Convert.ToString(dr["Category"]);
            }
            _newObject.RecordType = Convert.ToInt32(dr["recordtype"]);
            _newObject.DC = Convert.ToString(dr["DC"]);

            return _newObject;
        }

    }
}