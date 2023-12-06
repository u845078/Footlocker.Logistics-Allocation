using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
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
