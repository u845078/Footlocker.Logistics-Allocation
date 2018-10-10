using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Footlocker.Logistics.Allocation.Models;
using System.Data.Entity.Infrastructure;
using System.Data;

namespace Footlocker.Logistics.Allocation.Services
{
    public class AllocationLibraryContext : DbContext
    {
        public DbSet<Footlocker.Logistics.Allocation.Models.Rule> Rules { get; set; }
        public DbSet<RuleSet> RuleSets { get; set; }
        public DbSet<StoreLookup> StoreLookups { get; set; }
        public DbSet<RangePlanDetail> RangePlanDetails { get; set; }
        public DbSet<RangePlan> RangePlans { get; set; }
        public DbSet<SizeAllocation> SizeAllocations { get; set; }
        public DbSet<StorePlan> StorePlans { get; set; }
        public DbSet<Instance> Instances { get; set; }
        public DbSet<InstanceDivision> InstanceDivisions { get; set; }
        public DbSet<NetworkLeadTime> NetworkLeadTimes { get; set; }
        public DbSet<NetworkZone> NetworkZones { get; set; }
        public DbSet<NetworkZoneStore> NetworkZoneStores { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<RouteDetail> RouteDetails { get; set; }
        public DbSet<DistributionCenter> DistributionCenters { get; set; }
        public DbSet<Hold> Holds { get; set; }
        public DbSet<RingFence> RingFences { get; set; }
        public DbSet<RingFenceDetail> RingFenceDetails { get; set; }
        public DbSet<RingFenceType> RingFenceTypes { get; set; }
        public DbSet<ItemMaster> ItemMasters { get; set; }
        public DbSet<RingFenceHistory> RingFenceHistory { get; set; }
        public DbSet<RDQ> RDQs { get; set; }
        public DbSet<AuditRDQ> AuditRDQs { get; set; }
        public DbSet<WarehouseBlackout> WarehouseBlackouts { get; set; }
        public DbSet<StoreSeasonality> StoreSeasonality { get; set; }
        public DbSet<StoreSeasonalityDetail> StoreSeasonalityDetails { get; set; }
        public DbSet<ExpeditePO> ExpiditePOs { get; set; }
        public DbSet<DirectToStoreConstraint> DirectToStoreConstraints { get; set; }
        public DbSet<DirectToStoreSku> DirectToStoreSkus { get; set; }
        public DbSet<RuleSelectedStore> RuleSelectedStores { get; set; }
        public DbSet<ItemPack> ItemPacks { get; set; }
        public DbSet<ItemPackDetail> ItemPackDetails { get; set; }
        public DbSet<StoreLeadTime> StoreLeadTimes { get; set; }
        public DbSet<RouteDistributionCenter> RouteDistributionCenters { get; set; }
        public DbSet<StoreAttribute> StoreAttributes { get; set; }
        public DbSet<CrossDockExclusion> CrossDockExclusions { get; set; }
        public DbSet<EcommWarehouse> EcommWarehouses { get; set; }
        public DbSet<EcommInventory> EcommInventory { get; set; }
        public DbSet<ControlDate> ControlDates { get; set; }
        public DbSet<SizeObj> Sizes { get; set; }
        public DbSet<StoreCluster> StoreClusters { get; set; }
        public DbSet<ValidStoreLookup> vValidStores { get; set; }
        public DbSet<DeliveryGroup> DeliveryGroups { get; set; }
        public DbSet<MaxLeadTime> MaxLeadTimes { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<ConfigParam> ConfigParams { get; set; }
        public DbSet<OrderPlanningRequest> OrderPlanningRequests { get; set; }
        public DbSet<RingFenceStatusCodes> RingFenceStatusCodes { get; set; }
        public DbSet<PurgeArchiveType> PurgeArchiveTypes { get; set; }
        public DbSet<Departments> Departments { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<BrandIDs> BrandIDs { get; set; }
        public DbSet<InventoryReductions> InventoryReductions { get; set; }
        public DbSet<ProductHierarchyOverrides> ProductOverrides { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<MinihubStore> MinihubStores { get; set; }
        public DbSet<CancelInventoryHoldsNextBatch> CancelInventoryHolds { get; set; }
        public DbSet<QuantumRecordTypeCode> QuantumRecordTypes { get; set; }

        public DbSet<Vendors> Vendors { get; set; }

        public AllocationLibraryContext()
            : base("AllocationContext")
        {
            // Get the ObjectContext related to this DbContext
            var objectContext = (this as IObjectContextAdapter).ObjectContext;

            // Sets the command timeout for all the commands
            objectContext.CommandTimeout = 300;
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // TODO: Pull over too?
            //modelBuilder.Entity<RangePlan>().HasRequired(o => o.ItemMaster).WithMany().HasForeignKey(c => c.ItemID);
            //modelBuilder.Entity<DirectToStoreSku>().HasRequired(o => o.ItemMaster).WithMany().HasForeignKey(c => c.ItemID);
            //modelBuilder.Entity<RingFence>().HasRequired(o => o.ItemMaster).WithMany().HasForeignKey(c => c.ItemID);


            modelBuilder.Entity<RangePlan>().HasRequired(o => o.ItemMaster).WithMany().HasForeignKey(c => c.ItemID);
            modelBuilder.Entity<DirectToStoreSku>().HasRequired(o => o.ItemMaster).WithMany().HasForeignKey(c => c.ItemID);
            modelBuilder.Entity<RingFence>().HasRequired(o => o.ItemMaster).WithMany().HasForeignKey(c => c.ItemID);


            // NOTE: StoreLookup->StoreExtension relationship defined on constraint that IS ALL of principal's compositePK, so optional, unilateral (1to 1) relationship...
            modelBuilder.Entity<StoreLookup>().HasOptional(sl => sl.StoreExtension).WithRequired();

            // NOTE: RuleSelectedStore->StoreLookup relationship defined on constraint that IS NOT ALL of principal's composite PK (no ruleSetID), so multiplicity must be * to 1 (even though really 1to1)
            modelBuilder.Entity<RuleSelectedStore>().HasRequired(rss => rss.StoreLookup).WithMany().HasForeignKey(rss => new { rss.Division, rss.Store });

            // NOTE: RuleSet->RuleSelectedStore relationship is 1 to many, defined on specified FK field
            modelBuilder.Entity<RuleSet>().HasMany(rss => rss.Stores).WithRequired().HasForeignKey(rss => rss.RuleSetID);

            modelBuilder.Entity<VendorGroupLeadTime>().HasRequired(o => o.Zone).WithMany().HasForeignKey(c => c.ZoneID);

            modelBuilder.Entity<VendorGroupLeadTime>().HasRequired(o => o.Group).WithMany().HasForeignKey(c => c.VendorGroupID);

            modelBuilder.Entity<RDQ>().HasRequired(o => o.DistributionCenter).WithMany().HasForeignKey(c => c.DCID);
            //modelBuilder.Entity<RDQ>().HasOptional(x => x.QuantumRecordType).WithMany().HasForeignKey(c => c.RecordType);

            modelBuilder.Entity<RingFence>().HasRequired(o => o.RingFenceType).WithMany().HasForeignKey(c => c.Type);

            modelBuilder.Entity<SizeAllocation>().Property(x => x.RangeFromDB).HasColumnName("Range");

            modelBuilder.Entity<SkuAttributeHeader>().HasMany(x => x.SkuAttributeDetails).WithRequired(y => y.header).HasForeignKey(z => z.HeaderID);

            

            modelBuilder.Entity<ValidStoreLookup>().Map(m =>
            {
                m.MapInheritedProperties();
                m.ToTable("vValidStores");
            });

            //modelBuilder.Entity<ProductHierarchyOverrides>()
            //    .HasRequired<ProductOverrideTypes>(p => p.productOverrideType)
            //    .WithMany(s => s.productHierarchyOverrides);
        }

        public void HistoryRingFence(RingFence rf, string user, System.Data.EntityState state)
        {
            RingFenceHistory history = new RingFenceHistory();
            history.RingFenceID = rf.ID;
            history.Qty = rf.Qty;
            history.Division = rf.Division;
            history.EndDate = rf.EndDate;
            history.Size = rf.Size;
            history.Sku = rf.Sku;
            history.StartDate = rf.StartDate;
            history.Store = rf.Store;
            history.Action = state.ToString();
            history.CreateDate = DateTime.Now;
            history.CreatedBy = user;
            this.RingFenceHistory.Add(history);
        }

        public void HistoryRingFenceDetail(RingFenceDetail det, string user, System.Data.EntityState state)
        {
            RingFenceHistory history = new RingFenceHistory();
            history.RingFenceID = det.RingFenceID;
            history.DCID = det.DCID;
            history.PO = det.PO;
            history.Qty = det.Qty;
            history.Size = det.Size;
            history.Action = state.ToString() + " Det";
            history.CreateDate = DateTime.Now;
            history.CreatedBy = user;
            this.RingFenceHistory.Add(history);

        }

        private int GetQtyForRingFenceWithChange(RingFenceDetail det, RingFence rf, System.Data.EntityState state)
        {
            int qty = 0;

            var details = (from a in this.RingFenceDetails
                           where ((a.RingFenceID == det.RingFenceID) &&
                               (a.Size.Length == 3)
                               && ((a.Size != det.Size) || (a.PO != det.PO) || (a.DCID != det.DCID)) &&
                               a.ActiveInd == "1")
                           select a);

            //var details = (from a in this.RingFenceDetails
            //                where ((a.RingFenceID == det.RingFenceID) && 
            //                    (a.Size.Length == 3)
            //                    && ((a.Size != det.Size) || 
            //                    //basically saying the PO doesn't match (handles null and blank as the same)
            //                    (((det.PO == null)||(det.PO == "")) ? ((!(a.PO == null))&&(a.PO.Length > 0)) : ((a.PO != det.PO)||(a.PO == null) || (a.PO == ""))) 
            //                    || (a.DCID != det.DCID))
            //                    )
            //                select a);

            if (details.Count() > 0)
            {
                qty = details.Sum(a => a.Qty);
            }
            else
            {
                qty = 0;
            }

            var caselots = (from a in this.RingFenceDetails
                            where ((a.RingFenceID == det.RingFenceID) && (a.Size.Length == 5))
                                && ((a.Size != det.Size) || (a.PO != det.PO) || (a.DCID != det.DCID)) &&
                                a.ActiveInd == "1"
                            select a).ToList();
            foreach (RingFenceDetail cs in caselots)
            {
                try
                {
                    var query = ((from a in this.ItemPacks where (a.Name == cs.Size) select a.TotalQty));

                    qty += (det.Qty * query.First());
                }
                catch
                {
                    //don't have details, leave qty without caselots
                }
            }

            if (state != System.Data.EntityState.Deleted)
            {
                //add current detail
                if (det.Size.Length == 3)
                {
                    qty += det.Qty;
                }
                else
                {
                    try
                    {
                        var query = ((from a in this.ItemPacks where (a.Name == det.Size) select a.TotalQty));

                        qty += (det.Qty * query.First());
                    }
                    catch
                    {
                        //don't have details, leave qty without caselots
                    }
                }
            }
            if (qty == null)
            {
                qty = 0;
            }
            return qty;
        }

        public void PreCommitRingFenceDetail(RingFenceDetail det, string user, System.Data.EntityState state)
        {
            RingFence rf = (from a in this.RingFences where a.ID == det.RingFenceID select a).First();

            rf.Qty = GetQtyForRingFenceWithChange(det, rf, state);
            rf.CreateDate = DateTime.Now;
            rf.CreatedBy = user;

            if (this.Entry(rf).State != System.Data.EntityState.Modified)
            {
                this.Entry(rf).State = System.Data.EntityState.Modified;
            }

            //if ((rf.Type == 2)&&((det.PO == "")||(det.PO == null)))
            //{
            //    //update Ecomm Warehouse inventory qty
            //    var ecomm = (from a in this.EcommInventory where ((a.Store == rf.Store) && (a.ItemID == rf.ItemID) && (a.Size == det.Size)) select a);
            //    if (ecomm.Count() > 0)
            //    {
            //        if (state == System.Data.EntityState.Deleted)
            //        {
            //            ecomm.First().Qty = 0;
            //        }
            //        else
            //        {
            //            ecomm.First().Qty = det.Qty;
            //        }
            //    }
            //    else if (state == System.Data.EntityState.Added)
            //    {
            //        CreateEcommInventory(det, rf, user);
            //    }

            //}
        }

        //private void CreateEcommInventory(RingFenceDetail newDet, RingFence rf, string user)
        //{
        //    EcommInventory ecommInv;
        //    Boolean addInventory;
        //    if (rf.ItemID == 0)
        //    {
        //        rf.ItemID = (from a in ItemMasters where a.MerchantSku == rf.Sku select a.ID).First();
        //    }
        //    ecommInv = (from a in EcommInventory where ((a.Division == rf.Division) && (a.Store == rf.Store) && (a.ItemID == rf.ItemID) && (a.Size == newDet.Size)) select a).FirstOrDefault();
        //    addInventory = false;
        //    if (ecommInv == null)
        //    {
        //        addInventory = true;
        //        ecommInv = new EcommInventory();
        //        ecommInv.Division = rf.Division;
        //        ecommInv.Store = rf.Store;
        //        ecommInv.Size = newDet.Size;
        //        ecommInv.ItemID = rf.ItemID;
        //    }
        //    ecommInv.Qty = newDet.Qty;
        //    ecommInv.UpdateDate = DateTime.Now;
        //    ecommInv.UpdatedBy = user;
        //    if (addInventory)
        //    {
        //        EcommInventory.Add(ecommInv);
        //    }
        //}

        public void AuditRDQ(RDQ r, string user, System.Data.EntityState state)
        {
            if ((r.Status == "WEB PICK") || (r.Status == "FORCE PICK"))
            {
                AuditRDQ rdq = new AuditRDQ();
                rdq.RDQID = r.ID;
                rdq.Division = r.Division;
                rdq.Store = r.Store;
                rdq.DCID = r.DCID;
                rdq.PO = r.PO;
                rdq.ItemID = r.ItemID;
                rdq.Sku = r.Sku;
                rdq.Size = r.Size;
                rdq.Qty = r.Qty;
                rdq.TargetQty = r.TargetQty;
                rdq.UserRequestedQty = r.UserRequestedQty;
                rdq.NeedQty = r.NeedQty;
                rdq.ForecastQty = r.ForecastQty;
                rdq.Comment = state.ToString() + " Pick";
                rdq.PickedBy = user;
                rdq.PickDate = DateTime.Now;

                this.AuditRDQs.Add(rdq);
            }
        }

        public override int SaveChanges()
        {
            return SaveChanges("unknown");
        }

        public int SaveChanges(string user)
        {
            var audits = this.ChangeTracker.Entries().Where(p => ((p.State == System.Data.EntityState.Added || p.State == System.Data.EntityState.Deleted || p.State == System.Data.EntityState.Modified) && (p.Entity.GetType().Name.StartsWith("RingFence") || p.Entity.GetType().Name.StartsWith("RDQ"))));

            if (audits.Count() == 0)
            {
                return base.SaveChanges();
            }
            List<object> auditRecords = new List<object>();
            List<System.Data.EntityState> states = new List<System.Data.EntityState>();

            foreach (var ent in this.ChangeTracker.Entries().Where(p => p.State == System.Data.EntityState.Added || p.State == System.Data.EntityState.Deleted || p.State == System.Data.EntityState.Modified))
            {
                if (ent.Entity.GetType().Name.StartsWith("RingFenceHistory"))
                { }
                else if (ent.Entity.GetType().Name.StartsWith("RingFenceDetail"))
                {
                    PreCommitRingFenceDetail((RingFenceDetail)ent.Entity, user, ent.State);
                    auditRecords.Add(ent.Entity);
                    states.Add(ent.State);
                }
                else
                {
                    auditRecords.Add(ent.Entity);
                    states.Add(ent.State);
                }
            }
            if ((user == "unknown") && ((auditRecords.Count() > 0)))
            {
                throw new Exception("Must call SaveChanges with Userid for this recordtype");
            }

            int returnVal = base.SaveChanges();
            if (returnVal > 0)
            {
                int i = 0;
                foreach (object obj in auditRecords)
                {
                    if (obj.GetType().Name.StartsWith("RingFenceDetail"))
                    {
                        HistoryRingFenceDetail((RingFenceDetail)obj, user, states[i]);
                    }
                    else if (obj.GetType().Name.StartsWith("RingFence"))
                    {
                        HistoryRingFence((RingFence)obj, user, states[i]);
                    }
                    else if (obj.GetType().Name.StartsWith("RDQ"))
                    {
                        AuditRDQ((RDQ)obj, user, states[i]);
                    }

                    returnVal = base.SaveChanges();
                    i++;
                }
            }

            return returnVal;
        }

    }
}