using ShoppingCartApp.Models.Data;
using ShoppingCartApp.Models.Data.DTO;
using ShoppingCartApp.Models.ViewModels.Page;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ShoppingCartApp.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{page}
        public ActionResult Index(string page = "")
        {
            if (page == "")
                page = "home";

            PageVM pageVM;
            PageDTO dto;

            using (CartDbContext db = new CartDbContext())
            {
                if (!db.Pages.Any(p => p.Slug.Equals(page)))
                    return RedirectToAction("Index", new { page = "" });
            }

            using (CartDbContext db = new CartDbContext())
            {
                dto = db.Pages.Where(p => p.Slug == page).FirstOrDefault();
            }

            ViewBag.PageTitle = dto.Title;

            if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";
            }

            pageVM = new PageVM(dto);

            return View(pageVM);
        }

        public ActionResult PagesMenuPartial()
        {
            List<PageVM> pageVMList;

            using (CartDbContext db = new CartDbContext())
            {
                pageVMList = db.Pages.ToArray()
                                .OrderBy(p => p.Sorting)
                                    .Where(p => p.Slug != "home")
                                        .Select(p => new PageVM(p)).ToList();
            }

            return PartialView(pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            SidebarVM sidebarVM;

            using (CartDbContext db = new CartDbContext())
            {
                SidebarDTO dto = db.Sidebar.Find(1);
                sidebarVM = new SidebarVM(dto);
            }

            return PartialView(sidebarVM);
        }
    }
}