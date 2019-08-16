using ShoppingCartApp.Models.Data.DTO;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ShoppingCartApp.Models.ViewModels.Page
{
    public class PageVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Title { get; set; }
        public string Slug { get; set; }

        [Required]
        [StringLength(int.MaxValue, MinimumLength = 3)]
        [AllowHtml]
        public string Body { get; set; }
        public int Sorting { get; set; }
        public bool HasSidebar { get; set; }

        public PageVM()
        {
        }

        public PageVM(PageDTO dto)
        {
            Id = dto.Id;
            Title = dto.Title;
            Slug = dto.Slug;
            Body = dto.Body;
            Sorting = dto.Sorting;
            HasSidebar = dto.HasSidebar;
        }
    }
}