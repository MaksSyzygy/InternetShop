using Shop.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shop.Models.ViewModels.Shop
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public OrderViewModel(OrderDTO row)
        {
            OrderId = row.OrderId;
            UserId = row.UserId;
            CreatedAt = row.CreatedAt;
        }

        public OrderViewModel() { }
    }
}