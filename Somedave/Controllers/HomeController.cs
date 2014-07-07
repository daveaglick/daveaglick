using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttributeRouting.Web.Mvc;

namespace Somedave.Controllers
{
    public partial class HomeController : Controller
    {
        [GET("")]
        public virtual ActionResult Index()
        {
            return View();
        }

        [GET("about")]
        public virtual ActionResult About()
        {
            return View();
        }

        [GET("fluentbootstrap")]
        public virtual ActionResult FluentBootstrapProject()
        {
            return View();
        }

        [GET("razordatabase")]
        public virtual ActionResult RazorDatabaseProject()
        {
            return View();
        }
    }
}
