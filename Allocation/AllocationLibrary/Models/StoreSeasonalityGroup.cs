using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreSeasonalityGroup : BiExtract
    {
        public override bool IsValid()
        {
            return true;
        }

        [StringLayoutDelimited(0)]
        public int ID { get; set; }

        [StringLayoutDelimited(1)]
        public string Division { get; set; }

        [Key]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Store number must be in the format #####")]
        [Required]
        [Column(Order = 1)]
        [StringLayoutDelimited(2)]
        public string Store { get; set; }

        [Required]
        [StringLayoutDelimited(3)]
        public string Name { get; set; }

        [StringLayoutDelimited(4)]
        public string CreatedBy { get; set; }

        [StringLayoutDelimited(5, "yyyy-MM-dd h:mm:ss tt")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Initializes a new instance of the StoreSeasonalityGroup class.
        /// </summary>
        public StoreSeasonalityGroup()
        {
            this.ID = 0;
            this.Division = String.Empty;
            this.Store = String.Empty;
            this.Name = String.Empty;
            this.CreatedBy = String.Empty;
            this.CreateDate = DateTime.MinValue;
        }

        /// <summary>
        /// Initializes a new instance of the StoreSeasonalityGroup class.
        /// </summary>
        /// <param name="id">The initial value for the identifier property.</param>
        /// <param name="division">The initial value for the division property.</param>
        /// <param name="store">The initial value for the store property.</param>
        /// <param name="name">The initial value for the name property.</param>
        /// <param name="createdBy">The initial value for the created by property.</param>
        /// <param name="createDate">The initial value for the create date property.</param>
        public StoreSeasonalityGroup(int id, string division, string store, string name, string createdBy
                , DateTime createDate)
            : this()
        {
            this.ID = id;
            this.Division = division;
            this.Store = store;
            this.Name = name;
            this.CreatedBy = createdBy;
            this.CreateDate = createDate;
        }
    }
}
