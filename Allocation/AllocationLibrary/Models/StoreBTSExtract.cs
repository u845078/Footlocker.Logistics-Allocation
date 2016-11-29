using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using Footlocker.Common.Utilities.File;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreBTSExtract : BiExtract
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
        public int Year { get; set; }

        [StringLayoutDelimited(5)]
        public string TYLY { get; set; }

        [StringLayoutDelimited(6)]
        public int Count { get; set; }

        [StringLayoutDelimited(7)]
        public string CreatedBy { get; set; }

        [StringLayoutDelimited(8, "yyyy-MM-dd h:mm:ss tt")]
        public DateTime CreateDate { get; set; }

        [StringLayoutDelimited(9)]
        public int TY { get; set; }

        /// <summary>
        /// Initializes a new instance of the StoreBTSExtract class.
        /// </summary>
        public StoreBTSExtract()
        {
            this.ID = 0;
            this.Division = String.Empty;
            this.Store = String.Empty;
            this.Name = String.Empty;
            this.Year = 0;
            this.TYLY = String.Empty;
            this.Count = 0;
            this.CreatedBy = String.Empty;
            this.CreateDate = DateTime.MinValue;
            this.TY = 0;
        }

        /// <summary>
        /// Initializes a new instance of the StoreBTSExtract class.
        /// </summary>
        /// <param name="id">The initial value for the identifier property.</param>
        /// <param name="division">The initial value for the division property.</param>
        /// <param name="store">The initial value for the store property.</param>
        /// <param name="name">The initial value for the name property.</param>
        /// <param name="year">The initial value for the year property.</param>
        /// <param name="tyLy">The initial value for the this year last year property.</param>
        /// <param name="count">The initial value for the count property.</param>
        /// <param name="createdBy">The initial value for the created by property.</param>
        /// <param name="createDate">The initial value for the create date property.</param>
        public StoreBTSExtract(int id, string division, string store, string name, int year, string tyLy, int count
                , string createdBy, DateTime createDate, int ty)
            : this()
        {
            this.ID = id;
            this.Division = division;
            this.Store = store;
            this.Name = name;
            this.Year = year;
            this.TYLY = tyLy;
            this.Count = count;
            this.CreatedBy = createdBy;
            this.CreateDate = createDate;
            this.TY = ty;
        }
    }
}
