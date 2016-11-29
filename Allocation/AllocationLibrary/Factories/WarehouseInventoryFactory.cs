using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class WarehouseInventoryFactory
    {
        public static WarehouseInventory Create(DistributionCenter dc, long itemID, string size, int quantity, 
            int pickReserve, ItemPack caselot, int ringFenceQuantity, int rdqQuantity)
        {
            WarehouseInventory _newObject = new WarehouseInventory();
            _newObject.distributionCenter = dc;
            _newObject.itemID = itemID;
            _newObject.size = size;
            _newObject.quantity = quantity;
            _newObject.pickReserve = pickReserve;
            _newObject.ringFenceQuantity = ringFenceQuantity;
            _newObject.rdqQuantity = rdqQuantity;

            if (size.Length > 3)
            {
                _newObject.caseLot = caselot;

                if (caselot != null)
                {
                    _newObject.totalQuantity = quantity * caselot.TotalQty;
                    _newObject.totalPickReserve = pickReserve * caselot.TotalQty;
                    _newObject.totalRingFenceQuantity = ringFenceQuantity * caselot.TotalQty;
                    _newObject.totalRDQQuantity = rdqQuantity * caselot.TotalQty;
                }
            }
            else
            {
                _newObject.totalQuantity = quantity;
                _newObject.totalPickReserve = pickReserve;
                _newObject.totalRingFenceQuantity = ringFenceQuantity;
                _newObject.totalRDQQuantity = rdqQuantity;
            }

            return _newObject;
        }
    }
}
