using System;
using System.ComponentModel.DataAnnotations;

namespace Footlocker.Logistics.Allocation.Models
{
    [Table("PurgeArchiveTypes")]
    public class PurgeArchiveType
    {
        private string _archiveType;
        private string _archiveTypeDescription;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PurgeArchiveTypeID { get; set; }

        [Required(ErrorMessage = "Purge Type is required")]
        [StringLength(50, ErrorMessage = "Purge Type must not exceed maximum character limit")]
        [Display(Name = "Purge Type")]
        public string ArchiveType
        {
            get
            {
                return _archiveType;
            }
            set
            {
                _archiveType = value.Trim();
            }
        }

        [Required(ErrorMessage = "Purge Type Description is required")]
        [StringLength(1000, ErrorMessage = "Purge Type Description must not exceed maximum character limit")]
        [Display(Name = "Purge Type Description")]
        public string ArchiveTypeDescription
        {
            get
            {
                return _archiveTypeDescription;
            }
            set
            {
                _archiveTypeDescription = value.Trim();
            }
        }

        [Required(ErrorMessage = "Days Until Purge is required")]
        [Range(0, Int32.MaxValue, ErrorMessage = "Days Until Purge must be between 0 and {2}")]
        [Display(Name = "Days Until Purge")]
        public int DaysUntilPurge { get; set; }

        [Display(Name = "Active")]
        public bool ActiveInd { get; set; }
        public int InstanceID { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedUser { get; set; }
    }
}
