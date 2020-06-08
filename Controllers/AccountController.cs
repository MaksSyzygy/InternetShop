using Shop.Models.Data;
using Shop.Models.ViewModels.Account;
using Shop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Shop.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        //GET: account/create-account
        [ActionName("create-account")]//явно указываем имя пути
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        //POST: account/create-account
        [ActionName("create-account")]//явно указываем имя пути
        [HttpPost]
        public ActionResult CreateAccount(UserViewModel userViewModel)
        {
            //Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", userViewModel);
            }

            //Проверка пароля
            if (!userViewModel.Password.Equals(userViewModel.ConfirmPassword))
            {
                ModelState.AddModelError("passCreateAccError", "Пароли не совпадают");
                userViewModel.Password = "";
                userViewModel.ConfirmPassword = "";

                return View("CreateAccount", userViewModel);
            }

            using (DB db = new DB())
            {
                //Проверка логина на уникальность
                if(db.Users.Any(x => x.UserName == userViewModel.UserName))
                {
                    ModelState.AddModelError("loginUsed", $"Логин '{userViewModel.UserName}' занят");

                    return View("CreateAccount", userViewModel);
                }

                //Создаем экземпляр контекста данных UserDTO и записываем в него данные
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = userViewModel.FirstName,
                    LastName = userViewModel.LastName,
                    Email = userViewModel.Email,
                    UserName = userViewModel.UserName,
                    Password = userViewModel.Password
                };

                //Добавить все данные в экземпляр класса UserDTO
                db.Users.Add(userDTO);//которые через контекст БД записываем в саму БД

                //Сохраняем данные
                db.SaveChanges();

                //Добавляем роль пользователю
                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }

            //Записываем сообщение про успех операции
            TempData["OK_Message"] = "Аккаунт успешно создан!";

            //Переадресовываем пользователя
            return RedirectToAction("Login");
        }

        //GET: account/Login
        [HttpGet]
        public ActionResult Login()
        {
            //Проверка, что пользователь не авторизован
            string userName = User.Identity.Name;//Получаем текущего пользователя

            if (!string.IsNullOrEmpty(userName))//Если в переменную пришли данные, т.е. она не пустая
            {
                return RedirectToAction("user-profile");
            }

            //Возвращаем представление
            return View();
        }

        //POST: account/Login
        [HttpPost]
        public ActionResult Login(LoginUserViewModel loginUserVM)
        {
            //Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("emptyField", "Введите данные");
                return View(loginUserVM);
            }

            //Проверка пользователя на валидность
            bool isValid = false;

            using(DB db = new DB())
            {
                if(db.Users.Any(x => x.UserName == loginUserVM.UserName && x.Password == loginUserVM.Password))//Проверяем логин и пароль
                {
                    isValid = true;
                }

                if (!isValid)
                {
                    ModelState.AddModelError("errorLogin", "Неверный логин или пароль");
                    return View(loginUserVM);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(loginUserVM.UserName, loginUserVM.RememberMe);//Создает файл cookie на ПК, в параметрах принимает логин и статус оставаться в системе
                    return Redirect(FormsAuthentication.GetRedirectUrl(loginUserVM.UserName, loginUserVM.RememberMe));//Переадресация происходит через web.config в котором указан адрес куда
                }
            }
        }

        //GET: account/logout
        [Authorize]//давать доступ всем авторизованным пользователям
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            //Получаем имя пользователя
            string userName = User.Identity.Name;//получаем имя из текущего запроса

            //Объявляем модель
            UserNavPartialViewModel userNavPartialVM;

            using (DB db = new DB())
            {
                //Получаем пользователя
                UserDTO userDTO = db.Users.FirstOrDefault(x => x.UserName == userName);//из БД вытаскиваем данные и передаем в контекст

                //Наполняем модель данными из контекста (DTO)
                userNavPartialVM = new UserNavPartialViewModel()
                {
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName
                };
            }

            //Возвращаем частичное представление с моделью
            return PartialView("_UserNavPartial", userNavPartialVM);
        }

        //GET: account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            //Получаем имя пользователя
            string userName = User.Identity.Name;

            //Объявляем модель
            UserProfileViewModel userProfileViewModel;

            using (DB db = new DB())
            {
                //Получаем пользователя из БД в контекст
                UserDTO userDTO = db.Users.FirstOrDefault(x => x.UserName == userName);

                //Возвращаем в представление МОДЕЛИ полученные данные из контекста данных DTO
                userProfileViewModel = new UserProfileViewModel(userDTO);
            }

            //Возвращаем модель в представление
            return View("UserProfile", userProfileViewModel);//так делается из-за явно указанного ActionName, потому передавать нужно с именем метода
        }

        //POST: acoount/user-profile
        [HttpPost]//При POST методе в параметры метода передаем модель представления
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileViewModel userProfileVM)
        {
            bool userNameIsChanged = false;

            //Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                return View("UserProfile", userProfileVM);
            }

            //Проверка пароля, если пароль будет изменяться
            if (!string.IsNullOrWhiteSpace(userProfileVM.Password))
            {
                if (!userProfileVM.Password.Equals(userProfileVM.ConfirmPassword))
                {
                    ModelState.AddModelError("errorPass", "Пароли не совпадают");
                    userProfileVM.Password = "";
                    userProfileVM.ConfirmPassword = "";

                    return View("UserProfile", userProfileVM);
                }
            }

            using (DB db = new DB())
            {
                //Получаем имя пользователя
                string userName = User.Identity.Name;

                //Проверяем, сменил ли пользователь login
                if(userName != userProfileVM.UserName)
                {
                    userName = userProfileVM.UserName;
                    userNameIsChanged = true;
                }

                //Проверям имя на уникальность, если оно будет изменяться
                //Делаем проверку - ID из БД НЕ равен текущему ID, ведь текущий ID открыт в данный момент
                if(db.Users.Where(x => x.Id != userProfileVM.Id).Any(x => x.UserName == userName))
                {
                    ModelState.AddModelError("userNameMatch", $"Логин '{userProfileVM.UserName}' занят");
                    userProfileVM.UserName = "";

                    return View("UserProfile", userProfileVM);
                }

                //Изменяем модель контекста данных
                UserDTO userDTO = db.Users.Find(userProfileVM.Id);//Находим в БД юзера по ID и замещаем данные в контексте

                userDTO.FirstName = userProfileVM.FirstName;
                userDTO.LastName = userProfileVM.LastName;
                userDTO.Email = userProfileVM.Email;
                userDTO.UserName = userProfileVM.UserName;

                if (!string.IsNullOrWhiteSpace(userProfileVM.Password))
                {
                    userDTO.Password = userProfileVM.Password;
                }

                //Сохраняем изменения
                db.SaveChanges();
            }

            //Вывести сообщение про успех операции
            TempData["OK_Message"] = "Данные успешно обновлены";

            if (!userNameIsChanged)
            {
                //Возвращаем представление с моделью, если login не менялся
                return View("UserProfile", userProfileVM);
            }
            else
            {
                return RedirectToAction("LogOut");//если login поменялся - переадресация на страницу входа
            }
        }

        //GET: /account/orders
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            //Инициализируем модель OrdersForUserViewModel
            List<OrdersForUserViewModel> ordersForUser = new List<OrdersForUserViewModel>();

            using (DB db = new DB())
            {
                //Получаем id пользователя, где имя из БД равно имени пользователя сессии
                UserDTO userDTO = db.Users.FirstOrDefault(x => x.UserName == User.Identity.Name);

                //присваиваем id пользователя которое получили при выборке по имени пользователя
                int userId = userDTO.Id;

                //Инициализируем модель OrderViewModel, где id пользователя из БД таблицы заказов равен id имени пользвателя
                List<OrderViewModel> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderViewModel(x)).ToList();

                //Перебираем список товаров
                foreach (var order in orders)
                {
                    //Инициализируем словарь товаров
                    Dictionary<string, int> productAndQuantity = new Dictionary<string, int>();

                    //Объявляем переменную общей суммы
                    decimal total = 0m;

                    //Инициализируем модель OrderDetailsDTO, где номер заказа из БД равен номеру заказа из модели представления
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //Перебираем список OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        //Получаем товар
                        ProductDTO productDTO = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        //Получаем стоимость товара
                        decimal price = productDTO.Price;

                        //Получаем имя товара
                        string productName = productDTO.Name;

                        //Добавляем товар в словарь
                        productAndQuantity.Add(productName, orderDetails.Quantity);

                        //Получаем конечную стоимость товаров
                        total += orderDetails.Quantity * price;
                    }

                    //Добавляем полученные данные в модель OrderForUserViewModel
                    ordersForUser.Add(new OrdersForUserViewModel()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQuantity = productAndQuantity,
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            //Возвращаем представление с моделью
            return View(ordersForUser);
        }
    }
}