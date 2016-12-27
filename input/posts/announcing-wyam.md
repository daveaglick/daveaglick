Title: Announcing Wyam
Lead: A modular static content generator built on Roslyn, Razor, and rainbows.
Published: 6/28/2015
Tags:
  - Wyam
  - static site generator
  - open source
  - Roslyn
  - .NET Compiler Platform
  - Razor
---

I am very proud to announce my newest project, [Wyam](https://wyam.io). It's a static site and content generator built from the ground up to be modular and flexible.

Here's the thing: I like static site generators. I've been thinking about this problem for a long time (as in, many years). In fact, I did a roundup of [.NET static site generators](/posts/a-survey-of-dotnet-static-site-generators) not too long ago. And while there are *a lot* of generators out there, none of them really fit with the way I think about the problem. I don't want to follow a prescribed notion of what sort of content I'm creating or follow assumptions about the location of files. I wanted a static *content* generator that is designed from the ground up to be flexible, even if it means making it slightly more complicated to configure. There are some generators that come close (I'm personally fond of the concepts in [Metalsmith](http://www.metalsmith.io/)). I'm also a .NET developer and while there are a couple good static site generators in the .NET ecosystem ([Pretzel](https://github.com/Code52/pretzel) and [Sandra.Snow](https://github.com/Sandra/Sandra.Snow) come to mind), I certainly don't think we've hit peak generator in that world as we have in, say, JavaScript.

More to the point, I'm a developer. You're (probably) a developer. Why do most static site generators ignore that the people most likely to use them are developers? I'm all for making things easy, but I also want the ability to use my development skills to create the site that *I* want to create. By making lots of assumptions and abstracting away so much of the process, I get the feeling a lot of the generators out there are try to cater to my mom. Except my mom will never use a static site generator. If I told her to "generate a blog with Jekyll" she'd say "that's nice dear, why don't you talk to your father about that."

While I've had ideas about how I would make such a thing for a long time, it wasn't until recently that the tools to realize my vision finally became available. Specifically, maturity of the [.NET Compiler Platform](https://github.com/dotnet/roslyn) has finally made it practical to create applications that can compile their own code. This meant that in addition to providing lots of great abstractions and extensibility points to make static content generation *easy*, I could also provide a mechanism to configre and extend the process for more *flexibility*.

Most of that flexibility is evident in the way you configure Wyam. [Wyam configuration files](https://wyam.io/getting-started/configuration) are written in C#. You can make extra classes and helper methods. You can create new base pages for the [Razor module](https://wyam.io/modules/razor). You can use delegates to configure modules. Whatever you need it to do, it can do, because you can write the code to do it.

But don't get me wrong, it also does a lot for you up front. Here's a list of the current features:

  - Written in .NET and <a href="https://wyam.io/knowledgebase/writing-a-module">easily extensible</a>
  - <a href="https://wyam.io/getting-started/installation">Low ceremony</a> - download a zip file, unzip, and run
  - Flexible <a href="https://wyam.io/getting-started/configuration">script-based configuration</a> using the power of the .NET Compiler Platform (Roslyn)
  - Lots of <a href="https://wyam.io/modules">modules</a> for things like <a href="https://wyam.io/modules/readfiles">reading</a> and <a href="https://wyam.io/modules/writefiles">writing</a> files, handling <a href="https://wyam.io/modules/frontmatter">frontmatter</a>, and manipulating <a href="https://wyam.io/modules/metadata">metadata</a>
  - <a href="https://wyam.io/modules/yaml">YAML parser</a>
  - <a href="https://wyam.io/modules/less">Less CSS compiler</a>
  - Support for multiple templating languages including <a href="https://wyam.io/modules/razor">Razor</a>
  - Integrated <a href="https://wyam.io/getting-started/usage">web server</a> for previewing output
  - Integrated <a href="https://wyam.io/getting-started/usage">file watching</a> and regeneration
  - Full <a href="https://wyam.io/getting-started/configuration#nuget">NuGet support</a>
  - <a href="https://wyam.io/knowledgebase/embedded-use">Embeddable engine</a>
  
In fact, to dogfood Wyam I generated the [Wyam.io](https://wyam.io) site with it (meta!). I also refactored this blog to be generated (a post on that coming soon). In the near future, I'll also be adding many more modules including JSON and Liquid (check out the [GitHub issues](https://github.com/Wyamio/Wyam/issues) for a look at upcoming features). I'd also like to explore using Wyam for non-site things like generating documentation or eBooks. I would love to attract developers of all backgrounds, but my greatest hope is that Wyam can catch on as a kind of anti-Jekyll for the .NET crowd.

Oh, and about the name. *Wyam* is a Native American name for the [Celilo Falls](https://en.wikipedia.org/wiki/Celilo_Falls) area and is also roughly translated as "echo of falling water" or "sound of water upon the rocks". Which sounds kind of like static. For a static site generator. Get it? I also liked the image of water going over the falls as one thing then going through a transition as it emerged at the bottom as something else. Plus the name just sounds cool, and the domain was available. It's also very searchable (looking at you [Visual Studio Code](https://code.visualstudio.com/)).
  