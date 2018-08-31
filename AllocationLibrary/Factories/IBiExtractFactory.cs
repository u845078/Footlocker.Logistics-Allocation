using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    /// <summary>
    /// Defines the business intelligence extract factory interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBiExtractFactory<out T> where T : BiExtract
    {
        /// <summary>
        /// Create a new business intelligence extract.
        /// </summary>
        /// <param name="reader">The data reader containing the business intelligence extract's properties.</param>
        /// <returns>The new business intelligence extract.</returns>
        T Create(IDataReader reader);
    }
}
