using ShoppingCartApp.Models.Data.DTO;
using System.Web.Mvc;

namespace ShoppingCartApp.Models.ViewModels.Page
{
    public class SidebarVM
    {
        public int Id { get; set; }

        [AllowHtml]
        public string Body { get; set; }

        public SidebarVM()
        {
        }

        public SidebarVM(SidebarDTO dto)
        {
            Id = dto.Id;
            Body = dto.Body;
        }
    }
}