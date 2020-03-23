using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Footlocker.Logistics.Allocation.Models;
using System.Data.Common;
using System.Data.Objects;
using Footlocker.Logistics.Allocation.Models.Services;
using System.Data.Entity.Infrastructure;

namespace Footlocker.Logistics.Allocation.DAO
{
    public class AllocationContext : DbContext
    {
        public DbSet<Rule> Rules { get; set; }
        public DbSet<RuleSet> RuleSets { get; set; }
        public DbSet<StoreLookup> StoreLookups { get; set; }
        public DbSet<RangePlanDetail> RangePlanDetails { get; set; }
        public DbSet<RangePlan> RangePlans { get; set; }
        public DbSet<SizeAllocation> SizeAllocations { get; set; }
        public DbSet<StorePlan> StorePlans { get; set; }
        public DbSet<Instance> Instances { get; set; }
        public DbSet<InstanceDivision> InstanceDivisions { get; set; }
        public DbSet<InstanceDistributionCenter> InstanceDistributionCenters { get; set; }
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
        public DbSet<VendorGroup> VendorGroups { get; set; }
        public DbSet<VendorGroupDetail> VendorGroupDetails { get; set; }
        public DbSet<StoreSeasonality> StoreSeasonality { get; set; }
        public DbSet<StoreSeasonalityDetail> StoreSeasonalityDetails { get; set; }
        public DbSet<StoreBTS> StoreBTS { get; set; }
        public DbSet<StoreBTSDetail> StoreBTSDetails { get; set; }
        public DbSet<StoreBTSControl> StoreBTSControls { get; set; }
        public DbSet<ExpeditePO> ExpeditePOs { get; set; }
        public DbSet<SkuAttributeHeader> SkuAttributeHeaders { get; set; }
        public DbSet<SkuAttributeDetail> SkuAttributeDetails { get; set; }
        public DbSet<DirectToStoreConstraint> DirectToStoreConstraints { get; set; }
        public DbSet<DirectToStoreSku> DirectToStoreSkus { get; set; }
        public DbSet<RuleSelectedStore> RuleSelectedStores { get; set; }
        public DbSet<ItemPack> ItemPacks { get; set; }
        public DbSet<ItemPackDetail> ItemPackDetails { get; set; }
        public DbSet<StoreLeadTime> StoreLeadTimes { get; set; }
        public DbSet<RouteDistributionCenter> RouteDistributionCenters { get; set; }
        public DbSet<StoreAttribute> StoreAttributes { get; set; }
        public DbSet<ConceptType> ConceptTypes { get; set; }
        public DbSet<ConceptTypeDivision> ConceptTypeDivisions { get; set; }
        public DbSet<CustomerType> CustomerTypes { get; set; }
        public DbSet<PriorityType> PriorityTypes { get; set; }
        public DbSet<StrategyType> StrategyTypes { get; set; }
        public DbSet<StoreExtension> StoreExtensions { get; set; }
        public DbSet<VendorGroupLeadTime> VendorGroupLeadTimes { get; set; }
        public DbSet<FOB> FOBs { get; set; }
        public DbSet<FOBDept> FOBDepts { get; set; }
        public DbSet<FOBPack> FOBPacks { get; set; }
        public DbSet<FOBPackOverride> FOBPackOverrides { get; set; }
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
        public DbSet<DTSConstraintModel> DTSConstraintModels { get; set; }
        public DbSet<ValidationSession> ValidationSessions { get; set; }
        public DbSet<InvalidTransaction> InvalidTransactions { get; set; }
        public DbSet<MandatoryCrossdock> MandatoryCrossdocks { get; set; }
        public DbSet<MandatoryCrossdockDefault> MandatoryCrossdockDefaults { get; set; }
        public DbSet<LegacyInventory> LegacyInventory { get; set; }
        public DbSet<LegacyFutureInventory> LegacyFutureInventory { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<ConfigParam> ConfigParams { get; set; }
        public DbSet<OrderPlanningRequest> OrderPlanningRequests { get; set; }
        public DbSet<ProductOverrideTypes> ProductOverrideTypes { get; set; }
        public DbSet<ProductHierarchyOverrides> ProductHierarchyOverrides { get; set; }
        public DbSet<Departments> Departments { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<BrandIDs> BrandIDs { get; set; }
        public DbSet<RingFenceStatusCodes> RingFenceStatusCodes { get; set; }
        public DbSet<PurchaseOrder> POs { get; set; }
        public DbSet<AllocationDriver> AllocationDrivers { get; set; }
        public DbSet<POStatus> POStatusCodes { get; set; }
        public DbSet<WarehouseAllocationType> WarehouseAllocationTypes { get; set; }
        public DbSet<MinihubStore> MinihubStores { get; set; }
        public DbSet<Vendors> Vendors { get; set; }
        public DbSet<TeamCodes> TeamCodes { get; set; }
        public DbSet<QuantumRecordTypeCode> QuantumRecordTypes { get; set; }
        public DbSet<RDQRejectReasonCode> RDQRejectReasons { get; set; }
        public DbSet<RDQRestriction> RDQRestrictions { get; set; }
        public DbSet<RDQType> RDQTypes { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<PreSaleSKU> PreSaleSKUs { get; set; }
        public DbSet<ReInitializeSKU> ReInitializeSKUs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<ItemMaster>().HasMany(o => o.RangePlans).WithOptional().HasForeignKey(c => c.ItemID);
            modelBuilder.Entity<RangePlan>().HasRequired(o => o.ItemMaster).WithMany().HasForeignKey(c => c.ItemID);
            //modelBuilder.Entity<RangePlan>().HasOptional(o => o.DirectToStoreSku);


            modelBuilder.Entity<DirectToStoreSku>().HasRequired(o => o.ItemMaster).WithMany().HasForeignKey(c => c.ItemID);
            modelBuilder.Entity<DTSConstraintModel>().HasRequired(o => o.ItemMaster).WithMany().HasForeignKey(c => c.ItemID);
            modelBuilder.Entity<RingFence>().HasRequired(o => o.ItemMaster).WithMany().HasForeignKey(c => c.ItemID);
            modelBuilder.Entity<MandatoryCrossdock>().HasRequired(o => o.ItemMaster).WithMany().HasForeignKey(c => c.ItemID);


            // NOTE: StoreLookup->StoreExtension relationship defined on constraint that IS ALL of principal's composite PK, so optional, unilateral (1to 1) relationship...
            modelBuilder.Entity<StoreLookup>().HasOptional(sl => sl.StoreExtension).WithRequired();

            // NOTE: RuleSelectedStore->StoreLookup relationship defined on constraint that IS NOT ALL of principal's composite PK (no ruleSetID), so multiplicity must be * to 1 (even though really 1to1)
            modelBuilder.Entity<RuleSelectedStore>().HasRequired(rss => rss.StoreLookup).WithMany().HasForeignKey(rss => new { rss.Division, rss.Store });

            // NOTE: RuleSet->RuleSelectedStore relationship is 1 to many, defined on specified FK field
            modelBuilder.Entity<RuleSet>().HasMany(rss => rss.Stores).WithRequired().HasForeignKey(rss => rss.RuleSetID);

            // NOTE: StoreSeasonalityDetail-> ValidStoreLookup relationship defined on constraint that IS NOT ALL of principal's composite PK (no groupID), so multiplicity must be * to 1 (even though really 1to1)
            modelBuilder.Entity<StoreSeasonalityDetail>().HasRequired(ssd => ssd.ValidStore).WithMany().HasForeignKey(ssd => new { ssd.Division, ssd.Store });

            // NOTE: InvalidTransaction -> StoreLookup relationship is 1 to many, defined on specified FK fields
            modelBuilder.Entity<InvalidTransaction>().HasRequired(it => it.Location).WithMany().HasForeignKey(it => new { it.LocationDiv, it.LocationStore });



            modelBuilder.Entity<VendorGroupLeadTime>().HasRequired(o => o.Zone).WithMany().HasForeignKey(c => c.ZoneID);

            modelBuilder.Entity<VendorGroupLeadTime>().HasRequired(o => o.Group).WithMany().HasForeignKey(c => c.VendorGroupID);

            modelBuilder.Entity<RDQ>().HasOptional(o => o.DistributionCenter).WithMany().HasForeignKey(c => c.DCID);


            // NOTE: CrossDockExclusion->StoreLookup relationship defined on constraint that IS ALL of principal's composite PK, so optional, unilateral (1to 1) relationship...
            modelBuilder.Entity<CrossDockExclusion>().HasOptional(rss => rss.StoreLookup).WithRequired();


            modelBuilder.Entity<RingFence>().HasRequired(o => o.RingFenceType).WithMany().HasForeignKey(c => c.Type);

            modelBuilder.Entity<SizeAllocation>().Property(x => x.RangeFromDB).HasColumnName("Range");

            //modelBuilder.Entity<RDQ>().HasMany(o => o.ItemPack).WithOptional().HasForeignKey(c => c.Name);

            // NOTE: InvalidTransaction -> StoreLookup relationship is 1 to many, defined on specified FK fields
            modelBuilder.Entity<Config>().HasRequired(it => it.ConfigParam).WithMany().HasForeignKey(it => it.ParamID);

        }

        public RingFenceHistory HistoryRingFence(RingFence rf, string user, System.Data.EntityState state)
        {
            RingFenceHistory history = new RingFenceHistory
            {
                RingFenceID = rf.ID,
                Qty = rf.Qty,
                Division = rf.Division,
                EndDate = rf.EndDate,
                Size = rf.Size,
                Sku = rf.Sku,
                StartDate = rf.StartDate,
                Store = rf.Store,
                Action = state.ToString(),
                CreateDate = DateTime.Now,
                CreatedBy = user
            };

            return history;            
        }

        public RingFenceHistory HistoryRingFenceDetail(RingFence header, RingFenceDetail det, string user, System.Data.EntityState state, string division = "")
        {
            RingFenceHistory history = new RingFenceHistory
            {
                RingFenceID = det.RingFenceID,
                DCID = det.DCID,
                PO = det.PO,
                Qty = det.Qty,
                Size = det.Size,
                Action = state.ToString() + " Det",
                CreateDate = DateTime.Now,
                CreatedBy = user
            };

            //RingFence rf = this.RingFences.Where(r => r.ID.Equals(det.RingFenceID)).FirstOrDefault();
            if (header != null)
            {
                history.Division = header.Division;
                history.Sku = header.Sku;
                history.Store = header.Store;
                history.StartDate = header.StartDate;
                history.EndDate = header.EndDate;
            }

            if (history.Division == null)
            {
                history.Division = division;
            }

            return history;            
        }

        private int GetQtyForRingFenceWithChange(RingFenceDetail det, RingFence rf, System.Data.EntityState state)
        {
            int qty = 0;
            var details = (from a in this.RingFenceDetails
                           where ((a.RingFenceID == det.RingFenceID) &&
                               (a.Size.Length == 3) &&
                               (a.ActiveInd == "1") &&
                               ((a.Size != det.Size) || (a.PO != det.PO) || (a.DCID != det.DCID)))
                           select a.Qty);
            if (details.Count() > 0)
            {
                qty = details.Sum();
            }
            else
            {
                qty = 0;
            }

            var caselots = (from a in this.RingFenceDetails
                            where ((a.RingFenceID == det.RingFenceID) &&
                                   (a.Size.Length == 5)) &&
                                   a.ActiveInd == "1" &&
                                  ((a.Size != det.Size) || (a.PO != det.PO) || (a.DCID != det.DCID))
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
            // 05/15/2018 this looks like it is unused. (it's commented out in SaveChanges)
            RingFence rf = (from a in this.RingFences where a.ID == det.RingFenceID select a).First();

            //rf.Qty = GetQtyForRingFenceWithChange(det, rf, state);
            rf.CreateDate = DateTime.Now;
            rf.CreatedBy = user;

            if (this.Entry(rf).State != System.Data.EntityState.Modified)
            {
                this.Entry(rf).State = System.Data.EntityState.Modified;
            }

            //if (rf.Type == 2)
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
                if (rdq.DCID == null)
                {
                    rdq.DCID = 0;
                }
                this.AuditRDQs.Add(rdq);
            }
        }

        public override int SaveChanges()
        {
            return SaveChanges("unknown");
        }

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public void CheckBatchRunning()
        {
            //Not Ready to implement this yet

            //List<AccessControl> Running = new List<AccessControl>();
            //AccessControl ac = new AccessControl();
            //ac.InstanceID = 1;
            //ac.Running = true;
            //Running.Add(ac);
            //List<InstanceDivision> all = this.InstanceDivisions.ToList();
            //List<InstanceDivision> blockedDivisions = (from a in all join b in Running on a.InstanceID equals b.InstanceID select a).ToList();
            //var audits = this.ChangeTracker.Entries().Where(p => ((p.State == System.Data.EntityState.Added || p.State == System.Data.EntityState.Deleted || p.State == System.Data.EntityState.Modified)));
            //string div;
            //int instance;
            //if (blockedDivisions.Count() > 0)
            //{
            //    //now, how do we figure out if it's the instance we're running?????

            //    foreach (System.Data.Entity.Infrastructure.DbEntityEntry e in audits)
            //    {
            //        var query = (from a in e.Entity.GetType().GetProperties() where a.Name == "Division" select e.Entity);
            //        if (query.Count() > 0)
            //        {
            //            div = e.Entity.GetType().GetProperty("Division").GetValue(e.Entity, null).ToString();
            //            var countQuery = (from a in blockedDivisions where a.Division == div select a);
            //            if (countQuery.Count() > 0)
            //            {
            //                throw new Exception("Batch is currently running, please try again later");
            //            }
            //        }

            //        var query2 = (from a in e.Entity.GetType().GetProperties() where a.Name == "InstanceID" select e.Entity);
            //        if (query2.Count() > 0)
            //        {
            //            instance = (int)e.Entity.GetType().GetProperty("InstanceID").GetValue(e.Entity, null);
            //            var countQuery = (from a in blockedDivisions where a.InstanceID == instance select a);
            //            if (countQuery.Count() > 0)
            //            {
            //                throw new Exception("Batch is currently running, please try again later");
            //            }
            //        }
            //    }
            //}

        }

        public void GenerateRingFenceHistoryRecords(List<DbEntityEntry> insertedRingFences, List<DbEntityEntry> modifiedRingFences)
        {
            string action = "Added Header";
            foreach (var rf in insertedRingFences)
            {
                RingFence ringFence = (RingFence)rf.Entity;
                RingFenceHistory rfh = new RingFenceHistory(ringFence.ID, ringFence.Division, ringFence.Store, ringFence.Sku, ringFence.Qty
                                                    , ringFence.StartDate, ringFence.EndDate, ringFence.CreateDate ?? DateTime.MinValue, ringFence.CreatedBy, action);
                this.RingFenceHistory.Add(rfh);
            }

            action = "Modified Header";
            foreach (var rf in modifiedRingFences)
            {
                RingFence ringFence = (RingFence)rf.Entity;
                RingFenceHistory rfh = new RingFenceHistory(ringFence.ID, ringFence.Division, ringFence.Store, ringFence.Sku, ringFence.Qty
                                                    , ringFence.StartDate, ringFence.EndDate, ringFence.CreateDate ?? DateTime.MinValue, ringFence.CreatedBy, action);
                this.RingFenceHistory.Add(rfh);
            }
        }

        public void GenerateRingFenceDetailHistoryRecords(List<DbEntityEntry> insertedRingFenceDetails, List<DbEntityEntry> modifiedRingFenceDetails, List<RingFence> existingRingFences)
        {
            string action = "Added Det";

            foreach (var ringFenceDetail in insertedRingFenceDetails)
            {
                RingFenceDetail rfd = (RingFenceDetail)ringFenceDetail.Entity;
                RingFence header = existingRingFences.Where(erf => erf.ID.Equals(rfd.RingFenceID)).FirstOrDefault();
                if (header != null)
                {
                    RingFenceHistory rfh = new RingFenceHistory(header.ID, header.Division, header.Store, header.Sku, rfd.Size, rfd.DCID, rfd.PO
                                            , rfd.Qty, header.StartDate, header.EndDate, rfd.LastModifiedDate, rfd.LastModifiedUser, action);
                    this.RingFenceHistory.Add(rfh);
                }
            }

            action = "Modified Det";
            foreach (var ringFenceDetail in modifiedRingFenceDetails)
            {
                RingFenceDetail rfd = (RingFenceDetail)ringFenceDetail.Entity;
                RingFence header = existingRingFences.Where(erf => erf.ID.Equals(rfd.RingFenceID)).FirstOrDefault();
                if (header != null)
                {
                    RingFenceHistory rfh = new RingFenceHistory(header.ID, header.Division, header.Store, header.Sku, rfd.Size, rfd.DCID, rfd.PO
                                            , rfd.Qty, header.StartDate, header.EndDate, rfd.LastModifiedDate, rfd.LastModifiedUser, action);
                    this.RingFenceHistory.Add(rfh);
                }
            }
        }

        public int SaveChangesBulk(string user)
        {
            // hold inserted and modified records in separate list because once we savechanges, the entitystate
            // will go from original state to "unchanged".
            // ringfencedetail records
            var insertedRingFenceDetails = this.ChangeTracker.Entries().Where(p => p.Entity is RingFenceDetail &&
                                                                                   p.State.Equals(System.Data.EntityState.Added)).ToList();

            var modifiedRingFenceDetails = this.ChangeTracker.Entries().Where(p => p.Entity is RingFenceDetail &&
                                                                                   p.State.Equals(System.Data.EntityState.Modified)).ToList();
            
            // ringfences records
            var insertedRingFences = this.ChangeTracker.Entries().Where(p => p.Entity is RingFence &&
                                                                             p.State.Equals(System.Data.EntityState.Added)).ToList();

            var modifiedRingFences = this.ChangeTracker.Entries().Where(p => p.Entity is RingFence &&
                                                                             p.State.Equals(System.Data.EntityState.Modified)).ToList();

            
            // First off, save data before auditing so we can reference primary key IDs between ringfences and ringfencedetails
            try
            {
                base.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                string exceptionMessage = string.Empty;
                foreach (var err in ex.EntityValidationErrors)
                {
                    foreach (var verr in err.ValidationErrors)
                    {
                        exceptionMessage += string.Format("{0}{1}: {2}.", err.ValidationErrors.Last().Equals(verr) ? "" : "  ", verr.PropertyName, verr.ErrorMessage);
                    }
                }

                throw new Exception(exceptionMessage);
            }
            

            // find ringfence records that locally correlate to ringfencedetail records in order to save db call time
            // ringfencedetails casted to concrete type so we can compare ids for existing ring fence headers
            List<RingFenceDetail> castedRFDs = insertedRingFenceDetails.Union(modifiedRingFenceDetails).Select(rfd => rfd.Entity).Cast<RingFenceDetail>().ToList();
            var existingRingFences = insertedRingFences.Union(modifiedRingFences).Where(rf => castedRFDs.Any(rfd => rfd.RingFenceID.Equals(((RingFence)rf.Entity).ID)))
                                               .Select(rf => (RingFence)rf.Entity)
                                               .ToList();

            // now add the non existing ones from the database
            var ringFenceIDs = castedRFDs.Where(rfd => !existingRingFences.Select(rf => rf.ID).Distinct().ToList().Contains(rfd.RingFenceID)).Select(rfd => rfd.RingFenceID).Distinct().ToList();
            existingRingFences.AddRange(this.RingFences.Where(rf => ringFenceIDs.Contains(rf.ID)).ToList());

            this.Configuration.AutoDetectChangesEnabled = false;

            // generate audit records for ringfencedetails
            this.GenerateRingFenceDetailHistoryRecords(insertedRingFenceDetails, modifiedRingFenceDetails, existingRingFences);

            // generate audit records for ringfences
            this.GenerateRingFenceHistoryRecords(insertedRingFences, modifiedRingFences);

            // save audit records
            return base.SaveChanges();
        }

        public int SaveChanges(string user)
        {
            CheckBatchRunning();

            var audits = this.ChangeTracker.Entries().Where(p => ((p.State == System.Data.EntityState.Added || p.State == System.Data.EntityState.Deleted || p.State == System.Data.EntityState.Modified) && (p.Entity.GetType().Name.StartsWith("RingFence") ||
                //p.Entity.GetType().Name.StartsWith("RDQ") || 
                p.Entity.GetType().Name.StartsWith("RangePlanDetail"))));

            if (audits.Count() == 0)
            {
                try
                {
                    return base.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException vex)
                {
                    string message = vex.EntityValidationErrors.First().ValidationErrors.First().PropertyName + ": " + vex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage;
                    message += ".";
                    throw vex;
                }
            }
            List<object> auditRecords = new List<object>();
            List<System.Data.EntityState> states = new List<System.Data.EntityState>();
            List<long> ringFenceUpdates = new List<long>();
            Boolean needUser = false;


            // ----------------------------------------------------------    prior logic     -------------------------------------------------------
            //                                                     select added || modified || deleted
            //
            // ----------------------------------------------------------      new logic     -------------------------------------------------------
            //                                                     select added || modified || deleted not rf/rfdetail
            //                                                                              select deleted rf/rfdetail

            var rfHeaders = this.ChangeTracker.Entries().Where(e => e.Entity is RingFence)
                .Select(e => e.Entity)
                .ToList();

            var deletedRFs = this.ChangeTracker.Entries().Where(e => e.Entity is RingFence && 
                                                                     e.State == System.Data.EntityState.Deleted)
                .Select(e => e.Entity)
                .ToList();

            foreach (var ent in this.ChangeTracker.Entries().Where(p => p.State == System.Data.EntityState.Deleted && 
                                                                        p.Entity is RingFenceDetail))
            {
                needUser = true;                
                // if the header for this detail is going to be deleted then we don't need to recompute the header qty, but knowing this seems hard

                long tmpl = ((RingFenceDetail)ent.Entity).RingFenceID;

                // if the header is not one of the deleted ones, then we'll have to recompute the header quantity
                if (deletedRFs.Where(drf => ((RingFence)drf).ID == tmpl).Count() == 0)
                    ringFenceUpdates.Add(((RingFenceDetail)ent.Entity).RingFenceID);

                // below is needed because the header might be deleted, or it might not be.  if it is, we need to store the division for the history                

                var rfHeader = rfHeaders.Where(e => ((RingFence)e).ID == tmpl).FirstOrDefault();

                RingFenceHistory tmp = new RingFenceHistory();

                tmp = HistoryRingFenceDetail((RingFence)rfHeader, (RingFenceDetail)ent.Entity, user, ent.State, ((RingFence)rfHeader).Division);

                auditRecords.Add(tmp);
                states.Add(ent.State);
            }

            foreach (DbEntityEntry ent in this.ChangeTracker.Entries().Where(p => p.State == System.Data.EntityState.Deleted && 
                                                                                  p.Entity is RingFence))
            {

                //added by mcg 2018/05/15 to separate audit logging out for detail / header (both were inside of 1 sp named setringfencehdrqty)
                needUser = true;
                //Write a history record to log the deletion of the header
                auditRecords.Add(HistoryRingFence((RingFence)ent.Entity, user, ent.State));
                states.Add(ent.State);
            }

            foreach (var ent in this.ChangeTracker.Entries().Where(p => p.State == System.Data.EntityState.Added || 
                                                                        p.State == System.Data.EntityState.Modified || 
                                                                        p.State == System.Data.EntityState.Deleted && 
                                                                        !p.Entity.GetType().Name.StartsWith("RingFence_") && 
                                                                        !p.Entity.GetType().Name.StartsWith("RingFenceDetail")))
            {
                if (ent.Entity.GetType().Name.StartsWith("RingFenceHistory"))
                { }
                else if (ent.Entity.GetType().Name.StartsWith("RingFence_"))
                {
                    //added by mcg 2018/05/15 to separate audit logging out for detail / header (both were inside of 1 sp named setringfencehdrqty)
                    needUser = true;
                    //Write a history record to log the deletion of the header                    
                    auditRecords.Add(HistoryRingFence((RingFence)ent.Entity, user, ent.State));
                    states.Add(ent.State);
                }
                else if (ent.Entity.GetType().Name.StartsWith("RingFenceDetail"))
                {
                    needUser = true;
                    ringFenceUpdates.Add(((RingFenceDetail)ent.Entity).RingFenceID);

                    RingFence rf = (RingFence)rfHeaders.Where(e => ((RingFence)e).ID == ((RingFenceDetail)ent.Entity).RingFenceID).FirstOrDefault();

                    auditRecords.Add(HistoryRingFenceDetail(rf, (RingFenceDetail)ent.Entity, user, ent.State));
                    states.Add(ent.State);
                }
                else
                {
                    if (!(ent.Entity.GetType().Name.StartsWith("RangePlanDetail")))
                    {
                        needUser = true;
                    }
                    auditRecords.Add(ent.Entity);
                    states.Add(ent.State);
                }
            }

            if ((user == "unknown") && needUser)
            {
                throw new Exception("Must call SaveChanges with Userid for this recordtype");
            }

            try
            {
                int returnVal = base.SaveChanges();
                if (returnVal > 0)
                {
                    int i = 0;
                    foreach (object obj in auditRecords)
                    {
                        if (obj.GetType().Name.StartsWith("RingFenceHistory"))
                        {
                            this.RingFenceHistory.Add((RingFenceHistory)obj);
                        }
                        //else if (obj.GetType().Name.StartsWith("RDQ"))
                        //{
                        //    AuditRDQ((RDQ)obj, user, states[i]);
                        //}
                        else if (obj.GetType().Name.StartsWith("RangePlanDetail"))
                        {
                            switch (states[i])
                            {
                                case System.Data.EntityState.Added:
                                    UpdateStoreCount(((RangePlanDetail)obj).ID, 1);
                                    break;
                                case System.Data.EntityState.Deleted:
                                    UpdateStoreCount(((RangePlanDetail)obj).ID, -1);
                                    break;
                                default:
                                    break;
                            }
                        }
                        
                        i++;
                    }

                    if (i > 0)
                        returnVal = base.SaveChanges();
                }

                if (ringFenceUpdates.Count > 0)
                {
                    //call stored procedure to update count and history
                    RingFenceDAO dao = new RingFenceDAO();
                    dao.SetRingFenceHeaderQtyAndHistory(ringFenceUpdates);
                }

                return returnVal;
            }
            catch
            {
                throw;
            }
        }

        private void UpdateStoreCount(Int64 planID, int CountOfStoresAdded)
        {
            RangePlan p = (from a in this.RangePlans where a.Id == planID select a).First();
            p.StoreCount += CountOfStoresAdded;
        }

        public IEnumerable<RangePlanDetail> GetRangePlanDetail(int planID, string div, string store)
        {
            return RangePlanDetails.Where(d => ((d.ID == planID) && (d.Store == store) && (d.Division == div))).ToList();
        }

        public IEnumerable<RangePlanDetail> GetRangePlanDetailsForPlan(Int64 planID)
        {
            return RangePlanDetails.Where(a => (a.ID == planID)).ToList();
        }

        public List<StoreLookupModel> GetStoreLookupsForPlan(Int64 planID, string divisions)
        {
            RangePlan p = (from a in RangePlans
                           where a.Id == planID
                           select a).First();

            var list = (from store in StoreLookups
                        join det in RangePlanDetails
                        on new { store.Division, store.Store } equals new { det.Division, det.Store }
                        where ((det.ID == planID) && (det.Division == p.Sku.Substring(0, 2)))
                        select new { Store = store, StoreExtension = store.StoreExtension });
            List<StoreLookupModel> results = new List<StoreLookupModel>();

            List<ConceptType> concepts = (from a in ConceptTypes
                                          select a).ToList();
            foreach (var s in list)
            {
                try
                {
                    s.Store.StoreExtension = s.StoreExtension;
                    s.Store.StoreExtension.ConceptType = (from a in concepts
                                                          where a.ID == s.Store.StoreExtension.ConceptTypeID
                                                          select a).FirstOrDefault();
                }
                catch
                { }

                results.Add(new StoreLookupModel(s.Store, planID, true));
            }
            return results;
        }

        public List<StoreLookupModel> GetStoreLookupsNotInPlan(Int64 planID, string divisions)
        {
            RangePlan p = (from a in RangePlans
                           where a.Id == planID
                           select a).First();

            //get list of div/store that are not in plan
            var templist = (from s in StoreLookups
                            where (s.Division == p.Sku.Substring(0, 2))
                            select new { s.Division, s.Store }).Except(from det in RangePlanDetails
                                                                       where det.ID == planID
                                                                       select new { det.Division, det.Store });
            //get StoreLookup objects for these div/stores
            var list = (from s in StoreLookups
                        join det in templist
                            on new { s.Division, s.Store } equals new { det.Division, det.Store }
                        select new { Store = s, StoreExtension = s.StoreExtension });

            List<StoreLookupModel> results = new List<StoreLookupModel>();
            List<ConceptType> concepts = (from a in ConceptTypes select a).ToList();
            foreach (var s in list)
            {
                try
                {
                    s.Store.StoreExtension = s.StoreExtension;
                    s.Store.StoreExtension.ConceptType = (from a in concepts where a.ID == s.Store.StoreExtension.ConceptTypeID select a).FirstOrDefault();
                }
                catch
                { }
                results.Add(new StoreLookupModel(s.Store, planID, false));
            }
            return results;
        }

        /// <summary>
        /// gets all the stores that are associated with a rangeplan
        /// </summary>
        public List<StoreLookupModel> GetStoresForPlan(Int64 planID)
        {
            List<StoreLookupModel> list = new List<StoreLookupModel>();

            IEnumerable<RangePlanDetail> rangeDetails = this.GetRangePlanDetailsForPlan(planID);
            StoreLookup lookup;
            StoreLookupModel model;
            foreach (RangePlanDetail det in rangeDetails)
            {
                try
                {
                    lookup = (from a in this.StoreLookups where ((a.Store == det.Store) && (a.Division == det.Division)) select a).First();
                    model = new StoreLookupModel(lookup);
                    model.CurrentPlan = planID;
                    list.Add(model);
                }
                catch
                {
                    //store must no longer exist, ignore it
                }
            }

            return list;
        }


        public List<Rule> GetRulesForPlan(Int64 planID, string ruleType)
        {
            List<Rule> RulesForPlan = (from r in Rules
                                       join rs in RuleSets
                                           on r.RuleSetID equals rs.RuleSetID
                                       where ((rs.PlanID == planID) && (rs.Type == ruleType))
                                       orderby r.Sort ascending
                                       select r).ToList();
            return RulesForPlan;
        }
        public List<Rule> GetRulesForRuleSet(Int64 RuleSetID, string ruleType)
        {
            List<Rule> RulesForPlan = (from r in Rules
                                       join rs in RuleSets
                                           on r.RuleSetID equals rs.RuleSetID
                                       where ((rs.RuleSetID == RuleSetID) && (rs.Type == ruleType))
                                       orderby r.Sort ascending
                                       select r).ToList();
            return RulesForPlan;
        }

        public void UpdateRangePlanDate(long planID, string userName)
        {
            RangePlan p = (from a in this.RangePlans where a.Id == planID select a).FirstOrDefault();
            p.UpdatedBy = userName;
            p.UpdateDate = DateTime.Now;
            SaveChanges(userName);
        }


    }
}
