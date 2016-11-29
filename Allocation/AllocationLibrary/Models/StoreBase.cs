// -----------------------------------------------------------------------
// <copyright file="StoreBase.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Footlocker.Logistics.Allocation.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class StoreBase
    {
        public string Division { get; set; }
        public string Store { get; set; }

        private string _rangeType;

        public string RangeType
        {
            get { return _rangeType; }
            set { _rangeType = value; }
        }

    }
}
