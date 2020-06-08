using Shop.Models.Data;
using Shop.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Shop.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //Объявляем List<CartViewModel>
            var cart = Session["cart"] as List<CartViewModel> ?? new List<CartViewModel>();//если сессия пустая, то создается новый экземпляр листа

            //Проверка не пустая ли корзина
            if(cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Ваша корзина пустая";

                return View();
            }

            //Складываем сумму и записываем во ViewBag
            decimal total = 0m;

            foreach(var item in cart)
            {
                total += item.Total;
            }

            ViewBag.FinalTotal = total;

            //Возвращаем лист в представление
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //Объявляем модель CartViewModel
            CartViewModel cartViewModel = new CartViewModel();

            //Объявляем переменную количества
            int quantity = 0;

            //Объявляем переменную цены
            decimal price = 0m;

            //Проверяем сессию корзины - есть ли данные в корзине
            if(Session["cart"] != null)
            {
                //Получить общее количество товаров и цену
                var list = (List<CartViewModel>)Session["cart"];//явно указываем, что list должен быть списком CartViewModel

                foreach(var item in list)
                {
                    quantity += item.Quantity;//количество
                    price += item.Quantity * item.Price;//общая цена
                }

                cartViewModel.Quantity = quantity;//Добавляем в сессию данные, чтоб они не терялись при переходе с одной страницы товора на другую
                cartViewModel.Price = price;
            }
            else//если корзина пустая
            {
                //Или устанавливаем количество и цену в 0
                cartViewModel.Quantity = 0;
                cartViewModel.Price = 0m;
            }

            //Возвращаем частичное представление с моделью
            return PartialView("_CartPartial", cartViewModel);
        }

        public ActionResult AddToCartPartial(int id)
        {
            //Объявляем список параметризированный CartViewModel
            List<CartViewModel> cartList = Session["cart"] as List<CartViewModel> ?? new List<CartViewModel>();

            //Объявляем модель CartViewModel
            CartViewModel cartViewModel = new CartViewModel();

            using (DB db = new DB())
            {
                //Получаем товар
                ProductDTO productDTO = db.Products.Find(id);

                //Проверяем наличие товара в корзине
                var productInCart = cartList.FirstOrDefault(x => x.ProductId == id);

                //Если нет, то добавляем новый товар в корзину
                if(productInCart == null)
                {
                    cartList.Add(new CartViewModel()
                    {
                        ProductId = productDTO.Id,
                        ProductName = productDTO.Name,
                        Quantity = 1,
                        Price = productDTO.Price,
                        Image = productDTO.ImageName
                    });
                }

                //Если да, добавляем единицу товара в корзину
                else
                {
                    productInCart.Quantity++;
                }
            }

            //Получаем общее кол-во, цену и добавляем данные в модель
            int quantity = 0;
            decimal price = 0m;

            foreach(var item in cartList)
            {
                quantity += item.Quantity;
                price += item.Quantity * item.Price;
            }

            cartViewModel.Quantity = quantity;
            cartViewModel.Price = price;

            //Сохранить список в корзине в сессию
            Session["cart"] = cartList;

            //Вернуть частичное представление с моделью
            return PartialView("_AddToCartPartial", cartViewModel);
        }

        //GET: /cart/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {
            //Объявляем List Cart
            List<CartViewModel> cartList = Session["cart"] as List<CartViewModel>;

            using (DB db = new DB())
            {
                //Получаем модель CartViewModel из List
                CartViewModel cartViewModel = cartList.FirstOrDefault(x => x.ProductId == productId);

                //Добавляем кол-во
                cartViewModel.Quantity++;//при клике на "+" в корзине будет добавляться 1 ед. товара

                //Сохраняем необходимые данные
                var result = new { qty = cartViewModel.Quantity, price = cartViewModel.Price };//эти значения созданы в файле IncrementProductScript и отвечают за эти переменные

                //Возвращаем JSON-ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);//запросы JSON разрешены. По умолчанию запрещены
            }
        }

        //GET: /cart/DecrementProduct
        public JsonResult DecrementProduct(int productId)
        {
            //Объявляем List Cart
            List<CartViewModel> cartList = Session["cart"] as List<CartViewModel>;

            using (DB db = new DB())
            {
                //Получаем модель CartViewModel из List
                CartViewModel cartViewModel = cartList.FirstOrDefault(x => x.ProductId == productId);

                //Отнимаем кол-во
                if(cartViewModel.Quantity > 1)
                {
                    cartViewModel.Quantity--;
                }
                else
                {
                    cartViewModel.Quantity = 0;//задаем 0
                    cartList.Remove(cartViewModel);//удаляем из коллекции товар в корзине
                }

                //Сохраняем необходимые данные
                var result = new { qty = cartViewModel.Quantity, price = cartViewModel.Price };//эти значения созданы в файле IncrementProductScript и отвечают за эти переменные

                //Возвращаем JSON-ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);//запросы JSON разрешены. По умолчанию запрещены
            }
        }

        public void RemoveProduct(int productId)
        {
            //Объявляем List Cart
            List<CartViewModel> cartList = Session["cart"] as List<CartViewModel>;

            using(DB db = new DB())
            {
                //Получаем модель CartViewModel из List
                CartViewModel cartViewModel = cartList.FirstOrDefault(x => x.ProductId == productId);

                cartList.Remove(cartViewModel);
            }
        }

        public ActionResult PayPalPartial()
        {
            //Получить лист товаров из корзины
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            //Вернуть частичное представление вместе с листом товаров
            return PartialView("_PayPalPartial", cart);
        }

        //POST: /cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            //Получаем лист товаров из корзины
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            //Получаем имя пользователя
            string userName = User.Identity.Name;

            //Объявляем переменную для OrderId
            int orderId = 0;

            using (DB db = new DB())
            {
                //Объявляем модель OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                //Получаем ID пользователя
                var searchId = db.Users.FirstOrDefault(x => x.UserName == userName);
                int userId = searchId.Id;

                //Заполняем модель OrderDTO данными и сохраняем
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);
                db.SaveChanges();

                //Получаем OrderId
                orderId = orderDTO.OrderId;

                //Объявляем модель OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                //Наполняем данными модель OrderDetailsDTO
                foreach(var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);
                    db.SaveChanges();
                }
            }

            //Отправляем письмо о заказе на почту администратора
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("f0442c94d547a5", "39e31c687fb06e"),
                EnableSsl = true
            };
            client.Send("shop@example.com", "admin@example.com", "New Order", $"Новый заказ номер {orderId}");

            //Обнулить сессию(Условия для PayPal)
            Session["cart"] = null;
        }
    }
}