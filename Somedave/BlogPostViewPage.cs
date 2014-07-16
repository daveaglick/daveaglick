using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Somedave
{
    public abstract class BlogPostViewPage<T> : SomedaveViewPage<T>
    {
        public string Title
        {
            get { return GetViewDataValue<string>(ViewDataKeys.Title); }
            set { ViewData[ViewDataKeys.Title] = value; }
        }

        public string Lead
        {
            get { return GetViewDataValue<string>(ViewDataKeys.Lead); }
            set { ViewData[ViewDataKeys.Lead] = value; }
        }

        public DateTime Published
        {
            get { return GetViewDataValue<DateTime>(ViewDataKeys.Published); }
            set { ViewData[ViewDataKeys.Published] = value; }
        }

        public DateTime Edited
        {
            get { return GetViewDataValue<DateTime>(ViewDataKeys.Edited); }
            set { ViewData[ViewDataKeys.Edited] = value; }
        }

        public string[] Tags
        {
            get { return GetViewDataValue<string[]>(ViewDataKeys.Tags); }
            set { ViewData[ViewDataKeys.Tags] = value; }
        }

        // This returns the default value if the key is not present
        private TType GetViewDataValue<TType>(string key)
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