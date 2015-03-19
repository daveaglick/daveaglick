using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using RazorDatabase;
using DaveAGlick.Controllers;

namespace DaveAGlick
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            RazorDb.Initialize(false);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Get the exception (if there is one)
            Exception ex = Server.GetLastError();

            // TODO: Log the exception
            
            // Set the status code to trigger the error handling in Application_EndRequest
            ((MvcApplication)sender).Context.Response.StatusCode =
                ex is HttpException ? ((HttpException)ex).GetHttpCode() : 500;
        }

        protected void Application_EndRequest()
        {
            if (Context.Response.StatusCode >= 400)
            {
                // Get (or create) an exception
                Exception ex = Server.GetLastError() ?? new HttpException(Context.Response.StatusCode, Context.Response.StatusDescription);

                // Clear the error and set the response
                Context.ClearError();
                Context.Response.Clear();
                Context.Response.StatusCode = ex is HttpException ? ((HttpException)ex).GetHttpCode() : 500;
                Context.Response.TrySkipIisCustomErrors = true;

                // Specify the controller and action to handle the error
                ErrorController controller = new ErrorController();
                RouteData routeData = new RouteData();
                routeData.Values["controller"] = "Error";
                routeData.Values["action"] = Context.Response.StatusCode == 404 ? "NotFound" : "Index";
                Context.Response.TrySkipIisCustomErrors = true;

                // Return the error view
                ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
            }
        } 
    }
}