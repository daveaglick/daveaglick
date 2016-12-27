Title: Announcing A New Documentation Experience
Lead: Easy to customize API, content/wiki pages, and blog posts in the new Wyam Docs recipe.
Published: 12/22/2016
Image: /images/code.jpg
Tags:
  - Wyam
  - documentation
---
# A Short Introduction

When I started working on [Wyam](http://wyam.io) about 2 years ago, a primary design goal (probably *the* primary goal) was to create a general-purpose static generator that could be easily adapted to any sort of content from the most complicated web site to output that isn't web-based at all. I was frustrated with both the lack of a popular and robust generator in the .NET ecosystem (why should Ruby and Node get all the fun?) and also with the limitations of the generators that do exist on other platforms. Nearly all of them favor strong conventions and patterns, and while many are extensible, creating experiences that vary too widely from the accepted ones becomes challenging fast (a notable exception to this is Metalsmith, which is similar in spirit to Wyam).

If you look at Wyam as a *generator toolkit*, a tool to build the perfect static generator for your specific purpose, this concept becomes clearer. Jessica can use Wyam to create a generator for her marketing site, while Marcus can use it to create a generator for his blog, and you can use it to create a generator for that eBook you've been working on. Wyam assumes nothing about the domain and output and instead provides building blocks and a framework for *your* static generator.

Practically speaking however, there are a number of standard patterns. Blogs are a great example. Every blog typically has some combination of posts, archives, and tags or categories. To accommodate these sorts of uses the notion of a *recipe* was always part of the plan (it was literally [issue #1](https://github.com/Wyamio/Wyam/issues/1)) and their implementation was introduced about six months ago with the first Blog recipe. This turns Wyam into a powerful blogging static generator if you want it to be.

Even with recipes and blog support, it was clear that the potential of this concept hadn't been realized yet. Today I am thrilled to announce the culmination of 2 years of hard work by presenting the Docs recipe. This recipe lets you easily build documentation for your projects including a blog, content/Wiki, and automatically generated API docs from .NET sources or assemblies that rivals anything produced by doc-specific generators. Perhaps more importantly it realizes that original vision of creating a static generation toolkit and then using the building blocks to create amazing purpose-specific generators.

# The Blog Recipe

As with all Wyam recipes, the blog recipe is designed to get up and running quickly. In the most basic scenario, just [download Wyam](http://wyam.io/docs/usage/obtaining) and scaffold a new documentation site:

```
wyam new -r Docs
```

This will create some sample pages like a wiki and blog post. You can edit these pages, add more, and customize your site using any editor and then run the following to build and preview it:

```
wyam -r Docs
```

Wikis and blogs are cool, but what really makes this recipe special is that it has full support for API documentation from .NET source files and assemblies. By default the recipe will look in a `src` folder adjacent to your `input` folder (the actual default [globbing pattern](http://wyam.io/docs/concepts/io#globbing) for locating source files is "src/**/{!bin,!obj,!packages,!*.Tests,}/**/*.cs"). This can be easily changed, along with many other settings, by editing [global metadata](http://wyam.io/recipes/docs/global-metadata). Once the recipe knows where to look for your source files, you'll see a special "API" section in your documentation with really nice looking API pages.

<img src="/posts/images/api-docs.png" class="img-responsive"></img>

Some of the other features of the recipe include:

- Nested documentation content pages.
- Blog posts with optional category and author.
- Paged blog archives for posts, categories, authors, and dates.
- Posts and pages can be in Markdown or Razor.
- API documentation from source files.
- API documentation from assemblies (with or without XML documentation file from MSBuild).
- Static site searching for API types.
- Meta-refresh redirects and/or a Netlify redirect file.
- RSS, Atom, and/or RDF feeds.

[Visit the Docs recipe section on the Wyam site](http://wyam.io/recipes/docs) for more information.

# More Power!

But why would you want to use this instead of one of the many excellent documentation-specific generators like [DocFx](https://dotnet.github.io/docfx/)? The most compelling answer to that question is the customization abilities Wyam provides. Because [a recipe](http://wyam.io/docs/concepts/recipes) in Wyam is just a combination of the same [modules](http://wyam.io/docs/concepts/modules) and [pipelines](http://wyam.io/docs/concepts/pipelines) you could have built on your own, bending them to your will is easy once you understand what's going on behind the scenes.

A good example is the [modules](http://wyam.io/modules) section on the Wyam website. That page, and the detail pages for each module, are automatically generated from a single template file by examining the code analysis output that the Docs recipe already performs to generate the API documentation. Making use of that information to create an additional experience required very little additional code (just one extra pipeline in the [configuration file](http://wyam.io/docs/usage/configuration) and a single Razor template). I'd venture that heavily data-driven customizations like this aren't this easy in any other generator.

# A Piece Of Cake

I need to take a moment here and thank the [Cake team](https://github.com/orgs/cake-build/people) for their support of this project. For the last several months, I've been working closely with them to convert their existing ASP.NET powered documentation site (which was already one of the best open source doc sites out there, at least in the .NET ecosystem) to a fully-static site powered by the new Wyam Docs recipe. It was a great use case requiring both built-in functionality and site-specific customization and helped mould this initial release of the Docs recipe. I couldn't have asked for better team mates. The result is [a beautiful, fast, and robust documentation site](http://cakebuild.net). The source code for the new Cake site is [available on GitHub]() and serves as a great example of Wyam Docs recipe deployment with some additional customization.

<img src="/posts/images/cake-docs.png" class="img-responsive"></img>

# Go Forth And Build

If you're interested in taking a look, I'm happy to help. [The Wyam website](http://wyam.io) has detailed documentation, there's an acttive [Gitter room](https://gitter.im/Wyamio/Wyam), and I try to stay responsive with [GitHub issues](https://github.com/Wyamio/Wyam/issues).
