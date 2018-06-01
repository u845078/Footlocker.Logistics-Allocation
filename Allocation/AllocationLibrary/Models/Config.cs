// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="">
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
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Config
    {
        [Key]
        [Column(Order=0)]
        public int InstanceID { get; set; }
        [Key]
        [Column(Order = 1)]
        public int ParamID { get; set; }
        public string Value { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdateDate { get; set; }

        public string ParamName 
        {
            get { return ConfigParam.Name; }
        }
        public virtual ConfigParam ConfigParam { get; set; }
    }
}
