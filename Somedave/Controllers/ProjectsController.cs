using AttributeRouting.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Somedave.Controllers
{
    public partial class ProjectsController : Controller
    {
        [GET("fluentbootstrap")]
        public virtual ActionResult FluentBootstrapProject()
        {
            return View();
        }

        [GET("razordatabase")]
        public virtual ActionResult RazorDatabase()
        {
            return View();
        }
    }
}