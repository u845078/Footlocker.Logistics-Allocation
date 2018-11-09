using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    /// <summary>
    /// Provides an object representation of an expedite purchase order factory.
    /// </summary>
    public class ExpeditePOFactory : IBiExtractFactory<ExpeditePO>
    {
        /// <summary>
        /// Create an expedite purchase order.
        /// </summary>
        /// <param name="reader">The data reader containing the expedite purchase order's properties.</param>
        /// <returns>The new expedite purchase order.</returns>
        public ExpeditePO Create(IDataReader reader)
        {
            string division = Convert.ToString(reader["Division"]);
            string po = Convert.ToString(reader["PO"]);
            DateTime deliveryDate = Convert.ToDateTime(reader["DeliveryDate"]);
            DateTime overrideDate = Convert.ToDateTime(reader["OverrideDate"]);
            string createdBy = Convert.ToString(reader["CreatedBy"]);
            DateTime createDate = Convert.ToDateTime(reader["CreateDTTM"]);

            return new ExpeditePO(division, po, deliveryDate, overrideDate, createdBy, createDate, String.Empty
                , String.Empty, Decimal.Zero, 0);
        }
    }
}
