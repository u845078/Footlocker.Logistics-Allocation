using System;
using System.Collections.Generic;
using System.Data;
using Footlocker.Logistics.Allocation.Models;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Services
{

    public class PurgeArchiveDAO
    {
        private AllocationLibraryContext db;

        public PurgeArchiveDAO()
        {
            db = new AllocationLibraryContext();
        }

        #region PurgeArchiveType Calls

        /// <summary>
        /// Retrieve all PurgeArchiveTypes
        /// </summary>
        /// <returns>List of PurgeArchiveTypes</returns>
        public List<PurgeArchiveType> GetPurgeArchiveTypes()
        {
            List<PurgeArchiveType> purgeArchiveTypes = new List<PurgeArchiveType>();
            purgeArchiveTypes = (from a in db.PurgeArchiveTypes select a).ToList();
            return purgeArchiveTypes;
        }

        /// <summary>
        /// Retrieve one PurgeArchiveType dependent on the specified PurgeArchiveTypeID
        /// </summary>
        /// <param name="purgeArchiveTypeID">Specified PurgeArchiveTypeID</param>
        /// <returns>PurgeArchiveType</returns>
        public PurgeArchiveType GetPurgeArchiveTypeByID(int purgeArchiveTypeID)
        {
            PurgeArchiveType pat = (  from a in db.PurgeArchiveTypes
                                     where a.PurgeArchiveTypeID == purgeArchiveTypeID
                                    select a ).FirstOrDefault();
            return pat;
        }

        /// <summary>
        /// Retrieve PurgeArchiveTypes dependent on the specified instance
        /// </summary>
        /// <param name="instanceID">InstanceID</param>
        /// <returns>List of PurgeArchiveTypes</returns>
        public List<PurgeArchiveType> GetPurgeArchiveTypesByInstance(int instanceID)
        {
            List<PurgeArchiveType> purgeArchiveTypes = new List<PurgeArchiveType>();
            //default order should be archive type
            purgeArchiveTypes = (from a in db.PurgeArchiveTypes
                                 where a.InstanceID == instanceID
                                 select a).OrderBy(model => model.ArchiveType).ToList();
            return purgeArchiveTypes;
        }

        /// <summary>
        /// Update the specified PurgeArchiveType
        /// </summary>
        /// <param name="pat">PurgeArchiveType to be updated</param>
        /// <param name="user">User updating the PurgeArchiveType</param>
        public void Update(PurgeArchiveType pat, string user)
        {
            pat.LastModifiedUser = user;
            pat.LastModifiedDate = DateTime.Now;
            db.Entry(pat).State = EntityState.Modified;
            db.SaveChanges();
        }

        /// <summary>
        /// Create PurgeArchiveType.
        /// </summary>
        /// <param name="pat">PurgeArchiveType to create</param>
        /// <param name="user">User creating the PurgeArchiveType</param>
        public void Create(PurgeArchiveType pat, string user)
        {
            pat.LastModifiedUser = user;
            pat.LastModifiedDate = DateTime.Now;
            db.PurgeArchiveTypes.Add(pat);
            db.SaveChanges();
        }

        /// <summary>
        /// Create numerous PurgeArchiveTypes.
        /// </summary>
        /// <param name="pats">List of PurgeArchiveTypes to create.</param>
        /// <param name="user">User creating the PurgeArchiveType</param>
        public void Create(List<PurgeArchiveType> pats, string user)
        {
            foreach (PurgeArchiveType pat in pats)
            {
                pat.LastModifiedUser = user;
                pat.LastModifiedDate = DateTime.Now;
                db.PurgeArchiveTypes.Add(pat);
            }

            db.SaveChanges();
        }

        #endregion
    }
}
