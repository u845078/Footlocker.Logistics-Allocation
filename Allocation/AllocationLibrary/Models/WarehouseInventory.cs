using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class WarehouseInventory
    {
        public long itemID;
        public DistributionCenter distributionCenter;
        public string size;
        public ItemPack caseLot;
        public int quantity;
        public int totalQuantity;

        public string combinedQuantity
        {
            get
            {
                return quantity.ToString() + " (" + totalQuantity.ToString() + ")";
            }
        }
       
        public int pickReserve;
        public int totalPickReserve;

        public string combinedPickReserve
        {
            get
            {
                return pickReserve.ToString() + " (" + totalPickReserve.ToString() + ")";
            }
        }

        public int ringFenceQuantity;
        public int totalRingFenceQuantity;
        public string combinedRingFenceQuantity
        {
            get
            {
                return ringFenceQuantity.ToString() + " (" + totalRingFenceQuantity.ToString() + ")";
            }
        }

        public int rdqQuantity;
        public int totalRDQQuantity;
        public string combinedRDQQuantity
        {
            get
            {
                return rdqQuantity.ToString() + " (" + totalRDQQuantity.ToString() + ")";
            }
        }

        public int availableQuantity
        {
            get
            {
                return quantity - pickReserve - ringFenceQuantity;
            }
        }

        public int totalAvailableQuantity
        {
            get
            {
                return totalQuantity - totalPickReserve - totalRingFenceQuantity;
            }
        }

        public string combinedAvailableQuantity
        {
            get
            {
                return availableQuantity.ToString() + " (" + totalAvailableQuantity.ToString() + ")";
            }
        }

        public int sizeNumber
        {
            get
            {
                return Convert.ToInt32(size);
            }
        }
    }
}
