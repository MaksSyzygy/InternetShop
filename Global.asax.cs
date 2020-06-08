using Shop.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Shop
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //Метод обработки запросов аутентификации
        protected void Application_AuthenticateRequest()
        {
            //Проверка авторизации пользователя
            if (User == null)
            {
                return;
            }

            //Получаем имя пользователя
            string userName = Context.User.Identity.Name;

            //Объявляем массив ролей пользователей
            string[] roles = null;

            using (DB db = new DB())
            {
                //Заполняем массив ролями
                UserDTO userDTO = db.Users.FirstOrDefault(x => x.UserName == userName);

                if(userDTO == null)//проверка на Null при опции смены logina пользователем
                {
                    return;
                }

                roles = db.UserRoles.Where(x => x.UserId == userDTO.Id).Select(x => x.Role.Name).ToArray();
            }

            //Создаем объект интерфейса IPrincipal
            IIdentity userIdentity = new GenericIdentity(userName);
            IPrincipal newUserObject = new GenericPrincipal(userIdentity, roles);

            //Объявляем и инициализируем данными Context.User
            Context.User = newUserObject;
        }
    }
}
