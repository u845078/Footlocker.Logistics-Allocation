using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Common;
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
            AppConfig config = new AppConfig();

            SkuImage image = new SkuImage
            {
                Sku = sku,
                ImageUrl = string.Format("{0}{1}", config.ImageURL, sku)                
            };
                      
            Response.Redirect(image.ImageUrl);
            return View(image);
        }
    }
}
