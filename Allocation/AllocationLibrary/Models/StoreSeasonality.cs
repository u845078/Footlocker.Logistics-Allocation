using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("StoreSeasonality")]
    public class StoreSeasonality
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string Division { get; set; }

        public ICollection<StoreSeasonalityDetail> Details { get; set; }

        [NotMapped]
        public int ValidDetailsCount
        {
            get
            {
                return (Details != null && Details.Count > 0) ?
                    Details.Where(d => d.ValidStore != null).Count() :
                    0;
            }
        }
    }
}
