using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class ImageController : Controller
    {
        //
        // GET: /Image/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ShowImage(string sku)
        {
            SkuImage image = new SkuImage();
            image.Sku = sku;

            image.ImageUrl = System.Configuration.ConfigurationManager.AppSettings["imageURL"] + sku;
            Response.Redirect(image.ImageUrl);
            return View(image);
        }
    }
}
