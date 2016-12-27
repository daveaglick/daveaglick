Title: Serving Extensionless URLs with OWIN
Lead: My first OWIN middleware.
Published: 7/8/2015
Tags:
  - OWIN
---

[I recently](https://wyam.io) had a need for an embedded web server. In the past, I've used a variety of libraries for this purpose (including building my own kind-of standards-compliant HTTP server - don't do this), but have always had my eye on the [OWIN specification](http://owin.org/) and related projects. Specifically, the [Katana project](http://katanaproject.codeplex.com/documentation) from Microsoft looked interesting. Each previous time I investigated it, it looked like it was still pretty rough and needed a little more time. I was pleased to find what looked like a mature and easy to integrate library this time around though.

There is lots of great documentation and many blog posts about what OWIN is and how it works so I won't go into detail here. In a nutshell, OWIN provides a standard interface to hook into the web serving pipeline and do whatever you need to do to the request and/or response. The idea is that you can easily change the behavior of your web host (be it embedded, on a server, etc.) by swapping out different "middleware".

For Wyam, I wanted the embedded server to handle extensionless URLs. An extensionless URL is one in which the web server infers the file extension based on what files are available without having to specify it directly in the URL. For example, if there is a file named `mypage.html` you could access it at `http://mydomain.com/mypage` instead of `http://mydomain.com/mypage.html`. These extensionless URLs are easier to remember, are favored by search engines, and allow for easier migration to other technologies. Several web hosts support this concept (including GitHub Pages) and if someone is deploying to those hosts (or configured their own server to be extensionless), I wanted the Wyam preview server to support it.

Unfortunatly, Katana (and OWIN) doesn't support this idea out of the box. I also searched for custom middleware to do this and came up empty. Time to learn how to write OWIN middleware...

It was remarkably easy. I copied the conventions for some of the default middleware in Katana and just changed the behavior a little bit. I'll start with the options class:

```
public class ExtensionlessUrlsOptions
{
    public ExtensionlessUrlsOptions()
    {
        // Prioritized list
        DefaultExtensions = new List<string>()
        {
            ".htm",
            ".html"
        };
    }

    public IList<string> DefaultExtensions { get; set; }
    public IFileSystem FileSystem { get; set; }
}
```

This just holds the list of default extensions to check and the OWIN `IFileSystem` object that will be used to check for files.

The actual middleware class is below:

```
using AppFunc = Func<IDictionary<string, object>, Task>;

public class ExtensionlessUrlsMiddleware
{
    private readonly ExtensionlessUrlsOptions _options;
    private readonly AppFunc _next;

    public ExtensionlessUrlsMiddleware(AppFunc next, ExtensionlessUrlsOptions options)
    {
        if (next == null)
        {
            throw new ArgumentNullException("next");
        }
        if (options == null)
        {
            throw new ArgumentNullException("options");
        }
        if (options.FileSystem == null)
        {
            options.FileSystem = new PhysicalFileSystem(".");
        }
        options.DefaultExtensions =
            options.DefaultExtensions.Select(x => x.StartsWith(".") ? x : ("." + x)).ToList();

        _next = next;
        _options = options;
    }

    public Task Invoke(IDictionary<string, object> environment)
    {
        IOwinContext context = new OwinContext(environment);
        if (IsGetOrHeadMethod(context.Request.Method)
            && !PathEndsInSlash(context.Request.Path))
        {
            // Check if there's a file with the matched extension, and rewrite the request if found
            foreach (string extension in _options.DefaultExtensions)
            {
                string filePath = context.Request.Path + extension;
                IFileInfo fileInfo;
                if (_options.FileSystem.TryGetFileInfo(filePath, out fileInfo))
                {
                    context.Request.Path = new PathString(filePath);
                    break;
                }
            }
        }

        return _next(environment);
    }

    // These methods are from Microsoft.Owin.StaticFiles.Helpers
    private static bool IsGetOrHeadMethod(string method)
    {
        return IsGetMethod(method) || IsHeadMethod(method);
    }

    private static bool IsGetMethod(string method)
    {
        return string.Equals("GET", method, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHeadMethod(string method)
    {
        return string.Equals("HEAD", method, StringComparison.OrdinalIgnoreCase);
    }

    private static bool PathEndsInSlash(PathString path)
    {
        return path.Value.EndsWith("/", StringComparison.Ordinal);
    }
}
```

You can see that besides some housekeeping code, it's very straightforward. Literally all we're doing is checking if a file exists with one of our default extensions and then rewritting the query to include the extension if a file does exist. That way, downstream middleware will just see the request with the extension and it'll work as if the extension were there all along.

To actually use the middleware, I wrote some short extensions:

```
public static class ExtensionlessUrlsExtensions
{
    public static IAppBuilder UseExtensionlessUrls(this IAppBuilder builder)
    {
        return builder.UseExtensionlessUrls(new ExtensionlessUrlsOptions());
    }

    public static IAppBuilder UseExtensionlessUrls(this IAppBuilder builder, ExtensionlessUrlsOptions options)
    {
        if (builder == null)
        {
            throw new ArgumentNullException("builder");
        }

        return builder.Use<ExtensionlessUrlsMiddleware>(options);
    }
}
```

And then placed this code where I spin up the server:

```
WebApp.Start(options, app =>
{
    IFileSystem outputFolder = new PhysicalFileSystem(/* output folder */);

    // ...
    
    app.UseExtensionlessUrls(new ExtensionlessUrlsOptions
    {
        FileSystem = outputFolder
    });
    
    // ...
});
```