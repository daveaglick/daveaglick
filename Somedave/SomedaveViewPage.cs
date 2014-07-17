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
        public string Title
        {
            get { return GetViewDataValue<string>(ViewDataKeys.Title); }
            set { ViewData[ViewDataKeys.Title] = value; }
        }

        public BootstrapHelper<T> Bootstrap
        {
            get { return Html.Bootstrap(); }
        }

        // This returns the default value if the key is not present
        protected TType GetViewDataValue<TType>(string key)
        {
            object value;
            if (ViewData.TryGetValue(key, out value) && typeof(TType).IsAssignableFrom(value.GetType()))
            {
                return (TType)value;
            }
            return default(TType);
        }
    }
}