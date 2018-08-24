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
        public string DistributionCenterID { get; set; }
        public DistributionCenter distributionCenter { get; set; }
        public string size { get; set; }
        public string PO { get; set; }
        public ItemPack caseLot { get; set; }
        public int quantity { get; set; }
        public int totalQuantity { get; set; }
        public string combinedQuantity
        {
            get
            {
                return quantity.ToString() + " (" + totalQuantity.ToString() + ")";
            }
        }
        public int pickReserve { get; set; }
        public int totalPickReserve { get; set; }
        public string combinedPickReserve
        {
            get
            {
                return pickReserve.ToString() + " (" + totalPickReserve.ToString() + ")";
            }
        }
        public int ringFenceQuantity { get; set; }
        public int totalRingFenceQuantity { get; set; }
        public string combinedRingFenceQuantity
        {
            get
            {
                return ringFenceQuantity.ToString() + " (" + totalRingFenceQuantity.ToString() + ")";
            }
        }
        public int rdqQuantity { get; set; }
        public int totalRDQQuantity { get; set; }
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

        public WarehouseInventory()
            : base()
        {
            this.itemID = 0;
            this.DistributionCenterID = string.Empty;
            this.distributionCenter = null;
            this.Sku = string.Empty;
            this.size = string.Empty;
            this.PO = string.Empty;
            this.caseLot = null;
            this.quantity = 0;
            this.totalQuantity = 0;
            this.pickReserve = 0;
            this.totalPickReserve = 0;
            this.ringFenceQuantity = 0;
            this.totalRingFenceQuantity = 0;
            this.rdqQuantity = 0;
            this.totalRDQQuantity = 0;
        }

        public WarehouseInventory(string sku, string size, string DCID, int allocatableQuantity)
            : this()
        {
            this.Sku = sku;
            this.size = size;
            this.DistributionCenterID = DCID;
            this.totalQuantity = allocatableQuantity;
        }

        public WarehouseInventory(string sku, string size, string DCID, int allocatableQuantity, int pickReserveQuantity)
            : this(sku, size, DCID, allocatableQuantity)
        {
            this.totalPickReserve = pickReserveQuantity;
        }

        public WarehouseInventory(string sku, string size, string DCID, string PO, int allocatableQuantity)
            : this(sku, size, DCID, allocatableQuantity)
        {
            this.PO = PO;
        }
    }
}
