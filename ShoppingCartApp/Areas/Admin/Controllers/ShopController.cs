using PagedList;
using ShoppingCartApp.Areas.Admin.Models.ViewModels.Shop;
using ShoppingCartApp.Models.Data;
using ShoppingCartApp.Models.Data.DTO;
using ShoppingCartApp.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace ShoppingCartApp.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            List<CategoryVM> categoryList;

            using (CartDbContext db = new CartDbContext())
            {
                categoryList = db.Categories.ToArray()
                                 .OrderBy(o => o.Sorting)
                                 .Select(o => new CategoryVM(o)).ToList();
            }
            return View(categoryList);
        }

        [HttpPost]
        public string AddNewCategory(string catName)
        {
            string id;

            using (CartDbContext db = new CartDbContext())
            {
                if (db.Categories.Any(c => c.Name == catName))
                    return "titletaken";

                var dto = new CategoryDTO();
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                db.Categories.Add(dto);
                db.SaveChanges();

                id = dto.Id.ToString();
            }

            return id;
        }

        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (CartDbContext db = new CartDbContext())
            {
                int count = 1;
                CategoryDTO dto;

                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }
        }

        public ActionResult DeleteCategory(int id)
        {
            using (CartDbContext db = new CartDbContext())
            {
                var dto = db.Categories.Find(id);

                db.Categories.Remove(dto);
                db.SaveChanges();
            }
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (CartDbContext db = new CartDbContext())
            {
                if (db.Categories.Any(c => c.Name == newCatName))
                    return "Title Taken";

                var dto = db.Categories.Find(id);
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                db.SaveChanges();
            }

            return "Okay";
        }

        [HttpGet]
        public ActionResult AddProduct()
        {
            var product = new ProductVM();

            using (CartDbContext db = new CartDbContext())
            {
                product.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            return View(product);
        }

        [HttpPost]
        public ActionResult AddProduct(ProductVM productVM, HttpPostedFileBase file)
        {

            if (!ModelState.IsValid)
            {
                using (CartDbContext db = new CartDbContext())
                {
                    productVM.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(productVM);
                }
            }

            using (CartDbContext db = new CartDbContext())
            {
                if (db.Products.Any(p => p.Name == productVM.Name))
                {
                    productVM.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is already taken!");
                    return View(productVM);
                }
            }


            int id;

            using (CartDbContext db = new CartDbContext())
            {
                var productDTO = new ProductDTO();

                productDTO.Name = productVM.Name;
                productDTO.Slug = productVM.Name.Replace(" ", "-").ToLower();
                productDTO.Description = productVM.Description;
                productDTO.Price = productVM.Price;
                productDTO.CategoryId = productVM.CategoryId;

                var categoryDTO = db.Categories.FirstOrDefault(c => c.Id == productVM.CategoryId);
                productDTO.CategoryName = categoryDTO.Name;

                db.Products.Add(productDTO);
                db.SaveChanges();

                id = productDTO.Id;
            }

            TempData["SM"] = "You have added a product";

            #region Upload Image

            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);


            if (file != null && file.ContentLength > 0)
            {
                string extensions = file.ContentType.ToLower();
                if (extensions != "image/jpg" && extensions != "image/jpeg" && extensions != "image/pjpeg" &&
                   extensions != "image/gif" && extensions != "image/png" && extensions != "image/x-png")
                {
                    using (CartDbContext db = new CartDbContext())
                    {
                        productVM.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "This image was not uploaded or wrong image extension!");
                        return View(productVM);
                    }
                }

                string imageName = file.FileName;

                using (CartDbContext db = new CartDbContext())
                {
                    var dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                file.SaveAs(path);

                var img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }
            #endregion

            return RedirectToAction("AddProduct");
        }

        public ActionResult Products(int? page, int? categoryId)
        {
            List<ProductVM> listOfProductVM;

            var pageNumber = page ?? 1;

            using (CartDbContext db = new CartDbContext())
            {
                listOfProductVM = db.Products.ToArray()
                    .Where(p => categoryId == null || categoryId == 0 || p.CategoryId == categoryId)
                    .Select(p => new ProductVM(p))
                    .ToList();

                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                ViewBag.SelectedCategory = categoryId.ToString();
            }

            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);

            ViewBag.OnePageOfProducts = onePageOfProducts;

            return View(listOfProductVM);
        }

        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            ProductVM productVM;

            using (CartDbContext db = new CartDbContext())
            {
                ProductDTO dto = db.Products.Find(id);
                if (dto == null)
                    return Content("That product does not exist.");

                productVM = new ProductVM(dto);
                productVM.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                productVM.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                                    .Select(fn => Path.GetFileName(fn));
            }

            return View(productVM);
        }

        [HttpPost]
        public ActionResult EditProduct(ProductVM productVM, HttpPostedFileBase file)
        {
            int id = productVM.Id;

            using (CartDbContext db = new CartDbContext())
            {
                productVM.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            productVM.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                                    .Select(fn => Path.GetFileName(fn));

            if (!ModelState.IsValid)
                return View(productVM);

            using (CartDbContext db = new CartDbContext())
            {
                if (db.Products.Where(p => p.Id != id).Any(p => p.Name == productVM.Name))
                {
                    ModelState.AddModelError("", "That product name is taken.");
                    return View(productVM);
                }
            }

            using (CartDbContext db = new CartDbContext())
            {
                var dto = db.Products.Find(id);

                dto.Name = productVM.Name;
                dto.Slug = productVM.Name.Replace(" ", "-").ToLower();
                dto.Description = productVM.Description;
                dto.Price = productVM.Price;
                dto.CategoryId = productVM.CategoryId;
                dto.ImageName = productVM.ImageName;

                var categoryDTO = db.Categories.FirstOrDefault(c => c.Id == productVM.CategoryId);
                dto.CategoryName = categoryDTO.Name;

                db.SaveChanges();
            }

            TempData["SM"] = "You have edited the product.";


            #region Image Upload

            if (file != null && file.ContentLength > 0)
            {
                string extensions = file.ContentType.ToLower();
                if (extensions != "image/jpg" && extensions != "image/jpeg" && extensions != "image/pjpeg" &&
                   extensions != "image/gif" && extensions != "image/png" && extensions != "image/x-png")
                {
                    using (CartDbContext db = new CartDbContext())
                    {
                        ModelState.AddModelError("", "This image was not uploaded or wrong image extension!");
                        return View(productVM);
                    }
                }
            }

            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

            DirectoryInfo dir1 = new DirectoryInfo(pathString1);
            DirectoryInfo dir2 = new DirectoryInfo(pathString2);

            foreach (FileInfo file2 in dir1.GetFiles())
                file2.Delete();

            foreach (FileInfo file3 in dir2.GetFiles())
                file3.Delete();

            string imageName = file.FileName;

            using (CartDbContext db = new CartDbContext())
            {
                var dto = db.Products.Find(id);
                dto.ImageName = imageName;

                db.SaveChanges();
            }

            var path = string.Format("{0}\\{1}", pathString1, imageName);
            var path2 = string.Format("{0}\\{1}", pathString2, imageName);

            file.SaveAs(path);

            var img = new WebImage(file.InputStream);
            img.Resize(200, 200);
            img.Save(path2);

            #endregion

            return RedirectToAction("EditProduct");
        }

        public ActionResult DeleteProduct(int id)
        {
            using (CartDbContext db = new CartDbContext())
            {
                var dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }

            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            string pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

            return RedirectToAction("Products");
        }

        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            foreach (string fileName in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[fileName];

                if (file != null && file.ContentLength > 0)
                {
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    file.SaveAs(path);
                    var img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }
            }
        }

        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

        // Get
        public ActionResult Orders()
        {
            // Init list of OrdersForAdminVM
            var ordersForAdmin = new List<OrdersForAdminVM>();

            using (CartDbContext db = new CartDbContext())
            {
                // Init list of OrderVM
                var orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                // Loop through list of OrderVM
                foreach (var order in orders)
                {
                    // Init product dict
                    Dictionary<string, int> productsAndQuantity = new Dictionary<string, int>();

                    // Declare total
                    decimal total = 0m;

                    // Init list of OrderDetailsDTO
                    var orderDetailsList = db.OrderDetails.Where(X => X.OrderId == order.OrderId).ToList();

                    // Get username
                    var user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string username = user.UserName;

                    // Loop through list of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        // Get product
                        var product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // Get product price
                        decimal price = product.Price;

                        // Get product name
                        string productName = product.Name;

                        // Add to product dict
                        productsAndQuantity.Add(productName, orderDetails.Quantity);

                        // Get total
                        total += orderDetails.Quantity * price;
                    }

                    // Add to ordersForAdminVM list
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        UserName = username,
                        Total = total,
                        ProductsAndQuantity = productsAndQuantity,
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            // Return view with OrdersForAdminVM list
            return View(ordersForAdmin);
        }
    }
}