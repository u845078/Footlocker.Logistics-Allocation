
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Services
{
    public class RangeReformat
    {
        private const int _productIdent = 0;
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
        private const int _markdown = 20;

        public RangeReformat()
        { }

        public RangeReformat(int instance)
        {
            _InstanceOnOrderPlanning = (new Allocation.Services.ConfigService()).IsTrue(instance, "ORDER_PLANNING");

        }
        public string Format(IDataReader dr, int instance)
        {
            return Format(dr, "Y", instance);
        }

        public string Format(IDataReader dr, string MLD, int instance)
        {
            string line = "";
            line = line + "\"" + Convert.ToString(dr[_productIdent]) + "\",";
            line = line + "\"" + Convert.ToString(dr[_locationType]) + "\",";
            line = line + "\"" + Convert.ToString(dr[_locationID]) + "\",";
            line = line + ",,";
            line = line + Convert.ToString(dr[_max]) + ",";
            line = line + ",";
            line = line + Convert.ToString(dr[_min]) + ",";
            line = line + ",,,,";
            line = line + "\"" + Convert.ToString(dr[_onRange]).Trim() + "\",";
            if (Convert.IsDBNull(dr[_markdown]))
            {
                line = line + ",";
            }
            else
            {
                line = line + "\"" + Convert.ToString(dr[_markdown]).Trim() + "\",";
            }

            line = line + "\"" + Convert.ToString(dr[_offRange]).Trim() + "\",";
            if (Convert.IsDBNull(dr[_todayUnitCost]))
            {
                line = line + ",";
            }
            else
            {
                line = line + "\"" + Convert.ToString(dr[_todayUnitCost]).Trim() + "\",";
            }

            if (Convert.IsDBNull(dr[_todayUnitRetail]))
            {
                line = line + ",";
            }
            else
            {
                line = line + "\"" + Convert.ToString(dr[_todayUnitRetail]).Trim() + "\",";
            }
            line = line + ",,,,";

            string initDemand = "";
            if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings[instance + "_DEFAULT_DEMAND"]) > 0)
            {
                if ((!(Convert.IsDBNull(dr[_initialDemand])))&&
                    (Convert.ToString(dr[_initialDemand]) != "0") && (Convert.ToString(dr[_initialDemand]).Length>0))
                {
                    initDemand = Convert.ToString(dr[_initialDemand]);
                }
                else
                {
                    initDemand = System.Configuration.ConfigurationManager.AppSettings[instance + "_DEFAULT_DEMAND"];                
                }
            }
            else
            {
                initDemand = Convert.ToString(dr[_initialDemand]);
            }
            if (initDemand == "0")
            {
                initDemand = "";
            }
            line = line + initDemand + ",";

            line = line + "\"0\",\"0\",\"0\",\"0\",\"0\",\"0\",\"0\",";
            line = line + "\"0\"";
            line = line + ",";
            if (!(Convert.IsDBNull(dr[_attribute_9])))
            {
                line = line + Convert.ToString(dr[_attribute_9]) + ",";
            }
            else
            {
                line = line + ",";
            }

            if (!(Convert.IsDBNull(dr[_launch])))
            {
                if (Convert.ToInt16(dr[_launch]) == 1)
                {
                    line = line + "\"LAUNCH\",";
                }
                else
                {
                    line = line + "\"REGULAR\",";
                }
            }
            else
            {
                line = line + "\"REGULAR\",";
            }

            line = line + ",,,,";
            if (_InstanceOnOrderPlanning)
            {
                line = line + "\"" + dr[_attribute_15] + "\""; //might be wrong!  this should match to attribute 15
                MLD = Convert.ToString(dr[_mld]);
            }
            line = line + ",,,,,";
            line = line + MLD + ",";
            if (Convert.IsDBNull(dr[_firstReceivableDt]))
            {
                line = line + ",";
            }
            else
            {
                line = line + "\"" + Convert.ToString(dr[_firstReceivableDt]).Trim() + "\",";
            }

            if (Convert.IsDBNull(dr[_learningTransitionCode]))
            {
                line = line + ",";
            }
            else
            {
                line = line + "\"" + Convert.ToString(dr[_learningTransitionCode]).Trim() + "\",";
            }

            if (Convert.IsDBNull(dr[_minEndDate]))
            {
                line = line + ",";
            }
            else
            {
                line = line + "\"" + Convert.ToString(dr[_minEndDate]).Trim() + "\",";
            }

            
            
            return line;
        }

    }
}
