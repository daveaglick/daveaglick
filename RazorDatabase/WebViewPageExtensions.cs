using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using ReflectionMagic;
using Moq;

namespace RazorDatabase
{
    // This is essentially from RazorGenerator.Testing without:
    // the mocked default route (since that overwrites our actual routes)
    // the HtmlAgilityPack stuff
    // the dummy view stuff
    internal static class WebViewPageExtensions
    {
        public static string Render(this WebViewPage view, object model = null)
        {
            return Render(view, null, model);
        }

        public static string Render(this WebViewPage view, HttpContextBase httpContext, object model = null)
        {
            var writer = new StringWriter();
            view.Initialize(httpContext, writer);
            view.ViewData.Model = model;
            var webPageContext = new WebPageContext(view.ViewContext.HttpContext, null, null);

            // Using private reflection to access some internals
            // Also make sure the use the same writer used for initializing the ViewContext in the OutputStack
            // Note: ideally we would not have to do this, but WebPages is just not mockable enough :(
            var dynamicPageContext = webPageContext.AsDynamic();
            dynamicPageContext.OutputStack.Push(writer);            

            // Push some section writer dictionary onto the stack. We need two, because the logic in WebPageBase.RenderBody
            // checks that as a way to make sure the layout page is not called directly
            var sectionWriters = new Dictionary<string, SectionWriter>(StringComparer.OrdinalIgnoreCase);
            dynamicPageContext.SectionWritersStack.Push(sectionWriters);
            dynamicPageContext.SectionWritersStack.Push(sectionWriters);

            // Set the body delegate to do nothing
            dynamicPageContext.BodyAction = (Action<TextWriter>)(w => { });

            view.AsDynamic().PageContext = webPageContext;
            view.Execute();

            return writer.ToString();
        }

        private static void Initialize(this WebViewPage view, HttpContextBase httpContext, TextWriter writer)
        {
            var context = httpContext ?? CreateMockContext();
            var routeData = new RouteData();

            var controllerContext = new ControllerContext(context, routeData, new Mock<ControllerBase>().Object);

            view.ViewContext = new ViewContext(controllerContext, new Mock<IView>().Object, view.ViewData, new TempDataDictionary(), writer);

            view.InitHelpers();
        }

        /// <summary>
        /// Creates a basic HttpContext mock for rendering a view.
        /// </summary>
        /// <returns>A mocked HttpContext object</returns>
        private static HttpContextBase CreateMockContext()
        {
            // Use Moq for faking context objects as it can setup all members
            // so that by default, calls to the members return a default/null value 
            // instead of a not implemented exception.

            // members were we want specific values returns are setup explicitly.

            // mock the request object
            var mockRequest = new Mock<HttpRequestBase>(MockBehavior.Loose);
            mockRequest.Setup(m => m.IsLocal).Returns(false);
            mockRequest.Setup(m => m.ApplicationPath).Returns("/");
            mockRequest.Setup(m => m.ServerVariables).Returns(new NameValueCollection());
            mockRequest.Setup(m => m.RawUrl).Returns(string.Empty);
            mockRequest.Setup(m => m.Cookies).Returns(new HttpCookieCollection());

            // mock the response object
            var mockResponse = new Mock<HttpResponseBase>(MockBehavior.Loose);
            mockResponse.Setup(m => m.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>((virtualPath) => virtualPath);
            mockResponse.Setup(m => m.Cookies).Returns(new HttpCookieCollection());

            // mock the httpcontext

            var mockHttpContext = new Mock<HttpContextBase>(MockBehavior.Loose);
            mockHttpContext.Setup(m => m.Items).Returns(new Hashtable());
            mockHttpContext.Setup(m => m.Request).Returns(mockRequest.Object);
            mockHttpContext.Setup(m => m.Response).Returns(mockResponse.Object);
            mockHttpContext.Setup(m => m.PageInstrumentation).Returns(new System.Web.Instrumentation.PageInstrumentationService()); // DG - Without this, certain calls like WriteAttribute fail

            return mockHttpContext.Object;
        }
    }
}
