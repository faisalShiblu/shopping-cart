using ShoppingCartApp.Models.Data;
using ShoppingCartApp.Models.Data.DTO;
using ShoppingCartApp.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ShoppingCartApp.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            List<CategoryVM> categoryVMList;

            using (CartDbContext db = new CartDbContext())
            {
                categoryVMList = db.Categories.ToArray()
                                    .OrderBy(c => c.Sorting)
                                        .Select(c => new CategoryVM(c))
                                            .ToList();
            }

            return PartialView(categoryVMList);
        }

        //  /shop/category/name
        public ActionResult Category(string name)
        {
            List<ProductVM> productVMList;

            using (CartDbContext db = new CartDbContext())
            {
                // Get Catergory Id
                var categoryDTO = db.Categories.Where(c => c.Slug == name).FirstOrDefault();
                int categoryId = categoryDTO.Id;

                //Initialize the list
                productVMList = db.Products.ToArray()
                                  .Where(c => c.CategoryId == categoryId)
                                  .Select(c => new ProductVM(c)).ToList();


                // Get Category Name
                var productCategory = db.Products.Where(c => c.CategoryId == categoryId).FirstOrDefault();
                ViewBag.CategoryName = productCategory.CategoryName;
            }

            return View(productVMList);
        }

        //  /shop/product-details/name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            ProductVM productVM;
            ProductDTO productDTO;

            int id = 0;

            using (CartDbContext db = new CartDbContext())
            {
                if (!db.Products.Any(p => p.Slug.Equals(name)))
                    return RedirectToAction("Index", "Shop");

                productDTO = db.Products.Where(p => p.Slug == name).FirstOrDefault();

                id = productDTO.Id;

                productVM = new ProductVM(productDTO);
            }

            productVM.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                                   .Select(fn => Path.GetFileName(fn));

            return View("ProductDetails", productVM);
        }
    }
}