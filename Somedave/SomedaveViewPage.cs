using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FluentBootstrap;

namespace Somedave
{
    public abstract class SomedaveViewPage<T> : WebViewPage<T>
    {
        public BootstrapHelper<T> Bootstrap
        {
            get { return Html.Bootstrap(); }
        }
    }
}