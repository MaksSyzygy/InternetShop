using Shop.Models.Data;
using Shop.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shop.Controllers
{
    public class PagesController : Controller
    {
        //Метод настройки доступа к основному layout, где настраиваем что будем выводить на экран (представления, частичные представления, другое)
        // GET: Index/{page}
        public ActionResult Index(string page = "")//если не будет передан аргумент, формат будет ввиде пустой строки
        {
            //Получаем/устанавливаем краткий заголовок (Title)
            if(page == "")
            {
                page = "home";
            }

            //Объявляем модель и контекст данных (DTO)
            PageViewModel pageViewModel;
            PagesDTO pagesDTO;

            //Проверяем доступность текущей страницы
            using(DB db = new DB())
            {
                if(!db.Pages.Any(x => x.Title.Equals(page)))//если не найдено совпадений по title
                {
                    return RedirectToAction("Index", new { page = ""});//возвращаем пустую строку и переадресация уйдет на главную
                }
            }

            //Получаем контекст данных страницы (DTO), если страница найдена
            using(DB db = new DB())
            {
                pagesDTO = db.Pages.Where(x => x.Title == page).FirstOrDefault();//находит первое совпадение и зписывает его в контекст данных
            }

            //Устанавливаем заголовок страницы (Title)
            ViewBag.PageTitle = pagesDTO.Title;

            //Проверяем боковую панель (Sidebar)
            if (pagesDTO.HasSidebar)
            {
                ViewBag.Sidebar = "Yes";//есть боковая панель
            }
            else
            {
                ViewBag.Sidebar = "No";//или нет
            }

            //Заполняем модель данными
            pageViewModel = new PageViewModel(pagesDTO);

            //Возвращаем представление с моделью
            return View(pageViewModel);
        }

        public ActionResult PagesMenuPartial()
        {
            //Иниализируем List PageViewModel
            List<PageViewModel> pageVMList;

            //Получаем все страницы, кроме Home
            using(DB db = new DB())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Title != "home")
                    .Select(x => new PageViewModel(x)).ToList();//выбираем из БД страницы, приводим к массиву, сортируем по полю сортировки, 
                //исключаем страницу по title home, выбираем и заносим обработанные данные в представление, приводим к List
            }

            //Возвращаем ЧАСТИЧНОЕ представление и List с данными
            return PartialView("_PagesMenuPartial", pageVMList);//указываем обязательно имя частичного представления
        }

        public ActionResult SidebarPartial()
        {
            //Объявляем модель
            SidebarViewModel sidebarViewModel;

            //Инициализируем модель данными
            using(DB db = new DB())
            {
                SidebarDTO sidebarDTO = db.Sidebars.Find(1);//ищем сайдбар по индексу 1

                sidebarViewModel = new SidebarViewModel(sidebarDTO);
            }

            //Возвращаем модель в частичное представление
            return PartialView("_SidebarPartial", sidebarViewModel);
        }
    }
}