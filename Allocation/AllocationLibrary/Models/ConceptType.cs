using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("ConceptTypes")]
    public class ConceptType
    {
        [Key]
        [Column("ID", Order = 0)]
        public int ID { get; set; }

        [Column("Name", Order = 1)]
        [Required]
        public string Name { get; set; }

        public ICollection<ConceptTypeDivision> Divisions { get; set; }

        // HACK: These properties are for the UI only and really should be in a ViewModel class
        // TODO: Create seperate ViewModel and mappings to and from model object...
        [NotMapped]
        public int DivisionCount
        {
            get
            {
                return Divisions != null ? Divisions.Count : 0;
            }
        }

        [NotMapped]
        public string DivisionString
        {
            get
            {
                return Divisions != null ?
                    String.Concat(Divisions.Select(d => d.Division + ",")).TrimEnd(',') :
                    String.Empty;
            }
        }

        [NotMapped]
        public bool IsUserWithAccess { get; set; }
    }
}