using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Shop
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //Путь для корзины товаров
            routes.MapRoute("Cart", "Cart/{action}/{id}", new { controller = "Cart", action = "Index", id = UrlParameter.Optional }, new[] { "Shop.Controllers" });

            //Путь для создания нового аккаунта
            routes.MapRoute("Account", "Account/{action}/{id}", new { controller = "Account", action = "Index", id = UrlParameter.Optional }, new[] { "Shop.Controllers" });

            //Путь для динамического меню из _PagesMenuPartial
            routes.MapRoute("PagesMenuPartial", "Pages/PagesMenuPartial", new { controller = "Pages", action = "PagesMenuPartial" }, new[] { "Shop.Controllers" });

            //Путь для Sidebar
            routes.MapRoute("SidebarPartial", "Pages/SidebarPartial", new { controller = "Pages", action = "SidebarPartial" }, new[] { "Shop.Controllers" });

            //Путь для вывода категорий
            //Параметры: Папка со страницами Views, URL:1. Имя контроллера, метод в контроллере, параметр(id, например)
            routes.MapRoute("Shop", "Shop/{action}/{name}", new { controller = "Shop", action = "Index", name = UrlParameter.Optional }, new[] { "Shop.Controllers" });

            //Путь по умолчанию
            routes.MapRoute("Default", "", new { controller = "Pages", action = "Index" }, new[] { "Shop.Controllers" });

            //Путь ко всем страницам
            //Передаем: View(папка со страницами Pages), Url из контроллера(параметр page), присваиваем контроллеру Pages, действию - главную страницу, в массив пространства имен передаем namespace контроллера
            routes.MapRoute("Pages", "{page}", new { controller = "Pages", action = "Index" }, new[] { "Shop.Controllers" });

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}
