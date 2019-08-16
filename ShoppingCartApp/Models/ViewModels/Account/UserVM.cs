using ShoppingCartApp.Models.Data.DTO;
using System.ComponentModel.DataAnnotations;

namespace ShoppingCartApp.Models.ViewModels.Account
{
    public class UserVM
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

        public UserVM()
        {
        }

        public UserVM(UserDTO dto)
        {
            Id = dto.Id;
            FirstName = dto.FirstName;
            LastName = dto.LastName;
            EmailAddress = dto.EmailAddress;
            UserName = dto.UserName;
            Password = dto.Password;
        }
    }
}