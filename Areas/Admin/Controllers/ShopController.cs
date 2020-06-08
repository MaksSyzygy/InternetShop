using PagedList;
using Shop.Areas.Admin.Models.ViewModels.Shop;
using Shop.Models.Data;
using Shop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Shop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //Объявляем модель типа List
            List<CategoryViewModel> categoryVMList;

            using (DB dB = new DB())
            {
                //Инициализируем модель данными
                categoryVMList = dB.Categories
                                .ToArray()
                                .OrderBy(x => x.Sorting)
                                .Select(x => new CategoryViewModel(x))
                                .ToList();
            }

            //Возвращаем List в представление
            return View(categoryVMList);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //Объявляем переменную ID
            string id = null;

            using (DB dB = new DB())
            {
                //Проверка имени категории на уникальность
                if(dB.Categories.Any(x => x.Name == catName))
                {
                    return "titletaken";//во View Categories присутствует проверка на уникальность по этому ключу
                }

                //Инициализируем CategoryDTO
                CategoryDTO categoryDTO = new CategoryDTO();

                //Заполняем данными модель
                categoryDTO.Name = catName;
                categoryDTO.Description = catName.Replace(" ", "-").ToLower();
                categoryDTO.Sorting = 100;

                //Сохраняем изменения
                dB.Categories.Add(categoryDTO);
                dB.SaveChanges();

                //Получаем ID для возврата в представление
                id = categoryDTO.Id.ToString();
            }

            //Возвращаем ID в представление
            return id;
        }

        // GET: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (DB dB = new DB())
            {
                //Реализуем начальный счетчик
                int count = 1;

                //Инициализируем модель данных
                CategoryDTO categoryDTO;

                //Устанавливаем сортировку для каждой страницы
                foreach (var categoryId in id)
                {
                    categoryDTO = dB.Categories.Find(categoryId);
                    categoryDTO.Sorting = count;

                    dB.SaveChanges();

                    count++;
                }
            }
        }

        public ActionResult DeleteCategory(int id)
        {
            using (DB dB = new DB())
            {
                //Получение модели категории
                CategoryDTO categoryDTO = dB.Categories.Find(id);

                //Удаление категории
                dB.Categories.Remove(categoryDTO);

                //Сохранение изменений
                dB.SaveChanges();
            }

            //Оповещение про успех операции
            TempData["OK_Message"] = "Категория удалена";

            //Переадресация пользователя
            return RedirectToAction("Categories");
        }

        // GET: Admin/Shop/RenameCategory/id
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (DB dB = new DB())
            {
                //Проверка имени на уникальность
                if(dB.Categories.Any(x => x.Name == newCatName))
                {
                    return "titletaken";
                }

                //Получаем модель CategoryDTO
                CategoryDTO categoryDTO = dB.Categories.Find(id);

                //Редактируем модель CategoryDTO
                categoryDTO.Name = newCatName;
                categoryDTO.Description = newCatName.Replace(" ", "-").ToLower();

                //Сохраняем изменения
                dB.SaveChanges();
            }

            //Возвращаем
            return RedirectToAction("Categories").ToString();
        }

        //Метод добавления товаров
        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Объявляем модель
            ProductViewModel productViewModel = new ProductViewModel();

            //Добавляем список категорий из базы в модель
            using(DB db = new DB())
            {
                productViewModel.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            //Возвращаем модель в представление
            return View(productViewModel);
        }

        //Метод добавления товаров
        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductViewModel productViewModel, HttpPostedFileBase file)//передаем модель и класс для загрузки картинок
        {
            //Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                using(DB db = new DB())
                {
                    //Заполняем список. Если не сделать, он будет пустым и получим исключение
                    productViewModel.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");//поновой заполняем выпадающий список категорий

                    return View(productViewModel);
                }
            }

            //Проверка имени продукта на уникальность
            using(DB db = new DB())
            {
                if(db.Products.Any(x => x.Name == productViewModel.Name))
                {
                    productViewModel.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Имя продукта уже используется");

                    return View(productViewModel);
                }
            }

            //Объявляем переменную ProductId
            int id = 0;

            //Инициализируем и сохраняем в модель (в базу данных черех кеш DbSet) на основе ProductDTO
            using(DB db = new DB())
            {
                ProductDTO productDTO = new ProductDTO();

                productDTO.Name = productViewModel.Name;
                productDTO.Title = productViewModel.Name.Replace(" ", "-");
                productDTO.Description = productViewModel.Description;
                productDTO.Price = productViewModel.Price;
                productDTO.CategoryId = productViewModel.CategoryId;

                //делаем выборку из БД из категорий первую или по умолчанию на основе ID, который сравниваем ID в БД и ID из представления. Если ок, то значение присваиваем
                CategoryDTO categoryDTO = db.Categories.FirstOrDefault(x => x.Id == productViewModel.CategoryId);

                productDTO.CategoryName = categoryDTO.Name;//и после этого присваиваем имя категории по её ID

                db.Products.Add(productDTO);//добавляем все измененный/полученные значения в БД

                db.SaveChanges();//сохраняем

                id = productDTO.Id;//получаем id нашего добавленного товара
            }

            //Сообщение про успех операции tempData
            TempData["OK_Message"] = "Продукт успешно добавлен";

            #region UploadImage

            //Создаем ссылки на директории
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));//метод создаст папку для загружаемых картинок

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");//создаем подпапки в созданной директории
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            //Проверяем наличие директорий (если нет, то создаем)
            if (!Directory.Exists(pathString1))//если директория пустая
            {
                Directory.CreateDirectory(pathString1);//то создаем
            }

            if (!Directory.Exists(pathString2))//если директория пустая
            {
                Directory.CreateDirectory(pathString2);//то создаем
            }

            if (!Directory.Exists(pathString3))//если директория пустая
            {
                Directory.CreateDirectory(pathString3);//то создаем
            }

            if (!Directory.Exists(pathString4))//если директория пустая
            {
                Directory.CreateDirectory(pathString4);//то создаем
            }

            if (!Directory.Exists(pathString5))//если директория пустая
            {
                Directory.CreateDirectory(pathString5);//то создаем
            }

            //Создаем список доступных для загрузки файлов
            string[] extensions = { "image/jpg", "image/jpeg", "image/gif", "image/png" };

            //Проверка был ли файл загружен
            if (file != null && file.ContentLength > 0)
            {
                //Получаем расширение файла
                string extension = file.ContentType.ToLower();
                int counter = 0;

                //Проверяем расширение файла
                foreach (var item in extensions)
                {
                    if (extension != item)
                    {
                        counter++;

                        if(counter == extensions.Length)
                        {
                            using (DB db = new DB())
                            {
                                productViewModel.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                                ModelState.AddModelError("", "Изображение не было загружено - неправильный формат изображения");

                                return View(productViewModel);
                            }
                        }
                    }
                }

                //Объявляем переменную с именем картинки
                string imageName = file.FileName;

                //Сохраняем имя картинки в модель ProductDTO
                using (DB db = new DB())
                {
                    ProductDTO productDTO = db.Products.Find(id);
                    productDTO.ImageName = imageName;

                    db.SaveChanges();
                }

                //Назначаем пути к оригинальному и уменьшенному изображению
                var path = string.Format($"{pathString2}\\{imageName}");//оригинальное изображение
                var path2 = string.Format($"{pathString3}\\{imageName}");//уменьшенное

                //Сохраняем оригинальное изображение
                file.SaveAs(path);

                //Создаем и сохраняем уменьшенное изображение
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            //Переадресовать пользователя
            return RedirectToAction("AddProduct");
        }

        //Метод списка товаров
        //Get: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)//для этого метода подключаем библиотеку PagedList.mvc. int? - значит, что переменная может принимать параметр null
        {
            //Объявляем модель ProductViewModel типа List
            List<ProductViewModel> productListVM;

            //Устанавливаем номер страницы
            var pageNumber = page ?? 1;//если вернется null - значение будет 1, в другом случае значение будет то, которое вернулось

            using (DB db = new DB())
            {
                //Инициализируем List и наполняем его данными
                productListVM = db.Products.ToArray()
                                .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                .Select(x => new ProductViewModel(x)).ToList();

                //Заполняем категории данными
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Устанавливаем выбранную категорию
                ViewBag.SelectedCat = catId.ToString();
            }

            //Устанавливаем постраничную навигацию
            var onePageOfProducts = productListVM.ToPagedList(pageNumber, 3);//первым аргументом передаем номер страницы, вторым - количество товара на странице
            ViewBag.onePageOfProducts = onePageOfProducts;

            //Возвращаем представление с данными

            return View(productListVM);
        }

        //Метод редактирования товаров
        //Get: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //Объявляем модель ProductViewModel
            ProductViewModel productViewModel;

            using (DB db = new DB())
            {
                //Получаем продукт
                ProductDTO productDTO = db.Products.Find(id);

                //Проверяем доступность товара
                if(productDTO == null)
                {
                    return Content("Товар не доступен");
                }

                //Инициализируем модель данными
                productViewModel = new ProductViewModel(productDTO);

                //Создаем список категорий
                productViewModel.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Получаем картинки товаров из галерии
                productViewModel.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                                .Select(fileName => Path.GetFileName(fileName));
            }

            //Возвращаем модель в представление
            return View(productViewModel);
        }

        //Метод редактирования товаров
        //Post: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductViewModel productViewModel, HttpPostedFileBase file)
        {
            //Получаем ID продукта
            int id = productViewModel.Id;

            //Заполняем список категориями
            using(DB db = new DB())
            {
                productViewModel.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            //И изображениями
            productViewModel.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                             .Select(fileName => Path.GetFileName(fileName));

            //Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                return View(productViewModel);
            }

            //Проверка имени продукта на уникальность
            using(DB db = new DB())
            {
                if(db.Products.Where(x => x.Id != id).Any(x => x.Name == productViewModel.Name))
                {
                    ModelState.AddModelError("", "Имя этого продукта занято");

                    return View(productViewModel);
                }
            }

            //Обновляем данные
            using(DB db = new DB())
            {
                ProductDTO productDTO = db.Products.Find(id);//Находим наш текущий продукт и заносим его в dto

                productDTO.Name = productViewModel.Name;
                productDTO.Title = productViewModel.Name.Replace(" ", "-").ToLower();
                productDTO.Description = productViewModel.Description;
                productDTO.Price = productViewModel.Price;
                productDTO.CategoryId = productViewModel.CategoryId;
                productDTO.ImageName = productViewModel.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == productViewModel.CategoryId);

                productDTO.CategoryName = catDTO.Name;

                db.SaveChanges();
            }

            //Сообщение про успех операции
            TempData["OK_Message"] = "Товар обновлен";

            //Логика обработки изображений
            #region Image Upload

            //Проверка загрузки файла
            if (file != null && file.ContentLength > 0)
            {
                //Получаем расширение файла
                string extension = file.ContentType.ToLower();

                //Создаем список доступных для загрузки файлов
                string[] extensions = { "image/jpg", "image/jpeg", "image/gif", "image/png" };
                int counter = 0;

                //Проверяем расширение файла
                foreach (var item in extensions)
                {
                    if (extension != item)
                    {
                        counter++;

                        if (counter == extensions.Length)
                        {
                            using (DB db = new DB())
                            {
                                ModelState.AddModelError("", "Изображение не было загружено - неправильный формат изображения");

                                return View(productViewModel);
                            }
                        }
                    }
                }

                //Устанавливаем пути для загрузки
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));//метод создаст папку для загружаемых картинок

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //Удаляем существующие файлы и директории
                DirectoryInfo directoryInfo1 = new DirectoryInfo(pathString1);
                DirectoryInfo directoryInfo2 = new DirectoryInfo(pathString2);

                foreach(var item in directoryInfo1.GetFiles())
                {
                    item.Delete();
                }

                foreach (var item in directoryInfo2.GetFiles())
                {
                    item.Delete();
                }

                //Сохраняем имя изображения
                string imageName = file.FileName;

                using(DB db = new DB())
                {
                    ProductDTO productDTO = db.Products.Find(id);//находим продукт и заносим его в модель
                    productDTO.ImageName = imageName;

                    db.SaveChanges();
                }

                //Сохраняем оригинал и превью версии
                var path = string.Format($"{pathString1}\\{imageName}");//оригинальное изображение
                var path2 = string.Format($"{pathString2}\\{imageName}");//уменьшенное

                //Сохраняем оригинальное изображение
                file.SaveAs(path);

                //Создаем и сохраняем уменьшенное изображение
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            //Переадресация пользователя
            return RedirectToAction("EditProduct");
        }

        //Удаление товара
        //POST: Admin/Shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            //Удаляем товар из БД
            using(DB db = new DB())
            {
                ProductDTO productDTO = db.Products.Find(id);

                db.Products.Remove(productDTO);

                db.SaveChanges();
            }

            //Удаляем директории с картинками товара
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));//метод создаст папку для загружаемых картинок

            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
            {
                Directory.Delete(pathString, true);//указываем путь удаления директории, а true - удаление всех подкаталогов
            }

            //Переадресовываем пользователя
            return RedirectToAction("Products");
        }

        //Добавление картинок товара в галерею
        //POST: Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            //Перебираем все полученные файлы
            foreach (string fileName in Request.Files)//Request - перебирает все полученные файлы из текущего HTTP запроса
            {
                //Инициализируем полученные файлы
                HttpPostedFileBase file = Request.Files[fileName];//перебирает все файлы по имени и добавляет

                //Проверка на NULL
                if (file != null && file.ContentLength > 0)
                {
                    //Назначаем пути к директориям
                    var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                    var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    //Назначаем пути изображений
                    var pathOriginalGallery = string.Format($"{pathString1}\\{file.FileName}");//file.FileName - сразу получаем имя картинки
                    var pathThumbs = string.Format($"{pathString2}\\{file.FileName}");

                    //Сохраняем оригинальные и уменьшенные копии картинок
                    file.SaveAs(pathOriginalGallery);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(pathThumbs);
                }
            }
        }

        //Удаление картинок товара из галереи
        //POST: Admin/Shop/DeleteImage/id/imageName
        public void DeleteImage(int id, string imageName)
        {
            //Реализуем путь
            string pathImg = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string pathThumbs = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            //Удаляем оригинальные картинки и уменьшенные копии
            if (System.IO.File.Exists(pathImg))//проверяем доступность и наличие файлов
            {
                System.IO.File.Delete(pathImg);//если есть - удаляем
            }

            if (System.IO.File.Exists(pathThumbs))
            {
                System.IO.File.Delete(pathThumbs);
            }
        }

        //Вывод всех заказов для администратора
        //GET: Admin/Shop/Orders
        public ActionResult Orders()
        {
            //Инициализируем модель OrdersForAdminViewModel
            List<OrdersForAdminViewModel> ordersForAdmin = new List<OrdersForAdminViewModel>();

            using (DB db = new DB())
            {
                //Инициализируем модель заказов OrderViewModel(Приводим к массиву, строим новый порядок, приводим к списку)
                List<OrderViewModel> orders = db.Orders.ToArray().Select(x => new OrderViewModel(x)).ToList();

                //Перебираем данные модели OrderViewModel
                foreach (var order in orders)
                {
                    //Инициализируем словарь товаров: OrdersForAdminViewModel -> ProductsAndQuantity
                    Dictionary<string, int> productAndQuantity = new Dictionary<string, int>();

                    //Объявляем переменную для общей суммы
                    decimal total = 0m;

                    //Инициализируем список OrderDetailsDTO и заполняем его сделав выборку
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //Получаем имя пользователя
                    UserDTO user = db.Users.FirstOrDefault(x => x.Id == order.UserId);
                    string userName = user.UserName;

                    //Перебираем список товаров из OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        //Получаем товар относящийся к пользователю
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        //Получаем цену товара
                        decimal price = product.Price;

                        //Получаем название товара
                        string productName = product.Name;

                        //Добавляем товар в словарь
                        productAndQuantity.Add(productName, orderDetails.Quantity);

                        //Получаем полную стоимость всех товаров пользователя
                        total += orderDetails.Quantity * price;
                    }

                    //Добавляем данные в модель OrdersForAdminViewModel
                    ordersForAdmin.Add(new OrdersForAdminViewModel()
                    {
                        OrderNumber = order.OrderId,
                        UserName = userName,
                        Total = total,
                        ProductsAndQuantity = productAndQuantity,
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            //Возвращаем представление с моделью OrdersForAdminViewModel
            return View(ordersForAdmin);
        }
    }
}