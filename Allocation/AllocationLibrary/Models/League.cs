using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("Leagues")]
    public class League
    {
        [Key, Column(Order = 0)]
        public int InstanceID { get; set; }

        [Key, Column("Div", Order = 1)]
        public string Division { get; set; }

        [Key, Column("League", Order = 2)]
        public string LeagueCode { get; set; }

        public string Description { get; set; }
    }
}
