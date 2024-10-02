using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("StoreBTS")]
    public class StoreBTS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }
        public int Year { get; set; }
        [NotMapped]
        public string DisplayName
        {
            get {
                return Year + " " + Name;
            }
        }

        [NotMapped]
        public int Count 
        { 
            get
            {
                if (Details != null)
                    return Details.Count;
                else
                    return 0;
            }
        }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string Division { get; set; }

        public virtual List<StoreBTSDetail> Details { get; set; }

        [NotMapped]
        public int ClusterID { get; set; }
    }
}
