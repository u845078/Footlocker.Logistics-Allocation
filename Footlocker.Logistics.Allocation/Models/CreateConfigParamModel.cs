using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class CreateConfigParamModel
    {
        public List<ConfigParam> Params { get; set; }
        public ConfigParam Param { get; set; }
    }
}