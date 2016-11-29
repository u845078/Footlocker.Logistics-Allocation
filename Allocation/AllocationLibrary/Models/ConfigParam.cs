// -----------------------------------------------------------------------
// <copyright file="ConfigParams.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Footlocker.Logistics.Allocation.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ConfigParam
    {
        [Key]
        public int ParamID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Comment { get; set; }

        //public virtual List<Config> Configs { get; set; }
    }
}
