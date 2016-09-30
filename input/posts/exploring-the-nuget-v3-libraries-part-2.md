Title: Exploring the NuGet v3 Libraries, Part 2
Lead: Searching for packages
Published: 5/27/2016
Tags:
  - NuGet
---
In [the first part of this series](/posts/exploring-the-nuget-v3-libraries-part-1) I looked at the overall design of the new libraries and how to set up your environment. In this post I'll start to look at implementation details and specifically how to search for packages. The previous caveat still holds, and I could be leading you to use these libraries in a completely inappropriate way. Without documentation, it's hard to say. You've been warned.

## Logging

You'll probably want some visibility into the search process. Thankfully, logging in the NuGet libraries as been reasonably abstracted so it's easy enough to implement your own logging handler. This is done by creating a class derived from `NuGet.Logging.ILogger`:

```
public class Logger : ILogger
{
  public void LogDebug(string data) => // Do something
  public void LogVerbose(string data) => // Do something
  public void LogInformation(string data) => // Do something
  public void LogMinimal(string data) => // Do something
  public void LogWarning(string data) => // Do something
  public void LogError(string data) => // Do something
  public void LogSummary(string data) => // Do something
}
```

Unfortunately, there's a whole other logging abstraction that gets used for package installation, but I'll cover that in part 3.

## Getting Package Metadata

The first search activity we'll look at is getting all the metadata for a given package. There are several classes involved in this:

* `INuGetResourceProvider` - Implementations of this provide a particular "resource" (I.e., some defined class). This is similar to how traditional dependency injection containers work. NuGet uses a set of resource providers to return a particular type of class when needed (which we'll see below).
* `PackageSource` - A specific NuGet package endpoint, which is typically a URI.
* `SourceRepository` - The combination of a package source and the resource providers that can be used to act upon it.
* `PackageMetadataResource` - The particular resource that can be used to query package metadata.
* `IPackageSearchMetadata` - The result of searching package metadata containing information about version, authors, etc.

The first step is to get all of our resource providers. These will in turn provide classes that operate on the NuGet wire API for a particular version of the API (v2 or v3). We can get resource providers for the v3 API, for the older v2 API, or for both. For the former you will need to install the `NuGet.Protocol.Core.v3` package and for the later, the `NuGet.Protocol.Core.V2` package. The `.GetCoreV3()` method shown below is an extension method so you'll also need to bring the `NuGet.Protocol.Core.V3` namespace into scope to use it (and likewise for v2). This extension provides a sequence of all the available resource providers for that particular version of the API.

Next we create a `PackageSource` for a v3 endpoint. I use the official v3 endpoint below, but this could easiy be any other NuGet server including a v2 endpoint if you included the v2 resource providers. Then we combine the `PackageSource` and the resource providers into a `SourceRepository` and ask it for a `PackageMetadataResource` that can be used to search package metadata. Finally, we perform the search and get the results.

The following is a LINQPad script that can be used to execute a search. The `.Dump()` extension just outputs whatever object it's called on to the LINQPad output window. This is a great way to experiment, but feel free to use whatever tooling you want (you'll just have to remove the `.Dump()` calls).

```
async Task Main()
{
  Logger logger = new Logger();
  List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
  providers.AddRange(Repository.Provider.GetCoreV3());  // Add v3 API support
  providers.AddRange(Repository.Provider.GetCoreV2());  // Add v2 API support
  PackageSource packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
  SourceRepository sourceRepository = new SourceRepository(packageSource, providers);
  PackageMetadataResource packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
  IEnumerable<IPackageSearchMetadata> searchMetadata = await packageMetadataResource.GetMetadataAsync("Wyam.Core", true, true, logger, CancellationToken.None);
  searchMetadata.Dump();
}

public class Logger : ILogger
{
  public void LogDebug(string data) => $"DEBUG: {data}".Dump();
  public void LogVerbose(string data) => $"VERBOSE: {data}".Dump();
  public void LogInformation(string data) => $"INFORMATION: {data}".Dump();
  public void LogMinimal(string data) => $"MINIMAL: {data}".Dump();
  public void LogWarning(string data) => $"WARNING: {data}".Dump();
  public void LogError(string data) => $"ERROR: {data}".Dump();
  public void LogSummary(string data) => $"SUMMARY: {data}".Dump();
}
```

## Performing a Full Search

Performing a full search is very similar, it just uses a different resource. To perform a search you'll need to get the `PackageSearchResource`. The code above is all the same except for the last three lines of the `Main()` method.

```
async Task Main()
{
  // ...
  PackageSearchResource searchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>();
  IEnumerable<IPackageSearchMetadata> searchMetadata = await searchResource.SearchAsync("Json.NET", new SearchFilter(), 0, 10, logger, CancellationToken.None);
  searchMetadata.Dump();
}

// Logger
```

This will return a `IPackageSearchMetadata` for each result package. You can then use this object to get information about the available versions, metadata, etc.

To control whether you want to limit the search to packages that support a particular framework or to pre-release or unlisted packages, you can use an alternate constructor for the `SearchFilter` class.

## Framework

You'll need to know how to get the framework if you want to search for packages that are compatible with your current application. NuGet has its own class to represent this, but thankfully it's easy to get using reflection:

```
string frameworkName = Assembly.GetExecutingAssembly().GetCustomAttributes(true)
  .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
  .Select(x => x.FrameworkName)
  .FirstOrDefault();
NuGetFramework currentFramework = frameworkName == null
  ? NuGetFramework.AnyFramework
  : NuGetFramework.ParseFrameworkName(frameworkName, new DefaultFrameworkNameProvider());
```

Of course, you can also substitute any assembly for the currently executing one to get the correct NuGet framework for that assembly. Alternatively, if you already know the name of the framework you want to search for, you can get that using a call to the static method `NuGetFramework.ParseFrameworkName()`.

## Other Resources

There are many other resources you can use to do different things. In fact, a good way to explore the NuGet library support for the protocols is to find all the implementations of `INuGetResource` in the `NuGet.Core` solution using a tool like ReSharper. Here are some more:

* `DownloadResource` - Downloads packages (we'll look at this more in part 3).
* `DependencyInfoResource` - Gets dependency information for a given package.
* `PackageUpdateResource` - Contains methods to push or delete a package to/from a local repository.

Note that the whole resource model is fairly low-level and directly supports the on-the-wire NuGet protocols. While they provide you with adequate control for most simple use cases, they may not be the best way to support more complex scenarios. In part 3 we'll look at some higher-level abstractions designed to interoperate with project files and the host.

Update: [Part 3 is available here](/posts/exploring-the-nuget-v3-libraries-part-3).
