using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("Division")]
    public class AllocationDivision
    {
        [Column("DivCode")]
        [Key]
        public string DivisionCode { get; set; }

        public string DivisionName { get; set; }

        [Column("ConnectionName")]
        public string DatabaseConnection { get; set; }

        public string CountryOfOrigin { get; set; }

        public string PriorityCode { get; set; }

        public string DefaultCountryCode { get; set; }

        public short StartDateOffset { get; set; }

        public short EndDateOffset { get; set; }

        [Column("BlanketPOCapableInd")]
        public bool BlanketPOCapable { get; set; }

        [Column("HasSeparateECOMInventoryInd")]
        public bool HasSeparateECOMInventory { get; set; }
    }
}
