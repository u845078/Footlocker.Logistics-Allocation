using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("vActiveHolds")]
    public class ActiveHolds
    {
        public long ID { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Category { get; set; }
        public string Store { get; set; }
        public string Level { get; set; }
        public string Value { get; set; }
        public string Vendor { get; set; }
        public string Brand { get; set; }
        public string Team { get; set; }
        public string SKU { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Int16 ReserveInventory { get; set; }
        public string Duration { get; set; }
        public string Comments { get; set; }
    }
}
