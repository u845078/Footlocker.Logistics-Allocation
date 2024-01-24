using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("ConceptTypeDivisions")]
    public class ConceptTypeDivision
    {
        [Key]
        [Column("ID", Order = 0)]
        public int ID { get; set; }

        public int ConceptTypeID { get; set; }
        public string Division { get; set; }
    }
}
