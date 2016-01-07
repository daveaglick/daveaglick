Title: NuGet API Survey
Lead: What projects are using the NuGet API?
Published: 1/7/2016
Tags:
  - NuGet
---
By now, most .NET developers are at least a bit familiar with NuGet, the NuGet command line interface, and the NuGet GUI in Visual Studio. But did you know there's another way to use NuGet? The project has published libraries (on NuGet, how meta!) for a while. You can reference these libraries from your own code to make *your* application create packages, download them, extract them, etc. In NuGet version 2, most of the surface API was contained in a library called NuGet.Core. However, the move to NuGet version 3 is bringing some big changes including a transition to many smaller and more granular API libraries. I use the NuGet APIs in some of my own projects and I was wondering how many other projects use them and in what ways. There isn't a lot of documentation on the APIs, so looking at other code is one of the best ways to figure out what works and what doesn't. I was also hopeful it might give me some clues to the future of the NuGet APIs and how best to transition to version 3.

I started by performing exhaustive research and conducting complex GitHub searches. Just kidding. I posted on Twitter:

<blockquote class="twitter-tweet" lang="en"><p lang="en" dir="ltr">Curious about something: I know a few apps/libs use NuGet directly: Paket, LINQPad, Chocolatey, Cake (via CLI)... Any others?</p>&mdash; Dave Glick (@daveaglick) <a href="https://twitter.com/daveaglick/status/683654871380312064">January 3, 2016</a></blockquote>
<script async src="//platform.twitter.com/widgets.js" charset="utf-8"></script>

Between what I already knew about field usage of the NuGet API and the responses to my Tweet, I looked at several projects that use the NuGet API in one way or another (and one that does API-like stuff without actually using the API). Note that this isn't an in-depth code review, or even all that technical. Mainly, I was just curious where to start looking for examples if I wanted to explore further and though others might be interested too.

## Wyam

I'll start with my own project, [Wyam](https://github.com/Wyamio/Wyam). It's an extensible static site generator that uses NuGet to allow you to download and extract packages, both for including third-party libraries in your static build process and for distributing static content files (like the Bootstrap stylesheets). The actual usage is pretty simple. The user can specify a package by name along with an optional version specification, and Wyam will go pull it down (in needed) and extract it to the appropriate place.

I'm currently using version 2 of the NuGet API, primarily contained in the NuGet.Core library I mentioned earlier. However, I've also found myself referencing other NuGet libraries like NuGet.Frameworks, NuGet.Logging, and NuGet.Packaging (all of which are part of the version 3 API). Most of these came in as dependencies of NuGet.Frameworks, which I'm using independently of the core version 2 library just to compare framework versions.

## scriptcs

[scriptcs](http://scriptcs.net/) has tight NuGet integration, allowing you to install packages from your scripts and from the command line. It also uses NuGet to create and use [Script Packs](https://github.com/scriptcs/scriptcs/wiki/Script-Packs), which are reusable bits of functionality to make writing specific kinds of scripts easier. There's also a proposal for [Script Libraries](https://github.com/scriptcs/scriptcs/wiki/Script-Libraries) which will use NuGet in a similar way to Script Packs but are intended to provide user-specified APIs to your scripts. Currently, scriptcs [uses NuGet.Core version 2](https://github.com/scriptcs/scriptcs/wiki/NuGet-v3) which can cause problems with version 3 package sources.

## LINQPad

[Having spent some serious time with it's internals](/posts/announcing-linqpad-codeanalysis), I know [LINQPad](https://www.linqpad.net/) has some [serious NuGet integration](https://www.linqpad.net/Purchase.aspx#NuGet) in it's higher-end editions. It basically tries to recreate a similar experience to that offered by the Visual Studio GUI. You can search for packages, download them to a cache, add them to your query, etc. Last I checked, LINQPad was using the version 2 NuGet API (though that may have changed, but I doubt it). 

## Internal Tools

I received feedback that at least one company was using the NuGet API along with an internal feed to publish previous versions of their packages and then pull those down with testing tools for backwards compatibility tests. I have no idea how these actually work under the hood (they're internal), but I really like the concept. I especially like the idea that by using the NuGet APIs you can add specialized behavior to your own software that isn't associated with the "publish a package to consume as a reference" use case. 

## Sqlci

The sqlci tool (from Red Gate) uses the NuGet.Core API to package and publish database scripts. 

## Paket

[Paket](https://fsprojects.github.io/Paket/) is a dependency manager that supports NuGet packages as well as other sources such as Git repositories or HTTP resources. It has it's own packaging format, but is compatible with NuGet's. This means it needs to use the NuGet APIs to read NuGet packages as one of it's supported package formats. It can also pull packages from a NuGet server (such as the public NuGet gallery), though it's unclear to me if this functionality uses the NuGet API or a custom protocol implementation (I seem to remember reading something about the latter, but can't find it now). There are a lot of other NuGet integration points as well, and it appears that Paket has already implemented NuGet version 3 compatibility in a lot of places.

## Cake

[Cake](http://cakebuild.net/) is a build tool that can fetch NuGet packages as part of the build process. It does a neat end run around the confusion and complexity of the API by just farming out NuGet calls to the CLI (this seems to be a standard way of integrating external tools in Cake). For simple NuGet uses, this is probably far easier than programming against the poorly documented API, but you also give up some of the finer control. The API also exposes a lot of functionality that isn't present in the CLI.

It turns out Cake *also* uses the API for a very specific purpose. Because licensing restrictions on the early version of the Roslyn scripting libraries prohibited it from being distributed along with applications, the Cake team did something rather clever and uses the NuGet API to pull down the Roslyn scripting libraries from NuGet at run time. Runtime package download and library binding is an interesting use of the NuGet APIs and I'm a little surprised I haven't seen it more often to get around licensing, reduce initial download size, bootstrap libraries, etc.

## Squirrel

[Squirrel](https://github.com/Squirrel/Squirrel.Windows) is a very interesting case. It's an application installation and updating framework that uses NuGet under the hood. However, it doesn't use standard NuGet packages. Instead, it creates "delta packages" that contain only the deltas from some previous NuGet package to the current one. This allows it to efficiently distribute application updates without sending the whole package over again. I love this idea because it represents using the NuGet API to make use of some aspects of the technology while tailoring the exact use to the need at hand.

## ReSharper

Another product that uses NuGet packages for installation and deployment scenarios is the ReSharper Unified Installer. There's a [great blog post here](https://blog.jetbrains.com/dotnet/2015/07/01/resharper-unified-nuget-based-installer-how/) on how and why JetBrains selected NuGet for distributing their updates. It mostly comes down to flexibility, and they've done some neat things with package metadata and gallery hosting (Azure-based vs. local depending on the kind of install).

In addition, the [ReSharper Gallery](https://resharper-plugins.jetbrains.com/) is also based on NuGet. It works similarly to how the main NuGet functionality in Visual Studio can be used to add libraries to your projects. In this case, ReSharper uses the NuGet API to find, download, and extract extensions for use in the product.  

## Chocolatey

[Chocolatey](https://chocolatey.org/) essentially *is* NuGet (or at least the "gallery" idea) for whole applications instead of application dependencies. It makes extensive use of the NuGet version 2 libraries and [this discussion](https://github.com/NuGet/Home/issues/1870) with the NuGet team is a good indication of how challenging it is right now to move to version 3 APIs given the lack of documentation.

# Summary

I would love to see more custom uses of the NuGet API. This type of thing should be easy and supported. NuGet is more than just a repository of libraries for you to use as references. It's a format, protocol, and API for packaging, versioning, and associating assets (be they libraries, executables, content, etc.) There are probably a lot of "outside the box" use cases (see Paket) that could be addressed, but many developers aren't aware it's even an option. Unfortunately, this message isn't getting across very clearly right now, and it's even more confusing given the transition from version 2 to version 3. I'm hopeful that in the future as the NuGet team is able to relax from meeting the requirements of the rapidly evolving ASP.NET and .NET Core product teams, some more attention is given to features, documentation, and messaging around using NuGet in your own applications.