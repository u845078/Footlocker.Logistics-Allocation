using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class CreateConfigModel
    {
        public List<ConfigParam> Params { get; set; }
        public Config Config { get; set; }
    }
}