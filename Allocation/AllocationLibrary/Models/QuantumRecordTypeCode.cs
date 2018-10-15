using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("QuantumRecordTypeCodes")]
    public class QuantumRecordTypeCode
    {
        [Key]
        [Column("QuantumRecordTypeCode")]
        public string RecordTypeCode { get; set; }

        [Column("QuantumRecordTypeDesc")]
        public string RecordTypeDesc { get; set; }

        public string DisplayValue
        {
            get
            {
                return RecordTypeCode + " - " + RecordTypeDesc;
            }
        }
    }
}
