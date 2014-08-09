using System.Web.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace RazorDatabase
{
    public static class RazorDb
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentBag<IInternalViewType>> _viewTypes 
            = new ConcurrentDictionary<Type,ConcurrentBag<IInternalViewType>>();

        // Call this from Application_Start
        // persist indicates if the database should be persisted to App_Data as JSON files
        public static void Initialize(bool persist = true)
        {
            // Reflect over all loaded assemblies looking for ViewType objects
            HashSet<Type> viewTypes = new HashSet<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type viewType in assembly.GetTypes().Where(x => typeof(IInternalViewType).IsAssignableFrom(x) && !x.IsAbstract && !x.ContainsGenericParameters))
                    {
                        viewTypes.Add(viewType);
                    }
                }
                catch (Exception)
                {
                    // Ignore if we can't get a particular type or assembly
                }
            }

            // Get the App_Data folder and create a JSON serializer
            string appData = HostingEnvironment.MapPath("~/App_Data");
            if (!Directory.Exists(appData))
            {
                Directory.CreateDirectory(appData);
            }
            JsonSerializer serializer = new JsonSerializer();

            // Check for existing persisted data if requested
            if (persist)
            {
                foreach (Type viewType in viewTypes.ToArray())
                {
                    string fileName = Path.Combine(appData, "RazorDatabase." + viewType.FullName + ".json");
                    if (File.Exists(fileName))
                    {
                        // There is an existing JSON file, so deserialize it
                        using (StreamReader streamReader = new StreamReader(fileName))
                        {
                            using (JsonReader jsonReader = new JsonTextReader(streamReader))
                            {
                                Array instanceArray = (Array)serializer.Deserialize(jsonReader, viewType.MakeArrayType());
                                if (instanceArray != null)
                                {
                                    ConcurrentBag<IInternalViewType> bag = new ConcurrentBag<IInternalViewType>(instanceArray.Cast<IInternalViewType>());
                                    _viewTypes.TryAdd(viewType, bag);
                                }
                            }
                        }
                        viewTypes.Remove(viewType);
                    }
                }
            }

            // Render the remaining view types
            List<Tuple<Type, Array>> toSerialize = new List<Tuple<Type, Array>>();
            foreach (Type viewType in viewTypes)
            {
                ConcurrentBag<IInternalViewType> bag = new ConcurrentBag<IInternalViewType>();
                IInternalViewType getViews = (IInternalViewType)Activator.CreateInstance(viewType);
                foreach (WebViewPage view in getViews.GetViews())
                {
                    IInternalViewType instance = (IInternalViewType)Activator.CreateInstance(viewType);
                    object model = instance.GetModel(view);
                    string rendered = view.Render(model);
                    instance.SetViewTypeName(view.GetType().Name);
                    instance.SetRendered(rendered);
                    instance.MapProperties(view);
                    bag.Add(instance);
                }
                if (bag.Count > 0)
                {
                    toSerialize.Add(new Tuple<Type, Array>(viewType, bag.ToArray()));
                }
                _viewTypes.TryAdd(viewType, bag);
            }

            // Serialize the view types that we rendered if requested
            if (persist)
            {
                foreach (Tuple<Type, Array> serialize in toSerialize)
                {
                    string fileName = Path.Combine(appData, "RazorDatabase." + serialize.Item1.FullName + ".json");
                    using (StreamWriter streamWriter = new StreamWriter(fileName))
                    {
                        using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
                        {
                            serializer.Serialize(jsonWriter, serialize.Item2);
                        }
                    }
                }
            }
        }

        public static IEnumerable<TViewType> Get<TViewType>()
            where TViewType : IViewType
        {
            ConcurrentBag<IInternalViewType> bag;
            if (_viewTypes.TryGetValue(typeof(TViewType), out bag))
            {
                return bag.Cast<TViewType>();
            }
            return new TViewType[] { };
        }        
    }
}
