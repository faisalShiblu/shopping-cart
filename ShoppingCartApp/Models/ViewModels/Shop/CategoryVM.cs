using ShoppingCartApp.Models.Data.DTO;

namespace ShoppingCartApp.Models.ViewModels.Shop
{
    public class CategoryVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public int Sorting { get; set; }

        public CategoryVM()
        {
        }
        public CategoryVM(CategoryDTO dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Slug = dto.Slug;
            Sorting = dto.Sorting;
        }
    }
}