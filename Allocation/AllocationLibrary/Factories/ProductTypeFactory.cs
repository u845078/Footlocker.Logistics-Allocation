
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class ProductTypeFactory      
    {
        public ProductType Create(DataRow dr)
        {
            ProductType _newObject = new ProductType()
            {
                Division = Convert.ToString(dr["RETL_OPER_DIV_CODE"]),
                Dept = Convert.ToString(dr["STK_DEPT_NUM"]),
                ProductTypeCode = Convert.ToString(dr["PRODUCT_TYP_CODE"]),
                ProductTypeName = Convert.ToString(dr["PRODUCT_TYP_NAME"]),
                ProductTypeID = Convert.ToInt32(dr["PRODUCT_TYP_ID"])
            };

            return _newObject;
        }
    }
}