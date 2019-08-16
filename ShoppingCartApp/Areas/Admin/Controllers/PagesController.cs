using ShoppingCartApp.Models.Data;
using ShoppingCartApp.Models.Data.DTO;
using ShoppingCartApp.Models.ViewModels.Page;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ShoppingCartApp.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Declare list
            List<PageVM> pageList;

            using (CartDbContext db = new CartDbContext())
            {
                // initialize the list
                pageList = db.Pages.ToArray()
                    .OrderBy(p => p.Sorting).Select(p => new PageVM(p)).ToList();
            }

            // return view with list
            return View(pageList);
        }

        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPage(PageVM page)
        {
            // Check model state
            if (!ModelState.IsValid)
                return View(page);

            using (CartDbContext db = new CartDbContext())
            {
                // Declare slug
                string slug;

                // Initialize DTO
                var dto = new PageDTO();
                dto.Title = page.Title;

                // check for and slug if need be
                if (string.IsNullOrWhiteSpace(page.Slug))
                {
                    slug = page.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = page.Slug.Replace(" ", "-").ToLower();
                }

                // make sure title and slug are unique
                if (db.Pages.Any(p => p.Title == page.Title) || db.Pages.Any(p => p.Slug == page.Slug))
                {
                    ModelState.AddModelError("", "That title or slug already exist.");
                    return View(page);
                }

                //DTO the rest
                dto.Slug = slug;
                dto.Body = page.Body;
                dto.HasSidebar = page.HasSidebar;
                dto.Sorting = 100;

                // Save DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }
            // Set TempData message
            TempData["SM"] = "You have added a new page!";

            //Redirect 
            return RedirectToAction("AddPage");
        }

        [HttpGet]
        public ActionResult EditPage(int id)
        {
            // Declare
            PageVM model;

            using (CartDbContext db = new CartDbContext())
            {
                // Get the page
                var dto = db.Pages.Find(id);

                // Confirm page exist
                if (dto == null)
                    return Content("Page does not exist");

                // Initialize VM
                model = new PageVM(dto);
            }

            //return view with model
            return View(model);
        }

        [HttpPost]
        public ActionResult EditPage(PageVM page)
        {
            if (!ModelState.IsValid)
                return View(page);

            using (CartDbContext db = new CartDbContext())
            {
                int id = page.Id;
                string slug = "home";

                var dto = db.Pages.Find(id);
                dto.Title = page.Title;

                if (page.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(page.Slug))
                    {
                        slug = page.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = page.Slug.Replace(" ", "-").ToLower();
                    }
                }

                if (db.Pages.Where(p => p.Id != id).Any(p => p.Title == page.Title) ||
                    db.Pages.Where(p => p.Id != id).Any(p => p.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exist.");
                    return View(page);
                }

                dto.Slug = slug;
                dto.Body = page.Body;
                dto.HasSidebar = page.HasSidebar;

                db.SaveChanges();
            }

            TempData["SM"] = "You have edited the page.";

            return RedirectToAction("EditPage");
        }

        public ActionResult PageDetails(int id)
        {
            PageVM model;

            using (CartDbContext db = new CartDbContext())
            {
                var dto = db.Pages.Find(id);

                if (dto == null)
                {
                    return Content("This page does not exist.");
                }

                model = new PageVM(dto);
            }
            return View(model);
        }

        public ActionResult DeletePage(int id)
        {
            using (CartDbContext db = new CartDbContext())
            {
                var dto = db.Pages.Find(id);

                db.Pages.Remove(dto);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (CartDbContext db = new CartDbContext())
            {
                int count = 1;
                PageDTO dto;

                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }
        }

        [HttpGet]
        public ActionResult EditSidebar()
        {
            SidebarVM model;
            using (CartDbContext db = new CartDbContext())
            {
                SidebarDTO dto = db.Sidebar.Find(1);
                model = new SidebarVM(dto);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult EditSidebar(SidebarVM sidebar)
        {
            using (CartDbContext db = new CartDbContext())
            {
                var dto = db.Sidebar.Find(1);
                dto.Body = sidebar.Body;
                db.SaveChanges();
            }

            TempData["SM"] = "You have edited sidebar.";

            return RedirectToAction("EditSidebar");
        }
    }
}