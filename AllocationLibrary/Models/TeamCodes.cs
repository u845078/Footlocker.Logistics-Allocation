using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("TeamCodes")]
    public class TeamCodes
    {
        [Key, Column("InstanceID", Order = 0)]
        public int InstanceID { get; set; }

        [Key, Column("Div", Order = 1)]
        public string DivisionCode { get; set; }

        [Key, Column("TeamCode", Order = 2)]
        public string TeamCode { get; set; }

        [Column("Description")]
        public string TeamCodeName { get; set; }

        [NotMapped]
        public string TeamCodeDisplay
        {
            get
            {
                return TeamCode + " - " + TeamCodeName;
            }
        }
    }
}
