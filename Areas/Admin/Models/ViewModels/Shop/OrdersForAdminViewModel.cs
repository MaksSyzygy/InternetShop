using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Shop.Areas.Admin.Models.ViewModels.Shop
{
    public class OrdersForAdminViewModel
    {
        [DisplayName("Номер заказа")]
        public int OrderNumber { get; set; }

        [DisplayName("Имя пользователя")]
        public string UserName { get; set; }

        [DisplayName("Общая сумма")]
        public decimal Total { get; set; }
        public Dictionary<string, int> ProductsAndQuantity { get; set; }//название товара/кол-во товара

        [DisplayName("Дата создания")]
        public DateTime CreatedAt { get; set; }
    }
}