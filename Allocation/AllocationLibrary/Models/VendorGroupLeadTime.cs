// -----------------------------------------------------------------------
// <copyright file="VendorGroupLeadTime.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Footlocker.Logistics.Allocation.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class VendorGroupLeadTime
    {
        [Key]
        [Column(Order=0)]
        public Int32 VendorGroupID { get; set; }

        [Key]
        [Column(Order = 1)]
        public Int32 ZoneID { get; set; }

        public String LeadTime { get; set; }

        public virtual NetworkZone Zone { get; set; }
        public virtual VendorGroup Group { get; set; }

        [NotMapped]
        public string ZoneName
        {
            get 
            {
                if (Zone != null)
                    return Zone.Name;

                return "";
            }
            set { }
        }

        [NotMapped]
        public string GroupName
        {
            get
            {
                if (Group != null)
                    return "VG"+Group.ID;

                return "";
            }
            set { }
        }

    }
}
