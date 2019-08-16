using ShoppingCartApp.Models.Data;
using ShoppingCartApp.Models.Data.DTO;
using ShoppingCartApp.Models.ViewModels.Account;
using ShoppingCartApp.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace ShoppingCartApp.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/Account/Login");
        }

        // /account/create-account
        [HttpGet]
        [ActionName("create-account")]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        [HttpPost]
        [ActionName("create-account")]
        public ActionResult CreateAccount(UserVM model)
        {
            if (!ModelState.IsValid)
                return View("CreateAccount", model);

            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Password do not match.");
                return View("CreateAccount", model);
            }

            using (CartDbContext db = new CartDbContext())
            {
                if (db.Users.Any(u => u.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", "Username " + model.UserName + " is already exist.");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }

                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    UserName = model.UserName,
                    Password = model.Password
                };

                db.Users.Add(userDTO);
                db.SaveChanges();

                int id = userDTO.Id;

                UserRolesDTO userRoleDTO = new UserRolesDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }

            TempData["SM"] = "Your acccount has been created and you can login";

            return Redirect("~/account/login");
        }

        // /account/login
        [HttpGet]
        public ActionResult Login()
        {
            string username = User.Identity.Name;
            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("user-profile");

            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool isValid = false;
            using (CartDbContext db = new CartDbContext())
            {
                if (db.Users.Any(u => u.UserName.Equals(model.Username) && u.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
            }
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            string username = User.Identity.Name;

            UserNavPartialVM model;

            using (CartDbContext db = new CartDbContext())
            {
                UserDTO dto = db.Users.FirstOrDefault(u => u.UserName == username);
                model = new UserNavPartialVM() { FirstName = dto.FirstName, LastName = dto.LastName };
            }
            return PartialView(model);
        }

        // /account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            UserProfileVM model;

            using (CartDbContext db = new CartDbContext())
            {
                var dto = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                model = new UserProfileVM(dto);
            }

            return View("UserProfile", model);
        }

        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            if (!ModelState.IsValid)
                return View("UserProfile", model);

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Password does not match.");
                    return View("UserProfile", model);
                }
            }

            using (CartDbContext db = new CartDbContext())
            {
                if (db.Users.Where(u => u.Id != model.Id).Any(u => u.UserName == User.Identity.Name))
                {
                    ModelState.AddModelError("", "Username " + model.UserName + " already exist.");
                    model.UserName = "";
                    return View("UserProfile", model);
                }


                var dto = db.Users.Find(model.Id);
                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.UserName = model.UserName;

                if (!string.IsNullOrWhiteSpace(model.Password))
                    dto.Password = model.Password;

                db.SaveChanges();
            }

            TempData["SM"] = "You have edited your profile";

            return Redirect("~/account/user-profile");
        }


        // GET
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            var orderForUsersList = new List<OrdersForUserVM>();

            using (CartDbContext db = new CartDbContext())
            {
                var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                var orders = db.Orders.Where(o => o.UserId == userId).ToArray().Select(u => new OrderVM(u)).ToList();

                foreach (var order in orders)
                {
                    Dictionary<string, int> productsAndQuantity = new Dictionary<string, int>();
                    decimal total = 0m;

                    var orderDetailsDTO = db.OrderDetails.Where(o => o.OrderId == order.OrderId).ToList();

                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        var product = db.Products.Where(p => p.Id == orderDetails.ProductId).FirstOrDefault();
                        decimal price = product.Price;
                        string productName = product.Name;

                        productsAndQuantity.Add(productName, orderDetails.Quantity);

                        total += orderDetails.Quantity * price;
                    }

                    orderForUsersList.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQuantity = productsAndQuantity,
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            return View(orderForUsersList);
        }
    }
}