using ShoppingCartApp.Models.Data.DTO;
using System;

namespace ShoppingCartApp.Models.ViewModels.Shop
{
    public class OrderVM
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public OrderVM()
        {
        }

        public OrderVM(OrderDTO dto)
        {
            OrderId = dto.OrderId;
            UserId = dto.UserId;
            CreatedAt = dto.CreatedAt;
        }
    }
}