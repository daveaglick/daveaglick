using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttributeRouting.Web.Mvc;

namespace Somedave.Controllers
{
    public partial class BlogController : Controller
    {
        [GET("{viewName}")]
        public virtual ActionResult Post(string viewName)
        {
            // Convert underscores back to hyphens since they get converted the other way for the type name
            return View("Posts/" + viewName);
        }

        [GET("archive")]
        public virtual ActionResult Archive()
        {
            return View();
        }
    }
}
