using ShoppingCartApp.Models.Data;
using ShoppingCartApp.Models.Data.DTO;
using ShoppingCartApp.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace ShoppingCartApp.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty";
                return View();
            }

            decimal total = 0m;
            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            return View(cart);
        }

        public ActionResult CartPartial()
        {
            CartVM cartVM = new CartVM();

            int quantity = 0;
            decimal price = 0m;

            if (Session["cart"] != null)
            {
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    quantity += item.Quantity;
                    price += item.Quantity * item.Price;
                }
                cartVM.Quantity = quantity;
                cartVM.Price = price;
            }
            else
            {
                cartVM.Quantity = 0;
                cartVM.Price = 0m;
            }

            return PartialView(cartVM);
        }

        public ActionResult AddToCartPartial(int id)
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            CartVM cartVM = new CartVM();

            using (CartDbContext db = new CartDbContext())
            {
                var product = db.Products.Find(id);

                var productInCart = cart.FirstOrDefault(p => p.ProductId == id);
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                else
                {
                    productInCart.Quantity++;
                }
            }

            int quantity = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                quantity += item.Quantity;
                price = item.Quantity * item.Price;
            }

            cartVM.Quantity = quantity;
            cartVM.Price = price;

            Session["cart"] = cart;

            return PartialView(cartVM);
        }

        // Get
        public JsonResult IncrementProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (CartDbContext db = new CartDbContext())
            {
                CartVM cartVM = cart.FirstOrDefault(c => c.ProductId == productId);
                cartVM.Quantity++;

                var result = new
                {
                    qty = cartVM.Quantity,
                    price = cartVM.Price
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        // Get
        public JsonResult DecrementProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (CartDbContext db = new CartDbContext())
            {
                CartVM cartVM = cart.FirstOrDefault(c => c.ProductId == productId);

                if (cartVM.Quantity > 1)
                {
                    cartVM.Quantity--;
                }
                else
                {
                    cartVM.Quantity = 0;
                    cart.Remove(cartVM);
                }

                var result = new
                {
                    qty = cartVM.Quantity,
                    price = cartVM.Price
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Get
        public void RemoveProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (CartDbContext db = new CartDbContext())
            {
                CartVM cartVM = cart.FirstOrDefault(c => c.ProductId == productId);
                cart.Remove(cartVM);
            }
        }

        public ActionResult PaypalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            return PartialView(cart);
        }

        [HttpPost]
        public void PlaceOrder()
        {
            // Get cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Get username
            string username = User.Identity.Name;

            int orderId = 0;

            using (CartDbContext db = new CartDbContext())
            {
                // Init OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                // Get user id
                var uId = db.Users.FirstOrDefault(x => x.UserName == username);
                int userId = uId.Id;

                // Add to OrderDTO and save
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);

                db.SaveChanges();

                // Get inserted id
                orderId = orderDTO.OrderId;

                // Init OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                // Add to OrderDetailsDTO
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();
                }
            }

            // Email admin
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
             {
                 Credentials = new NetworkCredential("f51ec666a46180", "0af0373df85d78"),
                 EnableSsl = true
             };
            client.Send("admin@example.com", "admin@example.com", "New Order", "You have a new order and order number is " + orderId);

            // Reset session
            Session["cart"] = null;
        }
    }
}
