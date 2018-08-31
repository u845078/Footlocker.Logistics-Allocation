using System;
using System.Collections.Generic;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Models
{
    public class FOBSaveOptionsModel
    {
        #region Initializations

        public FOBSaveOptionsModel() { }

        public FOBSaveOptionsModel(int fobID, decimal newCost)
        {
            FOBID = fobID;
            NewCost = newCost;

            SaveChoices = new List<FOBSaveChoice>()
            {
                new FOBSaveChoice() { ID = (int)FOBSaveChoiceEntry.PrevCostPacks, Description = "Default Cost Packs Only" },
                new FOBSaveChoice() { ID = (int)FOBSaveChoiceEntry.All, Description = "All Packs" }
            };
        }

        #endregion

        #region Public Properties

        public ICollection<FOBSaveChoice> SaveChoices { get; set; }
        public int FOBID { get; set; }
        public decimal NewCost { get; set; }
        public int SelectedSaveChoiceID { get; set; }

        #endregion
    }

    public class FOBSaveChoice
    {
        public FOBSaveChoice() { }

        public string Description { get; set; }
        public int ID { get; set; }
    }

    public enum FOBSaveChoiceEntry
    {
        PrevCostPacks = 1,
        All = 2
    }
}