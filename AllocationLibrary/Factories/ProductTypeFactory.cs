
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
            ProductType _newObject = new ProductType();
            _newObject.Division = Convert.ToString(dr["RETL_OPER_DIV_CODE"]);
            _newObject.Dept = Convert.ToString(dr["STK_DEPT_NUM"]);
            _newObject.ProductTypeCode = Convert.ToString(dr["PRODUCT_TYP_CODE"]);
            _newObject.ProductTypeName = Convert.ToString(dr["PRODUCT_TYP_NAME"]);
            _newObject.ProductTypeID = Convert.ToInt32(dr["PRODUCT_TYP_ID"]);

            return _newObject;
        }
    }
}