using Shop.Models.Data;
using Shop.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PageController : Controller
    {
        // GET: Admin/Page
        public ActionResult Index()
        {
            //список для представления
            List<PageViewModel> pageList = new List<PageViewModel>();

            //инициализируем подключение к БД
            using(DB dB = new DB())
            {
                pageList = dB.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageViewModel(x)).ToList();
            }

            //возвращаем список из представления
            return View(pageList);
        }

        // GET: Admin/Page/AddPage
        [HttpGet]//то, что получаем с сервера
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/Page/AddPage
        [HttpPost]//то, что отправляем на сервер
        public ActionResult AddPage(PageViewModel model)
        {
            //проверка модели на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (DB dB = new DB())
            {
                //объявляем переменную для краткого описания
                string description = null;

                //инициализируем класс PagesDTO
                PagesDTO pagesDTO = new PagesDTO();

                //Присваиваем заголовок модели
                pagesDTO.Title = model.Title.ToUpper();

                //Проверка наличия и присвоение краткого описания
                if (string.IsNullOrWhiteSpace(model.Description))
                {
                    description = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    description = model.Description.Replace(" ", "-").ToLower();
                }

                //Проверка на уникальность заголовка и краткого описания
                if (dB.Pages.Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("titleError", "Этот заголовок не доступен");//создаем ключ ошибки и сообщение

                    return View(model);
                }
                else if(dB.Pages.Any(x => x.Description == model.Description))
                {
                    ModelState.AddModelError("descriptionError", "Это описание не доступно");

                    return View(model);
                }

                //Присваиваем оставшиеся значения модели
                pagesDTO.Description = description;
                pagesDTO.Body = model.Body;
                pagesDTO.HasSidebar = model.HasSidebar;
                pagesDTO.Sorting = 100;

                //Сохраняем модель в базу данных
                dB.Pages.Add(pagesDTO);//добавляем в БД НОВУЮ ЗАПИСЬ. Использовать ТОЛЬКО при добавлении новой записи
                dB.SaveChanges();//сохраняем изменения
            }

            //Передаем соoбщение через TempData
            TempData["OK_Message"] = "Операция успешно выполнена!";//комментарий успеха операции

            //Переадресовываем пользователя на метод INDEX
            return RedirectToAction("Index");
        }

        // GET: Admin/Page/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //Инициализуируем модель PageViewModel
            PageViewModel model;

            using (DB dB = new DB())
            {
                //Получаем данные страницы
                PagesDTO pagesDTO = dB.Pages.Find(id);//модели страницы присваиваем все значения которые находим в БД по id

                //Проверка доступности страницы
                if (pagesDTO == null)
                {
                    return Content("Данная страница не доступна");
                }

                //Инициализируем модель данными из PageDTO
                model = new PageViewModel(pagesDTO);
            }

            //Возвращаем модель в представление
            return View(model);
        }

        // POST: Admin/Page/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageViewModel model)
        {
            //Проверить модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (DB dB = new DB())
            {
                //Получаем ID страницы
                int id = model.Id;

                //Объявляем переменную для описания
                string description = null;

                //Получаем страницу (по ID)
                PagesDTO pagesDTO = dB.Pages.Find(id);

                //Получаем название от пользователя в модель PageDTO
                pagesDTO.Title = model.Title;

                //Проверка наличия описания и присвоение его
                if (string.IsNullOrWhiteSpace(model.Description))
                {
                    description = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    description = model.Description.Replace(" ", "-").ToLower();
                }

                //Проверка описания и заголовка на уникальность
                if (dB.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("titleExist", "Это заголовок не доступен");

                    return View(model);
                }
                else if(dB.Pages.Where(x => x.Id != id).Any(x => x.Description == description))
                {
                    ModelState.AddModelError("descriptionExist", "Это описание не доступно");

                    return View(model);
                }

                //Записываем остальные значения в класс PageDTO
                pagesDTO.Description = description;
                pagesDTO.Body = model.Body;
                pagesDTO.HasSidebar = model.HasSidebar;

                //Сохраняем изменения в БД
                dB.SaveChanges();//При редактировании просто сохраняем в БД новые значение поверх старых
            }

            //Оповещение про успех операции в TempData
            TempData["OK_Message"] = "Страница успешно изменена!";

            //Переадресация на главную страницу
            return RedirectToAction("Index");
        }

        // GET: Admin/Page/PageDetails/id
        [HttpGet]
        public ActionResult PageDetails(int id)
        {
            //Объявляем модель PageViewModel
            PageViewModel model = new PageViewModel();

            using (DB dB = new DB())
            {
                //Получаем страницу
                PagesDTO pagesDTO = dB.Pages.Find(id);

                //Проверка доступности страницы
                if(pagesDTO == null)
                {
                    return Content("Страница не доступна");
                }

                //Присваиваем модели информацию из БД
                model = new PageViewModel(pagesDTO);
            }

            //Возвращаем модель в представление
            return View(model);
        }

        //Удаление страниц
        // GET: Admin/Page/DeletePage/id
        public ActionResult DeletePage(int id)
        {
            using (DB dB = new DB())
            {
                //Получение страницы
                PagesDTO pagesDTO = dB.Pages.Find(id);

                //Удаление страницы
                dB.Pages.Remove(pagesDTO);

                //Сохранение изменений в базе
                dB.SaveChanges();
            }

            //Оповещение про успех операции
            TempData["OK_Message"] = "Страница успешно удалена";

            //Переадресация пользователя на страницу Index
            return RedirectToAction("Index");
        }

        //Создаем метод сохранения порядка сортировки
        // GET: Admin/Page/ReorderPages
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (DB dB = new DB())
            {
                //Реализуем начальный счетчик
                int count = 1;

                //Инициализируем модель данных
                PagesDTO pagesDTO;

                //Устанавливаем сортировку для каждой страницы
                foreach (var pageId in id)
                {
                    pagesDTO = dB.Pages.Find(pageId);
                    pagesDTO.Sorting = count;

                    dB.SaveChanges();

                    count++;
                }
            }
        }

        // GET: Admin/Page/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Инициализируем модель
            SidebarViewModel model;

            using (DB dB = new DB())
            {
                //Получить данные из SidebarDTO
                SidebarDTO sidebarDTO = dB.Sidebars.Find(1);

                //Заполнить модель данными
                model = new SidebarViewModel(sidebarDTO);
            }

            //Вернуть представление с моделью
            return View(model);
        }

        // POST: Admin/Page/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarViewModel model)
        {
            using (DB dB = new DB())
            {
                //Получаем данные из SidebarDTO
                SidebarDTO sidebarDTO = dB.Sidebars.Find(1);

                //Присваиваем данные в тело (в свойство Body)
                sidebarDTO.Body = model.Body;

                //Сохранить
                dB.SaveChanges();
            }

            //Сообщение про успех операции
            TempData["OK_Message"] = "Панель отредактирована успешно";

            //Переадресация
            return RedirectToAction("EditSidebar");
        }
    }
}