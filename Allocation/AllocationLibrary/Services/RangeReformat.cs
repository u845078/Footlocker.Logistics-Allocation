
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Services
{
    public class RangeReformat
    {
        public RangeReformat()
        { }

        public RangeReformat(int instance)
        {

        }
        public string Format(IDataReader dr, int instance)
        {
            return Format(dr, "Y", instance);
        }

        public string GetHeader()
        {
            string line = "";

            line += "Product Ident,Location Type Code,Location ID,,,Max,,Min,,,,,On Range Date,Markdown,Off Range Date,Today Unit Cost,Today Unit Retail,";
            line += ",,,,Initial Demand,Attribute 1,Attribute 2,,,,,,,Attribute 9,Launch,,,,,Attribute 15,,,,,MLD,First Receivable Date,Learning Transition Code,";
            line += "Min End Date";

            return line;
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

            line = line + initDemand + ",\"";

            line += Convert.ToString(dr["Attribute1"]) + "\",\"";
            line += Convert.ToString(dr["Attribute2"]) + "\",";

            line += "\"0\",\"0\",\"0\",\"0\",\"0\",\"0\",";

            if (!(Convert.IsDBNull(dr["attribute_9"])))
            {
                line = line + Convert.ToString(dr["attribute_9"]) + ",";
            }
            else
            {
                line = line + ",";
            }

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

            line = line + ",,,," + MLD + ",";

            if (Convert.IsDBNull(dr["FirstReceivableDt"]))
            {
                line = line + ",";
            }
            else
            {
                line = line + "\"" + Convert.ToString(dr["FirstReceivableDt"]).Trim() + "\",";
            }

            if (Convert.IsDBNull(dr["LearningTransitionCode"]))
            {
                line = line + ",";
            }
            else
            {
                line = line + "\"" + Convert.ToString(dr["LearningTransitionCode"]).Trim() + "\",";
            }

            if (Convert.IsDBNull(dr["MinEndDate"]))
            {
                line = line + ",";
            }
            else
            {
                line = line + "\"" + Convert.ToString(dr["MinEndDate"]).Trim() + "\",";
            }

            return line;
        }

    }
}
