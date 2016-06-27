Title: Exploring the NuGet v3 Libraries, Part 1
Lead: Introduction and concepts
Published: 5/18/2016
Tags:
  - NuGet
---
The NuGet version 3 libraries have been available for a while, both [on GitHub](https://github.com/NuGet/NuGet.Client) and on the [public NuGet gallery](https://www.nuget.org/packages/NuGet.PackageManagement). Despite advice by the NuGet team that they aren't ready for public consumption yet, it's been over six months since the first release of NuGet v3 and many packages are starting to require v3 clients for installation. If you already have an application that uses NuGet libraries, this is, or will shortly become, a problem for your application as more and more packages start only supporting the newer client versions.

This is the position I found myself in with [Wyam](http://wyam.io) when I started looking at moving our v2 integration to v3. This post looks at some of what I found and hopefully provides some helpful guidance if you want to integrate NuGet with your own application. Given the concerns of the NuGet team over the libraries not being "ready", I'll try to focus on how to use them and refrain from commentary on code quality except where it might be relevant to successfully integrating. That said, be warned: there's a good reason they aren't considered ready for public use yet. Also note that there doesn't appear to be any attempt to maintain a stable API. The public surface of these libraries will almost certainly change from one release to the next and this post will probably be out of date pretty quickly.

One more caveat: this is from my own attempts to integrate. It required a lot of digging through code and reverse engineering to get as far as I did. There could be much better ways to accomplish some of these tasks and some of my approaches might just be flat-out wrong. I certainly welcome clarifications or corrections.

This series will be split into a few different posts:
* In this first post I'll go over the new libraries in general and introduce some core concepts.
* In the next post ([available here](/posts/exploring-the-nuget-v3-libraries-part-2)) I'll talk about how to find packages, specify package sources, etc.
* In the final post ([available here](/posts/exploring-the-nuget-v3-libraries-part-3)) I'll look at the process of actually installing packages.

# What You'll Need

It used to be pretty simple to consume NuGet packages programmatically, and the NuGet team [wrote a post that described exactly what to do](http://blog.nuget.org/20130520/Play-with-packages.html). With the v2 libraries, there was just one `NuGet.Core` package you had to install and you were up and running. v3 deviates considerably from this and has a more micro-dependency architecture. I count about 40 v3 packages [on the public gallery](https://www.nuget.org/profiles/nuget). Even for my relatively simple use case (which I'll discuss in a bit) I needed to install 24 separate NuGet packages.

Despite the large number of packages, only a handful should really be considered "primary" and the rest are just dependencies.
* [`NuGet.PackageManagement`](https://www.nuget.org/packages/NuGet.PackageManagement) provides the overall management of packages, package sources, projects, etc.
* [`NuGet.Protocol.Core.v2`](https://www.nuget.org/packages/NuGet.Protocol.Core.v2/) and [`NuGet.Protocol.Core.v3`](https://www.nuget.org/packages/NuGet.Protocol.Core.v3/) provide implementations of the NuGet communications protocols so that you can talk to v2 or v3 feed endpoints.
* [`NuGet.ProjectManagement`](https://www.nuget.org/packages/NuGet.ProjectManagement/) provides the abstractions for representing a given project system and includes some fundamental implementations such as folder-based projects (more on projects below).
* [`NuGet.Packaging`](https://www.nuget.org/packages/NuGet.Packaging/) reads .nuspec and .nupkg files.

There are certainly other packages that could be considered "primary", but these are the ones I found myself using most directly. One thing to watch out for is that the package dependencies often seem a bit arbitrary or unnecessary. For example, `NuGet.PackageManagement` has a dependency on `NuGet.Commands`, which provides support for "commands" like packing. The idea of a "command" seems counter to the idea of directly embedding an API in your own application, like its intended to support some other application that isn't yours. This brings up another thing to keep in mind: the semantics of the v3 libraries can seem odd. I found multiple instances of classes, methods, etc. having names I wouldn't have expected. And speaking of naming, there are also a lot of conflicts with the old v2 libraries. This is especially troublesome because some of the v2 libraries are still referenced from the newer v3 libraries. It's very easy to find yourself asking "why isn't this compiling" only to find that you're `using` statements are referencing an older v2 namespace and not the newer one.

Given the points above and the lack of *any* documentation for these libraries, you'll almost certainly need to do some reverse engineering. A decomplier is handy for taking a look at the public surface area of each library (once that can decompile on the fly inside Visual Studio is even better). You'll probably also end up pulling down the entire [NuGet client source](https://github.com/NuGet/NuGet.Client) at some point too. Note that if you want to build it though, [the dev branch currently uses some private resources](https://github.com/NuGet/Home/issues/2616) so you'll have to build an older version.

# Key Concepts

In the old v2 libraries, there wasn't a ton of abstraction. Installing a package was four or five fairly direct commands to create a repository, list available packages, create a package manager, and install the package you want. The v3 libraries add a lot of extra abstraction, factories, etc. to this equation so similar tasks become a lot more complex. All this abstraction appears to be there in support of the different official clients (Visual Studio, command line, PowerShell, etc.) This is a common theme in the v3 libraries: a lot of the classes, methods, etc. are tied directly in name or function to a specific official client. For example, code to support Visual Studio is found everywhere. Despite the abstraction there isn't really a "pure" client-agnostic interface available.

There are a few key concepts that you'll need to know to do anything useful:

* *Project* - Installing and uninstalling packages is tied directly to the concept of a project with the apparent intent that these operations probably have a direct impact on something else like a project file or a list of resources. You can't just "install a package", you have to "install a package into a project". The `NuGet.ProjectManagement.NuGetProject` class is the base of all project implementations.
* *Context* - Operations on the project are often done with a context. While a lot of the functionality of the `NuGet.ProjectManagement.INuGetProjectContext` interface is tied to one or another specific project implementations, the important part is that this is also where you can intercept log messages.
* *Package Manager* - The package manager, an instance of `NuGet.PackageManagement.NuGetPackageManager`, orchestrates package operations such as an install or uninstall. It's the primary object that (mostly) ties everything else together.
* *Package Source* - A package source is a specific NuGet feed endpoint.
* *Protocol* - NuGet now supports two different protocols, v2 and v3, and not all package sources support all protocols. Protocols are exposed to callers via *resource providers*.
* *Repository* - A repository combines a specific package source with the protocols it supports.
* *Version* - (Almost) every package has a version and the NuGet libraries provide support for parsing and comparing them in the `NuGet.Versioning.NuGetVersion` class.
* *Package Identity* - A package identity is the combination of a package version and a package ID.

There are certainly other types of classes that we'll need to do anything useful, and we'll go over those in the next two parts of this series.

# Where To Start

There's almost no documentation about the new libraries. In addition to the source code and a decompiler, I found [this issue](https://github.com/NuGet/Home/issues/1870) to be somewhat helpful in getting started. As far as the source code goes, I also found that digging through [the implementations of various NuGet command line commands](https://github.com/NuGet/NuGet.Client/tree/dev/src/NuGet.Clients/NuGet.CommandLine/Commands) was a good place to start.

But really, it's just *a lot* of trial and error. Hopefully this series can also help once all the parts are done. Next time: searching for packages.

Update: [Part 2 is available here](/posts/exploring-the-nuget-v3-libraries-part-2) and [part 3 is here](/posts/exploring-the-nuget-v3-libraries-part-3).
 
