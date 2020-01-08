using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreBTSGridExtract
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string Division { get; set; }
        public int ClusterID { get; set; }
        public List<StoreLookup> StoreLookups { get; set; }
    }
}
