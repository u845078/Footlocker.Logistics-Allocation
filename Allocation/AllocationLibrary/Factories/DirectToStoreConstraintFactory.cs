
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class DirectToStoreConstraintFactory : IBiExtractFactory<DirectToStoreConstraint>
    {
        public DirectToStoreConstraint Create(DataRow dr)
        {
            DirectToStoreConstraint _newObject = new DirectToStoreConstraint();
            _newObject.CreateDate = Convert.ToDateTime(dr["CreateDate"]);
            _newObject.CreatedBy = Convert.ToString(dr["CreatedBy"]);
            if (!(Convert.IsDBNull(dr["EndDate"])))
            {
                _newObject.EndDate = Convert.ToDateTime(dr["EndDate"]);
            }
            _newObject.ItemID = Convert.ToInt64(dr["ItemID"]);
            _newObject.MaxQty = Convert.ToInt32(dr["MaxQty"]);
            _newObject.Size = Convert.ToString(dr["Size"]);
            _newObject.Sku = Convert.ToString(dr["Sku"]);
            _newObject.StartDate = Convert.ToDateTime(dr["StartDate"]);
            if (!(Convert.IsDBNull(dr["Description"])))
            {
                _newObject.Description = Convert.ToString(dr["Description"]);
            }

            return _newObject;
        }

        /// <summary>
        /// Create a direct to store constraint.
        /// </summary>
        /// <param name="reader">The data reader containing the direct to store constraint's properties.</param>
        /// <returns>The new direct to store constraint.</returns>
        public DirectToStoreConstraint Create(IDataReader reader)
        {
            string sku = Convert.ToString(reader["Sku"]);
            string size = Convert.ToString(reader["Size"]);
            int maxQty = Convert.ToInt32(reader["MaxQty"]);
            DateTime startDate = Convert.ToDateTime(reader["StartDate"]);
            DateTime? endDate
                = Convert.IsDBNull(reader["EndDate"]) ? new DateTime?()
                    : new DateTime?(Convert.ToDateTime(reader["EndDate"]));
            string createdBy = Convert.ToString(reader["CreatedBy"]);
            DateTime createDate = Convert.ToDateTime(reader["CreateDTTM"]);
            int vendorPackQty = Convert.ToInt32(reader["VendorPackQty"]);

            return new DirectToStoreConstraint(sku, 0L, size, maxQty, startDate, endDate, createdBy, createDate
                , String.Empty, vendorPackQty);
        }
    }
}