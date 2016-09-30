Title: Converting My Blog to Wyam
Lead: How I went from compiled to static in less than a day.
Published: 7/7/2015
Tags:
  - Wyam
  - static site generator
  - meta
  - blog
---

I [recently launched a new static site generator](/posts/announcing-wyam), and I figured what better test of whether it's ready for widespread use than to convert my entire blog to use it. Given that this blog was originally built with ASP.NET MVC, it should be a good fit for converting over to a Razor-based static site generator. The process was actually  easier than I thought it would be and suggests that [Wyam](http://wyam.io) is already ready for production use on personal sites, blogs, etc.

I went into the process with a few requirements:
- The rendered HTML had to match the original site. I wasn't interested in redesigning things at the same time I was converting the backend.
- I wanted to support Markdown documents as well as Razor pages (which was the only option before).
- The new site couldn't be more complex than the old site. For example, site layouts still had to be contained in a single file, etc.

I also decided that I wanted to see if I could easily support an alternate development environment that didn't involve Visual Studio (since Visual Studio wouldn't have recognized the collection of files Wyam was reading as a normal web site project anyway). I took the opportunity to really dig in to [Visual Studio Code](https://code.visualstudio.com/) and am happy to report I have fallen in love with it as a lightweight editing alternative when you don't need a full IDE. I also fired up Wyam in a console with the command `Wyam.exe --watch --preview` and left it running in the background while I worked. That way, any time I saved a file, the site would get regenerated and I could (almost) immediately preview it in a browser using the local Wyam web server.

The first step was getting all the layout files to work. Because the [Wyam Razor module](http://wyam.io/modules/razor) already supports the standard Razor layout file conventions, this mostly amounted to copying and pasting the layout and content files to the new site. Once I got the general layout working, I starting bringing over some of the non-blog pages (like [about](/about) and [likes](/likes)). These were originally Razor pages so I just left them that way. This is where I hit my first snag. The original site used a custom Razor base page to expose a page property that the views could use for easy access to [FluentBootstrap](http://fluentbootstrap.com). Getting [FluentBootstrap](http://fluentbootstrap.com) into the static site wasn't hard as I had already developed a [NuGet package for this purpose](https://www.nuget.org/packages/FluentBootstrap.Wyam), but exposing the property so I didn't have to rewrite all my views turned out to be a challenge. To resolve this, I added support to the [Wyam Razor module](http://wyam.io/modules/razor) for specifying base pages and then created my base page class right in the Wyam config file for the site:

```
public abstract class RazorPage : BaseRazorPage
{
  public WyamBootstrapHelper Bs
  {
    get { return Html.Bootstrap(); }
  }
}
// ...
Pipelines.Add(
  // ...
  Razor(typeof(RazorPage))
  // ...
);
```

Once I had the non-blog pages working, I moved on the blog posts. In the previous site each post specified it's metadata (like title, published date, etc.) by setting additional Razor page properties from the custom base class. In this case, I had a better mechanism for specifying metadata and used [YAML](http://wyam.io/modules/yaml) [front matter](http://wyam.io/modules/frontmatter) for the metadata. This required me to manually change the declarations at the top of each blog post from something like this:

```
@{
    Title = "Announcing LINQPad.CodeAnalysis";
    Lead = ".NET Compiler Platform helpers and utilities for LINQPad.";
    Published = new DateTime(2015, 3, 18);
    Tags = new[] { "LINQPad", "open source", "Roslyn", ".NET Compiler Platform" };
}
```

To something like this:

```
Title: Announcing LINQPad.CodeAnalysis
Lead: .NET Compiler Platform helpers and utilities for LINQPad.
Published: 3/18/2015
Tags:
  - LINQPad
  - open source
  - Roslyn
  - .NET Compiler Platform
---
```

It's a nice side benefit that the YAML is easier to read as well. This process took about an hour for all my posts.

The last step was to convert over the more dynamic pages in the site, specifically the [post archives](/posts) and [list of tags](/tags) and cooresponding tag archives. For the post archive, I wrote a LINQ statement that used the Wyam metadata to fetch all the posts in the site:

```
@{
  foreach(IGrouping<int, IDocument> year in Documents
    .Where(x => x.ContainsKey("Published"))
    .OrderByDescending(x => x.Get<DateTime>("Published"))
    .GroupBy(x => x.Get<DateTime>("Published").Year)
    .OrderByDescending(x => x.Key))
  {
    <h3>@year.Key</h3>
    // ...
  }
}
```

Then for the tags, I did something similar (this uses the special `ToLookup` extension for handling metadata lookup in Wyam):

```
@{    
  var DocumentsByTag = Documents
    .ContainsKey("Published")
    .ToLookup<string>("Tags");
}
// ...
@{
  foreach (var tagDocuments in DocumentsByTag.OrderBy(x => x.Key))
  {
    // ...
  }
}
```

Finally, to generate the individual archive page for each tag, I added a special pipeline that outputs a page per tag:

```
Pipelines.Add("Tags",
  ReadFiles(@"tags\index.cshtml"),
  FrontMatter(),
  Execute((doc, ctx) => ctx.Documents
    .Where(x => x.ContainsKey("Published") && x.ContainsKey("Tags"))
    .SelectMany(x => x.Get<string[]>("Tags"))
    .Distinct()
    .Select(x => doc.Clone(new Dictionary<string, object>()
    { 
      { "Title", x },
      { "Tag", x }
    }))),
  Razor(typeof(RazorPage)),
  WriteFiles(x => HtmlHelperExtensions.GetTagLink(x.String("Tag")) + ".html")
);
```

In total, it probably took me about 6 hours to convert over the entire site. While it wasn't turn-key, that's not really the point anyway. I could have built the site from scratch on a site generator that prescribed a specific file convention, metadata, etc. but instead I was able to relativly easily adapt an existing, fairly complex site without too much trouble. That's the reason I built Wyam, to give developers a powerful tool to build exactly the content they want, the way they want to build it. Now it's [building automatically from AppVeyor on every commit](http://wyam.io/knowledgebase/continuous-integration), I can use [Markdown](http://wyam.io/modules/markdown), and it gets served lightning-fast for free from [GitHub Pages](https://pages.github.com/). If you're interested in doing something similar, [check out the source code for this site](https://github.com/daveaglick/daveaglick) as an example. And please don't hesitate to ask here or on Twitter for help!