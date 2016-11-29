// -----------------------------------------------------------------------
// <copyright file="ValidStoreLookup.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Footlocker.Logistics.Allocation.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.DataAnnotations;
    using System.Xml;

    /// <summary>
    /// Basically the same as StoreLookup, but we get it back from our vValidStores view (for open, nonexcluded stores)
    /// Entity Framework wouldn't let us inherit from StoreLookup, which was the preferred approach.  May want to revisit later.
    /// </summary>
    [Table("vValidStores")]
    public class ValidStoreLookup
    {
        private string _division;

        [Key]
        [Column(Order = 0)]
        public string Division
        {
            get { return _division; }
            set { _division = value; }
        }

        private string _store;

        [Key]
        [Column(Order = 1)]
        public string Store
        {
            get { return _store; }
            set { _store = value; }
        }

        private string _region;

        [Display(Name = "Region")]
        public string Region
        {
            get { return _region; }
            set { _region = value; }
        }


        private string _league;

        public string League
        {
            get { return _league; }
            set { _league = value; }
        }

        private string _state;

        public string State
        {
            get { return _state; }
            set { _state = value; }
        }

        private string _mall;

        public string Mall
        {
            get { return _mall; }
            set { _mall = value; }
        }

        private string _storeType;

        public string StoreType
        {
            get { return _storeType; }
            set { _storeType = value; }
        }

        private string _marketArea;

        public string MarketArea
        {
            get { return _marketArea; }
            set { _marketArea = value; }
        }

        private string _climate;

        public string Climate
        {
            get { return _climate; }
            set { _climate = value; }
        }

        private string _city;

        public string City
        {
            get { return _city; }
            set { _city = value; }
        }

        private string _dba;

        public string DBA
        {
            get { return _dba; }
            set { _dba = value; }
        }

        public string AdHoc1 { get; set; }
        public string AdHoc2 { get; set; }
        public string AdHoc3 { get; set; }
        public string AdHoc4 { get; set; }
        public string AdHoc5 { get; set; }
        public string AdHoc6 { get; set; }
        public string AdHoc7 { get; set; }
        public string AdHoc8 { get; set; }
        public string AdHoc9 { get; set; }
        public string AdHoc10 { get; set; }
        public string AdHoc11 { get; set; }
        public string AdHoc12 { get; set; }

        public string status { get; set; }


        [NotMapped]
        public string LookupValue {
            get
            {
                return this.Division + this.Store;
            }
            set
            {
                try
                {
                    this.Division = value.Substring(0, 2);
                    this.Store = value.Substring(2);
                }
                catch { }
            }
        }
    }
}
