using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    public class PurgeArchiveTypeModel
    {
        public PurgeArchiveTypeModel()
        {
            Instances = new List<InstanceModel>();
        }
        public PurgeArchiveType purgeArchiveType { get; set; }
        public List<InstanceModel> Instances { get; set; }
        public List<PurgeArchiveType> PurgeArchiveTypes { get; set; }
        public bool CanEdit { get; set; }
    }
}