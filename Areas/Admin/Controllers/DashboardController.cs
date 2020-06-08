using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]//Authorize - говорит о том, что к этой области имеет доступ ТОЛЬКО залогинившийся администратор
    public class DashboardController : Controller
    {
        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}