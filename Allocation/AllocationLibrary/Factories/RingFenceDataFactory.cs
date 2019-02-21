using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class RingFenceDataFactory
    {
        AllocationLibraryContext db = new AllocationLibraryContext();

        public List<RingFenceDetail> CreateRFDetailsFromWarehouseInventory(List<WarehouseInventory> warehouseInventory, long? ringFenceID)
        {
            List<RingFenceDetail> newList = new List<RingFenceDetail>();
            RingFenceDetail rfd;
            List<RingFenceDetail> existingDetails = new List<RingFenceDetail>();

            if (ringFenceID != null)
            {
                // if you're editing a ring fence, we want to figure out the current quantity to take it out since it may be changed
                existingDetails = (from r in db.RingFenceDetails
                                   where r.RingFenceID == ringFenceID &&
                                         r.ActiveInd == "1" &&
                                         r.ringFenceStatusCode == "4" &&
                                         r.PO == ""
                                   select r).ToList();
            }

            foreach (WarehouseInventory wi in warehouseInventory)
            {
                if (wi.distributionCenter != null)
                {
                    rfd = new RingFenceDetail()
                    {
                        DCID = wi.distributionCenter.ID,
                        Warehouse = wi.distributionCenter.Name,
                        PO = "",
                        ActiveInd = "1",
                        Size = wi.size,
                        ringFenceStatusCode = "4"
                    };

                    if (ringFenceID != null)
                    {
                        int currentRFQty = (from ed in existingDetails
                                            where ed.DCID == rfd.DCID &&
                                                  ed.Size == rfd.Size
                                            select ed.Qty).FirstOrDefault();

                        rfd.Qty = currentRFQty;
                        rfd.RingFenceID = ringFenceID.Value;
                        rfd.AvailableQty = wi.availableQuantity + currentRFQty;
                    }

                    newList.Add(rfd);
                }
            }

            return newList;
        }
    }
}
