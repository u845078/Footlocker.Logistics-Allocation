
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class RangeFileItemFactory
    {
        private const int _productIdent=0;
        private const int _locationType = 1;
        private const int _locationID = 2;
        private const int _onRange = 3;
        private const int _offRange = 4;
        private const int _todayUnitCost = 5;
        private const int _todayUnitRetail = 6;
        private const int _lifeCycle = 7;
        private const int _min = 8;
        private const int _max = 9;
        private const int _initialDemand = 10;
        private const int _range = 11;
        private const int _launch = 12;
        private const int _firstReceivableDt = 13;
        private const int _minEndDate = 14;
        private const int _attribute_15 = 15;
        private const int _attribute_9 = 16;
        private const int _learningTransitionCode = 17;
        private const int _mld = 18;
        private Boolean _InstanceOnOrderPlanning = false;

        public RangeFileItem Create(IDataReader dr)
        {
            RangeFileItem _newObject = new RangeFileItem();
            _newObject.ProductIdent = Convert.ToString(dr[_productIdent]);
            _newObject.LocationTypeCode = Convert.ToString(dr[_locationType]);
            _newObject.LocationID = Convert.ToString(dr[_locationID]);
            try
            {
                _newObject.OnRangeDt = Convert.ToDateTime(dr[_onRange]).ToString("yyyyMMdd");
            }
            catch
            { 
                //legacy
                _newObject.OnRangeDt = Convert.ToString(dr[_onRange]);
            }
            try
            {
                _newObject.OffRangeDt = Convert.ToDateTime(dr[_offRange]).ToString("yyyyMMdd");
            }
            catch
            {
                //legacy
                _newObject.OffRangeDt = Convert.ToString(dr[_offRange]);
            }

            _newObject.TodayUnitCost = Convert.ToString(dr[_todayUnitCost]);
            _newObject.TodayUnitRetail = Convert.ToString(dr[_todayUnitRetail]);
            _newObject.NonsellableQty = Convert.ToString(dr[_min]);
            _newObject.MaxStockQty = Convert.ToString(dr[_max]);
            _newObject.InitWklyDemand = Convert.ToString(dr[_initialDemand]);
            _newObject.Ranged = (dr[_range] as int? == 1);

            //_newObject.MarkdownRetail = "0";
            _newObject.Attribute1 = "0";
            _newObject.Attribute2 = "0";
            _newObject.Attribute3 = "0";
            _newObject.Attribute4 = "0";
            _newObject.Attribute5 = "0";
            _newObject.Attribute6 = "0";
            _newObject.Attribute7 = "0";
            _newObject.Attribute8 = "0";

            if (!(Convert.IsDBNull(dr[_attribute_9])))
            {
                _newObject.Attribute9 = Convert.ToString(dr[_attribute_9]);
            }


            if (!(Convert.IsDBNull(dr[_launch])))
            {
                if (Convert.ToInt16(dr[_launch]) == 1)
                {
                    _newObject.Attribute10 = "LAUNCH";
                }
                else
                {
                    _newObject.Attribute10 = "REGULAR";
                }
            }
            else
            {
                _newObject.Attribute10 = "REGULAR";
            }

            if (!(Convert.IsDBNull(dr[_firstReceivableDt])))
            {
                _newObject.FirstReceivableDt = Convert.ToString(dr[_firstReceivableDt]);
            }

            if (!(Convert.IsDBNull(dr[_attribute_15])))
            {
                _newObject.Attribute15 = Convert.ToString(dr[_attribute_15]);
            }

            if (!(Convert.IsDBNull(dr[_minEndDate])))
            {
                _newObject.MinEndDate = Convert.ToString(dr[_minEndDate]);
            }

            if (!Convert.IsDBNull(dr[_learningTransitionCode]))
            {
                _newObject.LearningTransitionCode = Convert.ToString(dr[_learningTransitionCode]).Trim();
            }

            if (!Convert.IsDBNull(dr[_mld]))
            {
                _newObject.MldInd = Convert.ToString(dr[_mld]).Trim();
            }
            else
            {
                if (_newObject.Ranged)
                {
                    _newObject.MldInd = "Y";
                }
                else
                {
                    _newObject.MldInd = "N";
                }
            }
            return _newObject;
        }
    }
}
