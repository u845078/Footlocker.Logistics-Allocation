using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class CheckBoxModel
    {
        public int ID { get; set; }
        public string Desc { get; set; }
        public bool Checked { get; set; }
    }
}