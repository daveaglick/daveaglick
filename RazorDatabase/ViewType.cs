using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RazorDatabase
{    
    internal interface IInternalViewType
    {
        IEnumerable<WebViewPage> GetViews();
        object GetModel(WebViewPage webViewPage);
        void SetRendered(string rendered);
        void MapProperties(WebViewPage webViewPage);
    }

    public interface IViewType
    {
    }

    public abstract class ViewType<TViewPage> : IInternalViewType, IViewType
        where TViewPage : WebViewPage
    {
        public string Rendered { get; set; }

        void IInternalViewType.SetRendered(string rendered)
        {
            Rendered = rendered;
        }

        IEnumerable<WebViewPage> IInternalViewType.GetViews()
        {
            return GetViews().Cast<WebViewPage>();
        }

        // This gets all views that can be assigned to TViewType in all loaded assemblies
        protected virtual IEnumerable<TViewPage> GetViews()
        {
            List<TViewPage> views = new List<TViewPage>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.Contains("Somedave"))
                {
                    int test = 0;
                }
                try
                {
                    foreach (Type viewType in assembly.GetTypes().Where(x => typeof(TViewPage).IsAssignableFrom(x) && GetView(x)))
                    {
                        views.Add((TViewPage)Activator.CreateInstance(viewType));
                    }
                }
                catch (Exception)
                {
                    // Ignore if we can't get a particular type or assembly
                }
            }
            return views;
        }

        // Override this to specify on a per-view-type basis which views should be included for this view type
        protected virtual bool GetView(Type x)
        {
            return true;
        }

        object IInternalViewType.GetModel(WebViewPage webViewPage)
        {
            return GetModel((TViewPage)webViewPage);
        }

        // This returns a null model by default
        protected virtual object GetModel(TViewPage webViewPage)
        {
            return null;
        }

        void IInternalViewType.MapProperties(WebViewPage webViewPage)
        {
            MapProperties((TViewPage)webViewPage);
        }

        // This attempts to fill public properties by first looking for matching property names and types in the view and then checking the ViewBag for matching keys
        protected virtual void MapProperties(TViewPage webViewPage)
        {
            foreach (PropertyInfo destinationProp in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite && x.Name != "Rendered"))
            {
                // Check the view page for a matching property
                PropertyInfo sourceProp = typeof(TViewPage).GetProperty(destinationProp.Name, BindingFlags.Public | BindingFlags.Instance);
                if (sourceProp != null && sourceProp.CanRead && destinationProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                {
                    destinationProp.SetValue(this, sourceProp.GetValue(webViewPage));
                    continue;
                }

                // Try ViewData
                object sourceValue;
                if (webViewPage.ViewData.TryGetValue(destinationProp.Name, out sourceValue) && sourceValue != null && destinationProp.PropertyType.IsAssignableFrom(sourceValue.GetType()))
                {
                    destinationProp.SetValue(this, sourceValue);
                    continue;
                }

                // Try ViewBag (MVC uses an ExpandoObject for ViewBag, so we should be able to cast it to IDictionary)
                IDictionary<string, object> viewBag = webViewPage.ViewBag as IDictionary<string, object>;
                if (viewBag != null && viewBag.TryGetValue(destinationProp.Name, out sourceValue) && sourceValue != null && destinationProp.PropertyType.IsAssignableFrom(sourceValue.GetType()))
                {
                    destinationProp.SetValue(this, sourceValue);
                    continue;
                }
            }
        }
    }
}
