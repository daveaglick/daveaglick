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
        [GET("archive")]
        public virtual ActionResult Archive()
        {
            return View();
        }
    }
}
