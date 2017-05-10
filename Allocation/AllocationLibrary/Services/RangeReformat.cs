
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Services
{
    public class RangeReformat
    {
        //private const int _productIdent = 0;
        //private const int _locationType = 1;
        //private const int _locationID = 2;
        //private const int _onRange = 3;
        //private const int _offRange = 4;
        //private const int _todayUnitCost = 5;
        //private const int _todayUnitRetail = 6;
        //private const int _lifeCycle = 7;
        //private const int _min = 8;
        //private const int _max = 9;
        //private const int _initialDemand = 10;
        //private const int _range = 11;
        //private const int _launch = 12;
        //private const int _firstReceivableDt = 13;
        //private const int _minEndDate = 14;
        //private const int _attribute_15 = 15;
        //private const int _attribute_9 = 16;
        //private const int _learningTransitionCode = 17;
        //private const int _mld = 18;
        //private Boolean _InstanceOnOrderPlanning = false;
        //private const int _markdown = 20;

        public RangeReformat()
        { }

        public RangeReformat(int instance)
        {
            //_InstanceOnOrderPlanning = (new Allocation.Services.ConfigService()).IsTrue(instance, "ORDER_PLANNING");

        }
        public string Format(IDataReader dr, int instance)
        {
            return Format(dr, "Y", instance);
        }

        public string Format(IDataReader dr, string MLD, int instance)
        {
            string line = "";
            line = line + "\"" + Convert.ToString(dr["ProductIdent"]) + "\",";
            line = line + "\"" + Convert.ToString(dr["LocationTypeCode"]) + "\",";
            line = line + "\"" + Convert.ToString(dr["LocationID"]) + "\",";
            line = line + ",,";
            line = line + Convert.ToString(dr["Max"]) + ",";
            line = line + ",";
            line = line + Convert.ToString(dr["Min"]) + ",";
            line = line + ",,,,";
            line = line + "\"" + Convert.ToString(dr["OnRangeDt"]).Trim() + "\",";


			//string productIdent = Convert.ToString(dr["ProductIdent"]);
			//string markdown = "";
			//if (!productIdent.StartsWith("3440049-") && !productIdent.StartsWith("3306940-") &&
			//    !productIdent.StartsWith("3331343-") && !productIdent.StartsWith("3294248-"))
			//{
			//    markdown = "\"99991231\",";
			//}
			//else
			//{
			//    markdown = ",";
			//}
			//line = line + markdown;

			if (Convert.IsDBNull(dr["Markdown"]))
			{
				line = line + ",";
			}
			else
			{
				line = line + "\"" + Convert.ToString(dr["Markdown"]).Trim() + "\",";
			}

			line = line + "\"" + Convert.ToString(dr["OffRangeDt1"]).Trim() + "\",";            

            if (Convert.IsDBNull(dr["TodayUnitCost"]))
            {
                line = line + ",";
            }
            else
            {
                line = line + "\"" + Convert.ToString(dr["TodayUnitCost"]).Trim() + "\",";
            }

            if (Convert.IsDBNull(dr["TodayUnitRetail"]))
            {
                line = line + ",";
            }
            else
            {
                line = line + "\"" + Convert.ToString(dr["TodayUnitRetail"]).Trim() + "\",";
            }

            line = line + ",,,,";

            string initDemand = "";
            if (Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings[instance + "_DEFAULT_DEMAND"]) > 0)
            {
                if (!Convert.IsDBNull(dr["InitialDemand"]) &&
                    Convert.ToString(dr["InitialDemand"]) != "0" && 
                    Convert.ToString(dr["InitialDemand"]).Length > 0)
                {
                    initDemand = Convert.ToString(dr["InitialDemand"]);
                }
                else
                {
                    initDemand = System.Configuration.ConfigurationManager.AppSettings[instance + "_DEFAULT_DEMAND"];                
                }
            }
            else
            {
                initDemand = Convert.ToString(dr["InitialDemand"]);
            }

            if (initDemand == "0")
            {
                initDemand = "";
            }
            
            line = line + initDemand + "," + "\"0\",\"0\",\"0\",\"0\",\"0\",\"0\",\"0\",\"0\",,";

            if (Convert.IsDBNull(dr["Launch"]))
            {
                line = line + "\"REGULAR\",";
            }
            else
            {
                if (Convert.ToInt16(dr["Launch"]) == 1)
                {
                    line = line + "\"LAUNCH\",";
                }
                else
                {
                    line = line + "\"REGULAR\",";
                }
            }
  
            line = line + ",,,,";

            //if (_InstanceOnOrderPlanning)
            //{
            if (Convert.IsDBNull(dr["Attribute_15"]))
            {
                line = line + ",";
            }
            else
            {
                line = line + "\"" + dr["Attribute_15"] + "\",";
            }
            if (!Convert.IsDBNull(dr["MLD"]))
            {
                MLD = Convert.ToString(dr["MLD"]);
            }
            //}

            line = line + ",,,," + MLD + ",";

            if (Convert.IsDBNull(dr["FirstReceivableDt"]))
            {
                line = line + ",";
            }
            else
            {
                line = line + "\"" + Convert.ToString(dr["FirstReceivableDt"]).Trim() + "\",";
            }

            //if (Convert.IsDBNull(dr["LearningTransitionCode"]))
            //{
            //    line = line + ",";
            //}
            //else
            //{
            //    line = line + "\"" + Convert.ToString(dr["LearningTransitionCode"]).Trim() + "\",";
            //}

            //if (Convert.IsDBNull(dr["MinEndDate"]))
            //{
            //    line = line + ",";
            //}
            //else
            //{
            //    line = line + "\"" + Convert.ToString(dr["MinEndDate"]).Trim() + "\",";
            //}

            
            
            return line;
        }

    }
}
