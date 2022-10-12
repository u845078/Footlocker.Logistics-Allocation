using System;
using System.Collections.Generic;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RDQGroup
    {
        #region Initializations

        public RDQGroup() { }

        #endregion

        #region Public Proprties
        public int InstanceID { get; set; }
        public string Division { get; set; }
        public string Store { get; set; }
        public string WarehouseName { get; set; }
        public string Category { get; set; }
        public long ItemID { get; set; }
        public string Sku { get; set; }
        public bool IsBin { get; set; }
        public int Qty { get; set; }
        public int? UnitQty { get; set; }
        public string Status { get; set; }

        public bool CanPick
        {
            get 
            {
                if (Status == null)
                    return true;

                return Status.StartsWith("HOLD") && (Status != "HOLD-XDC");
            }
        }

        public string StatusNoSpace
        {
            get
            {
                if (!string.IsNullOrEmpty(Status))
                    return Status.Replace(" ", "");
                else
                    return "";
            }
        }
        public string WarehouseNoSpace { get { return (""+this.WarehouseName).Replace(" ", ""); } }


        #endregion
    }
}
