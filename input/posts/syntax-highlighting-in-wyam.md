Title: Syntax Highlighting In Wyam
Lead: How to make your code look pretty.
Published: 9/28/2016
Image: /images/chalk.jpg
Tags:
- Wyam
---
I'm going to try and blog about [Wyam](http://wyam.io) a bit more. As more folks start using it, I'm getting some really interesting questions about how to do things. Given how flexible Wyam is there's almost always multiple answers too, which makes exploring these questions a great exercise in discovering various features. In today's post I'll look at how to add syntax highlighting to code blocks in generated content.

# Client-Side

One of the most common ways to include syntax highlighting in web pages is to rely on a JavaScript library to insert the highlights at runtime on the client. In other words, your page just contains the raw code surrounded by an HTML element that indicates it needs to be processed (usually by adding a CSS class to the elements that contains the code to highlight). Then a JavaScript library looks for each of the elements that needs to be transformed and adds additional HTML elements for things like color and style once the page is loaded. The result is dynamically generated block of HTML that has your original code content plus additional elements like `span` inserted by the JavaScript library. This approach is easy since you just have to include the library and make sure the code blocks are properly delineated.

## Adding A CSS Class To Markdown Code Fences

If you're using the [Markdown module](http://wyam.io/modules/markdown) to render Markdown documents, then you can use a "code fence" to indicate code blocks with three tick marks before and after the code. In most Markdown processors this will result in surrounding the code with `<pre><code>` elements. That's usually sufficient to make sure the code the properly monospaced, but it doesn't do anything for highlighting.

If we want to add highlighting to those `<pre><code>` blocks, we can use a JavaScript library like [Prettify](https://github.com/google/code-prettify). There are many such libraries available (Prettify is my personal favorite) and they all work in essentially the same way. Most key off of a CSS class to indicate which content needs to be transformed. Thankfully, adding such as class to the `<pre><code>` blocks from our Markdown results is easy thanks to the [Replace module](http://wyam.io/modules/replace). Here's an example `config.wyam` file:

```
#n Wyam.Markdown
#n Wyam.Razor

Pipelines.Add("Pages",
	ReadFiles("**/*.md"),
	FrontMatter(Yaml()),
	Markdown(),
	Replace("<pre><code>", "<pre class=\"prettyprint\"><code>"),
	Razor(),
	WriteFiles(".html")
);
```

Note the [Razor module](http://wyam.io/modules/razor) in there. It's optional, but can be used to wrap your Markdown files in a standard layout using Razor conventions like a `_ViewStart.cshtml` file. Presumably, the layout is where you would include the actual Prettify script reference.

# Generated

In contrast to using a client-side JavaScript library to generate the necessary highlighting markup, you can create the markup as part of the generation process. This is a little trickier to set up but has a number of advantages including performance and compatibility. Since all the markup for the highlighting is added to the end result of generation, the client doesn't have to do anything.

Let's assume for the remaining examples that we have a .NET library that can take a string containing source code and covert it to a string of HTML with the appropriate elements for syntax highlighting. [ColorCode](https://colorcode.codeplex.com/) is one such library, and there are others like [syntaxtame](https://github.com/shayanelhami/syntaxtame). You could even write your own using regular expressions, Roslyn, etc. That's outside the scope of this post though. What's important is that we have a library that we need to call to create the HTML that represents our highlighted code.

## External Code Samples

One approach would be to store the code samples externally. Then you could setup a pipeline that reads them in and processes their content. Then in your main pipeline, iterate over each of the code documents from the previous pipeline, performing replacements using a generated token. Here's a complete example. Assume you have a code sample in an external file named `samples/somecode.cs` that looks like this:

```
int foo = 1 + 2;
```

Let's also assume we have a Markdown file:

~~~
This is an example post.

```
SAMPLE:SOMECODE
```

And that's my code!
~~~

Here's a `config.wyam` using the approach described above:

```
#n Wyam.Markdown

// Very simple implementation that just returns a wrapping
// span, presumably you'd use a library for this instead
public static class SyntaxHighlighter
{
    public static string Highlight(string input)
    {
        return $"<span class='code'>{input}</span>";
    }
}

// Load the code content and add syntax highlighting
Pipelines.Add("Samples",
    // Read the sample files from a special directory
    ReadFiles("samples/*"),
    // Apply the syntax highlighting
    Content(SyntaxHighlighter.Highlight(@doc.Content)),
    // Calculate and store the replacement token for each
    Meta("Token", $"SAMPLE:{@doc.String("SourceFileBase").ToUpper()}")
);

// Load the pages and insert the syntax highlighted code
Pipelines.Add("Pages",
    // Read all the Markdown files
    ReadFiles("**/*.md"),
    // Render the Markdown to HTML
    Markdown(),
    // Create a Replace module for each of the sample documents
    // and then execute them against each page document
    Execute(@ctx.Documents["Samples"]
        .Select(x => Replace(x.String("Token"), x.Content))),
    // Write the final results
    WriteFiles(".html")
);
```

The output of this config would be an HTML page that looks like this:

```
<p>This is an example post.</p>
<pre><code><span class='code'>int foo = 1 + 2;</span>
</code></pre>
<p>And that's my code!</p>
```

This approach is also extremely flexible. For example, instead of getting samples from the local file system you could grab them from Gists, from a database, etc.

## Highlighting In Place

Perhaps you have a whole bunch of code samples or you just don't want to maintain them separately. Another way to perform generation-time highlighting is to transform code blocks and add highlighting elements directly. The main trick here is finding where in your pages the code blocks are and swapping out their content. This is a good example of how to use a little custom code to tailor your site generation.

Let's again assume that we have a Markdown file, this time without a sample code file but with the code inside the page instead:

~~~
This is an example post.

```
int foo = 1 + 2;
```

And that's my code!
~~~

The Markdown processor will render the code block inside `<pre><code>` elements, so we just need to find them, process what's inside, and replace them with the result. Thankfully there's a great library called [AngleSharp](https://github.com/AngleSharp/AngleSharp) that makes this really easy. We can include the AngleSharp package in our config file and use a bit of custom code inside the [Execute](http://wyam.io/modules/execute) module to do the work:

```
#n Wyam.Markdown
#n AngleSharp

using AngleSharp.Parser.Html;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;

// Assume the same SyntaxHighlighter class as above

// We'll need an AngleSharp HTML parser
HtmlParser htmlParser = new HtmlParser();

// Load the pages
Pipelines.Add("Pages",
    // Read all the Markdown files
    ReadFiles("**/*.md"),
    // Render the Markdown to HTML
    Markdown(),
    // Perform the replacement
    Execute((doc, ctx) =>
    {
        // Parse the document content and get each code element
        IHtmlDocument htmlDocument = htmlParser.Parse(doc.Content);
        foreach(IElement codeElement in 
            htmlDocument.QuerySelectorAll("pre code"))
        {
            // Highlight and replace the inner HTML of the code element
            codeElement.InnerHtml = 
                SyntaxHighlighter.Highlight(codeElement.InnerHtml);
        }
        // Replace the content of the input document with the new HTML
        return htmlDocument.DocumentElement.OuterHtml;
    }),
    // Write the final results
    WriteFiles(".html")
);
```

The output will be the same as above. Hopefully this also demonstrates the flexibility that comes with a static generator that can run arbitrary code.

# The Easy Way

Now that we've gone over several approaches to doing this manually, there's actually a much easier way. The built-in [Blog recipe](http://wyam.io/recipes-themes/blog) does syntax highlighting by default. It uses the Prettify JavaScript library mentioned above to perform syntax highlighting on the client. The recipe is easy to use and doesn't require messing with config files (unless you want to!). This command scaffolds a new site:

```
wyam new -r Blog
```

Then, once you create some more pages and posts, this command will build your site using the recipe and the default theme:

```
wyam -r Blog
```

# Conclusion

We've looked at three different ways (four if you count the recipe) of adding syntax highlighting. Between [all the modules](http://wyam.io/modules/), custom code, etc. there's certainly other ways you could accomplish this too. More than that, I hope this article has given you an appreciation for the flexibility in Wyam and how it can be used to satisfy niche static generation requirements.