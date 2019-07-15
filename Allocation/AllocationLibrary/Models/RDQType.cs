using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("RDQTypes")]
    public class RDQType
    {
        [Key]
        public int RDQTypeID { get; set; }

        public string RDQTypeName { get; set; }

        public string RDQTypeDescription { get; set; }

        public string RDQTypeDisplayValue
        {
            get
            {
                return string.Format("{0} - {1}", this.RDQTypeName, this.RDQTypeDescription);
            }
        }

        public RDQType()
            : base()
        {
            this.RDQTypeID = 0;
            this.RDQTypeName = string.Empty;
            this.RDQTypeDescription = string.Empty;
        }
    }
}
