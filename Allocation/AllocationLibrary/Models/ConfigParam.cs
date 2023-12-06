using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class ConfigParam
    {
        [Key]
        public int ParamID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Comment { get; set; }
    }
}
