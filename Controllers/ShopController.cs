using PagedList;
using Shop.Models.Data;
using Shop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shop.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()//Метод Index уже есть, потому переадресовываем его на первичный Index метод
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            //Объявляем модель типа List CategoryViewModel
            List<CategoryViewModel> categoryVMList;

            //Инициализируем модель данными
            using(DB db = new DB())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryViewModel(x)).ToList();
            }

            //Возвращаем частичное представление с моделью
            return PartialView("_CategoryMenuPartial", categoryVMList);
        }

        //GET: Shop/Category/name
        public ActionResult Category(string name, int? page)
        {
            //Объявляем список List
            List<ProductViewModel> productVMList;

            //Устанавливаем номер страницы
            var pageNumber = page ?? 1;

            using (DB db = new DB())
            {
                //Получаем id категории
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Description == name).FirstOrDefault();

                int catId = categoryDTO.Id;

                //Инициализируем список данными
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductViewModel(x)).ToList();

                //Получаем имя категории
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                //Проверка на NULL
                if(productCat == null)//если в категории нет товаров, чтоб не получить исключение
                {
                    var catName = db.Categories.Where(x => x.Description == name).Select(x => x.Name).FirstOrDefault();//если null, то берем имя категории напрямую из БД
                    ViewBag.CategoryName = catName;
                }
                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;
                }
            }

            //Устанавливаем постраничную навигацию
            var PageOfProducts = productVMList.ToPagedList(pageNumber, 3);//первым аргументом передаем номер страницы, вторым - количество товара на странице
            ViewBag.PageOfProducts = PageOfProducts;

            //Возвращаем постраничное представление с моделью
            return View(PageOfProducts);
        }

        //GET: Shop/product-details/name
        [ActionName("product-details")]//задаем отображаемое имя метода действия
        public ActionResult ProductDetails(string name)
        {
            //Объявляем модели ProductDTO и ProductVM
            ProductDTO productDTO;
            ProductViewModel productViewModel;

            //Инициализируем id продукта
            int id = 0;

            using (DB db = new DB())
            {
                //Проверка на доступность продукта
                if (!db.Products.Any(x => x.Title.Equals(name)))//если не один title из БД != передаваемому имени
                {
                    return RedirectToAction("Index", "Shop");//переадресация на главную
                }

                //Наполняем модель DTO данными
                productDTO = db.Products.Where(x => x.Title == name).FirstOrDefault();//делаем выборку того элемента который равен переданному аргументу

                //Получаем id
                id = productDTO.Id;

                //Инициализируем модель ViewModel данными
                productViewModel = new ProductViewModel(productDTO);
            }

            //Получаем изображения из галерии продукта
            productViewModel.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/"
                + id + "/Gallery/Thumbs")).Select(fileNames => Path.GetFileName(fileNames));

            //Возвращаем модель в представление
            return View("ProductDetails", productViewModel);
        }
    }
}