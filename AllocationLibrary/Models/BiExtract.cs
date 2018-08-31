using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    /// <summary>
    /// Provides an object representation of a business intelligence extract.
    /// </summary>
    public abstract class BiExtract : StringLayoutDelimitedUtility
    {
        public virtual Boolean CopyForAllStoresInDivision()
        {
            return false;
        }

        /// <summary>
        /// This method is to substitute the store for a blank store, it should return true if you want to write the record out,
        /// false if you don't.
        /// </summary>
        /// <param name="division"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        public virtual Boolean SubstituteStore(string division, string store)
        {
            return false;
        }
    }
    }
