using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    class FoqExtractFactory : IBiExtractFactory<FoqExtract>
    {
        /// <summary>
        /// Create a store back to school extract.
        /// </summary>
        /// <param name="reader">The data reader containing the store back to school extract's properties.</param>
        /// <returns>The new store back to school extract.</returns>
        public FoqExtract Create(IDataReader reader)
        {
            FoqExtract roq = new FoqExtract();

            roq.PullId = Convert.ToInt32(reader["pullid"]);
            roq.Division = Convert.ToString(reader["StoreDivision"]);
            roq.Store = Convert.ToString(reader["Store"]);
            roq.DCQty = Convert.ToDecimal(reader["DCQty"]);
            roq.FinalQty = Convert.ToDecimal(reader["FinalQty"]);
            roq.RawQty = Convert.ToDecimal(reader["RawQty"]);
            roq.Ratio = Convert.ToDecimal(reader["Ratio"]);

            roq.ItemId = Convert.ToInt64(reader["ItemID"]);
            roq.MerchantSku = Convert.ToString(reader["MerchantSku"]);
            roq.Size = Convert.ToString(reader["Size"]);

            roq.SessionDate = Convert.ToDateTime(reader["SessionDate"]);
            roq.WeekStartDate = Convert.ToDateTime(reader["WeekStartDate"]);

            return roq;
        }
    }
}
