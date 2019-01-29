using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class WarehouseInventory
    {
        public long itemID { get; set; }
        public string Sku { get; set; }

        /// <summary>
        /// This is the mainframe code - ex. 08 = Junction City
        /// </summary>
        public string DistributionCenterID { get; set; }
        public DistributionCenter distributionCenter { get; set; }
        public string size { get; set; }
        public string PO { get; set; }
        public ItemPack caseLot { get; set; }
        public int quantity { get; set; }
        public int totalQuantity
        {
            get
            {
                if (caseLot != null)
                {
                    return caseLot.TotalQty * quantity;
                }
                else
                    return quantity;
            }
        }
        public string combinedQuantity
        {
            get
            {
                return quantity.ToString() + " (" + totalQuantity.ToString() + ")";
            }
        }

        public int pickReserve { get; set; }
        public int totalPickReserve
        {
            get
            {
                if (caseLot != null)
                {
                    return caseLot.TotalQty * pickReserve;
                }
                else
                    return pickReserve;
            }
        }

        public string combinedPickReserve
        {
            get
            {
                return pickReserve.ToString() + " (" + totalPickReserve.ToString() + ")";
            }
        }

        public int ringFenceQuantity { get; set; }
        public int totalRingFenceQuantity
        {
            get
            {
                if (caseLot != null)
                {
                    return caseLot.TotalQty * ringFenceQuantity;
                }
                else
                    return ringFenceQuantity;
            }
        }

        public string combinedRingFenceQuantity
        {
            get
            {
                return ringFenceQuantity.ToString() + " (" + totalRingFenceQuantity.ToString() + ")";
            }
        }

        public int orderQuantity { get; set; }

        public int rdqQuantity { get; set; }
        public int totalRDQQuantity
        {
            get
            {
                if (caseLot != null)
                {
                    return caseLot.TotalQty * rdqQuantity;
                }
                else
                    return rdqQuantity;
            }
        }
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
                return quantity - pickReserve - ringFenceQuantity - orderQuantity;
            }
        }
        public int totalAvailableQuantity
        {
            get
            {
                return totalQuantity - totalPickReserve - totalRingFenceQuantity - orderQuantity;
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

        public WarehouseInventory()
        {
            itemID = 0;
            DistributionCenterID = string.Empty;
            distributionCenter = null;
            Sku = string.Empty;
            size = string.Empty;
            PO = string.Empty;
            caseLot = null;
            quantity = 0;
            //totalQuantity = 0;
            pickReserve = 0;
            //totalPickReserve = 0;
            ringFenceQuantity = 0;
            //totalRingFenceQuantity = 0;
            rdqQuantity = 0;
            orderQuantity = 0;
            //totalRDQQuantity = 0;
        }

        public WarehouseInventory(string sku, string size, string DCID, int warehouseQuantity)
            : this()
        {
            Sku = sku;
            this.size = size;
            DistributionCenterID = DCID;
            quantity = warehouseQuantity;
        }

        public WarehouseInventory(string sku, string size, string DCID, int warehouseQuantity, int pickReserveQuantity)
            : this(sku, size, DCID, warehouseQuantity)
        {
            this.pickReserve = pickReserveQuantity;
        }

        public WarehouseInventory(string sku, string size, string DCID, string PO, int warehouseQuantity)
            : this(sku, size, DCID, warehouseQuantity)
        {
            this.PO = PO;
        }
    }
}
