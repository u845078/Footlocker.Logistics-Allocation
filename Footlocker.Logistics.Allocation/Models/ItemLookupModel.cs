using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class ItemLookupModel
    {
        public string QItem { get; set; }

        [Display(Name="MerchantSku (##-##-#####-##)")]
        public string MerchantSku { get; set; }

        public List<ItemMaster> noSizeItems { get; set; }
        public List<ItemMaster> sizeItems { get; set; }

    }
}
