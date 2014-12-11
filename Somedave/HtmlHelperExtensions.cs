using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FluentBootstrap;
using FluentBootstrap.Buttons;
using Somedave.Controllers;
using FluentBootstrap.Mvc;

namespace Somedave
{
    public static class HtmlHelperExtensions
    {
        public static LinkButton<MvcBootstrapHelper<TModel>> TagButton<TModel>(this HtmlHelper<TModel> helper, string tag, int? count = null, ButtonState buttonState = ButtonState.Default)
        {
            UrlHelper url = new UrlHelper(helper.ViewContext.RequestContext);
            return helper.Bootstrap().LinkButton(
                string.Format(" {0}{1}", tag, count == null ? string.Empty : string.Format(" <span class='badge'>{0}</span>", count)), 
                url.Action(MVC.Blog.Tags(tag.ToLowerInvariant().Replace(' ', '-'))))
                .SetState(buttonState)
                .SetSize(ButtonSize.Sm)
                .AddCss("tag-button", "icon-tag-2");
        }

        public static MvcHtmlString PostLink<TModel>(this HtmlHelper<TModel> helper, string linkText, Func<BlogController.ViewsClass._PostsClass, string> view)
        {
            return helper.ActionLink(linkText, PostAction(helper, view));
        }

        public static ActionResult PostAction<TModel>(this HtmlHelper<TModel> helper, Func<BlogController.ViewsClass._PostsClass, string> view)
        {
            return PostAction(view);
        }

        // Not really an HtmlHelper, but similar enough to the one above to include it here
        public static ActionResult PostAction(this Controller controller, Func<BlogController.ViewsClass._PostsClass, string> view)
        {
            return PostAction(view);
        }

        private static ActionResult PostAction(Func<BlogController.ViewsClass._PostsClass, string> view)
        {
            return MVC.Blog.Posts(System.IO.Path.GetFileNameWithoutExtension(view(MVC.Blog.Views.Posts)));
        }

        public static IHtmlString Code(this HtmlHelper htmlHelper, string code, string language = "csharp")
        {
            return new HtmlString(string.Format(@"<pre><code class=""language-{0}"">{1}</code></pre>", language,
                htmlHelper.Raw(HttpUtility.HtmlEncode(code.Replace("Bootstrap(this)", "Bootstrap()")))));
        }
    }
}