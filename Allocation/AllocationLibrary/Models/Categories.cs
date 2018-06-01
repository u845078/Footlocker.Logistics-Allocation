using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("Categories")]
    public class Categories
    {
        [Key, Column("InstanceID", Order = 0)]
        public int instanceID { get; set; }

        [Key, Column("Div", Order = 1)]
        public string divisionCode { get; set; }

        [Key, Column("Dept", Order = 2)]
        public string departmentCode { get; set; }

        [Key, Column("Category", Order = 3)]
        public string categoryCode { get; set; }

        [Column("Description")]
        public string CategoryName { get; set; }

        [NotMapped]
        public string CategoryDisplay
        {
            get
            {
                return categoryCode + " - " + CategoryName;
            }
        }
    }
}
