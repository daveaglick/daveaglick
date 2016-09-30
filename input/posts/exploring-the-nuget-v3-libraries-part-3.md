Title: Exploring the NuGet v3 Libraries, Part 3
Lead: Installing packages
Published: 6/27/2016
Tags:
  - NuGet
---
In [the first part of this series](/posts/exploring-the-nuget-v3-libraries-part-1) I looked at the overall design of the new libraries and how to set up your environment. In [the second part](/posts/exploring-the-nuget-v3-libraries-part-2) I explained how to search for packages and examined some of the resources provided by the NuGet libraries. In this final part of the series, I'll dive into installing packages. This is a little bit more complicated than the activities in the previous posts because installing a package is actually a much more complex action when you take into account things like current platform and dependency chains.

Note that in the first part of this series I mentioned that installing a package with the NuGet API makes some assumptions about the calling application. Specifically, that it's installing packages as part of a *project* and needs to update other artifacts during the install process. You can't just "install a package", you have to "install a package into a project". Thankfully, the libraries have some abstractions regarding this concept and we can substitute our own implementations to help define what a "project" means to us (if anything).

# Dependency Resolution

One of the fundemental aspects of any package manager is how it handles package dependencies. By default, NuGet typically restores the lowest matching version of a package thought slightly different rules apply for `project.json` projects vs. `package.config` projects. It's not actually important to know what kind of resolution strategy each kind of NuGet project uses, just that there *is* such a thing as a dependency resolution strategy. It's what determines which additional packages to install beyond the ones you explicitly specify and ensures that packages you do want to install have all the dependencies they need to work.

Note that using the resource classes described in [part 2](/posts/exploring-the-nuget-v3-libraries-part-2) you could replicate dependency replication however you wanted to. There are resources for downloading packages, placing them in a local folder, etc. The process of identifying dependencies, resolving dependency conflicts, and automatically downloading dependencies is really what separates simple *downloading* of packages from *installing* them.

For the API classes that deal with installing packages, the dependency resolution behavior is controlled by an instance of `ResolutionContext`. The `ResolutionContext` constructor has four arguments:

```
public ResolutionContext(DependencyBehavior dependencyBehavior, bool includePrelease, bool includeUnlisted, VersionConstraints versionConstraints)
```

The `DependencyBehavior` enum specifies what kind of resolution will be performed during package install. For example, suppose you're installing package A and it depends on package B. Let's also assume that package A indicates it needs a version of package B version 3 or higher and that package B has versions 1, 2, 3, 4, and 5 available. If you use the "lowest applicable version" resolution behavior that NuGet uses by default, the installation process would install version 3 of package B. However, you could also use a "latest version" strategy and install version 5 of package B. The `DependencyBehavior` enum has the following members:

```
public enum DependencyBehavior
{
  Ignore,
  Lowest,
  HighestPatch,
  HighestMinor,
  Highest
}
```

The other enum that controls dependency resolution is `VersionConstraints`, which indicates which versions of a dependent package the installer is able to consider. It's a flag (meaning you can specify more than one value) and has the following members:

```
[Flags]
public enum VersionConstraints
{
  None = 0,
  ExactMajor = 1,
  ExactMinor = 2,
  ExactPatch = 4,
  ExactRelease = 8,
}
```

# Project Context

Recal that in [part 2](/posts/exploring-the-nuget-v3-libraries-part-2) we could log activity using an implementation of `ILogger`. Unfortunately, the APIs that install packages actually requires a different mechanism of forwarding log messages. In addition, the project context also tells the installation routines what to do in certain situations like if a file conflict occurs. All of this is handled by an implementation of `INuGetProjectContext`. The most important members for custom implementations are probably `ResolveFileConflict(string message)` and `Log(MessageLevel level, string message, params object[] args)`. Here's a barebones implementation that should work fine for most simple package installation purposes:

```
public class ProjectContext : INuGetProjectContext
{
    public void Log(MessageLevel level, string message, params object[] args)
    {
        // Do your logging here...
    }

    public FileConflictAction ResolveFileConflict(string message) => FileConflictAction.Ignore;

    public PackageExtractionContext PackageExtractionContext { get; set; }

    public XDocument OriginalPackagesConfig { get; set; }

    public ISourceControlManagerProvider SourceControlManagerProvider => null;

    public ExecutionContext ExecutionContext => null;

    public void ReportError(string message)
    {
    }

    public NuGetActionType ActionType { get; set; }
}
```

# Projects

While installing packages, the package manager (discussed below) attempts to install each package *into* a project. The project is then responsible for copying files, updating any metadata files (like `packages.config`), and otherwise reacting to the installation. You can create a custom project by deriving from `NuGetProject`. In most simple cases, you will probably just instantiate `FolderNuGetProject` which represents a project based on a packages folder. Note that you can also derive a custom project implementation from `FolderNuGetProject` if you want packages to be installed into a folder but also want a little bit of extra control. For example, [Wyam](http://wyam.io) has a custom `FolderNuGetProject` implementation so that it can record information about every installed package such as whether it has a content folder.

# Settings

The package manager (discussed below) uses a `ISettings` object to provide some additional information about it's environment. In the default NuGet implementations, this interface basically provides the same information that's contained in the NuGet config file. There's a static factory method we can use to get an ISettings instance, though it does require us to implement `IMachineWideSettings` which typically provides information about the global NuGet configuration. A simple `IMachineWideSettings` implementation looks like this:

```
public class MachineWideSettings : IMachineWideSettings
{
    private readonly Lazy<IEnumerable<Settings>> _settings;

    public MachineWideSettings()
    {
        var baseDirectory = NuGetEnvironment.GetFolderPath(NuGetFolderPath.MachineWideConfigDirectory);
        _settings = new Lazy<IEnumerable<Settings>>(
            () => global::NuGet.Configuration.Settings.LoadMachineWideSettings(baseDirectory));
    }

    public IEnumerable<Settings> Settings => _settings.Value;
}
```

Then we can create our `ISettings` object like this:

```
string rootPath = ""; // The root path for your NuGet "project"
ISettings settings = Settings.LoadDefaultSettings(rootPath, null, new MachineWideSettings());
```

# Package Manager

The whole installation process is orchestrated by the `NuGetPackageManager` class. When you create one, you tell it about your source repositories (I.e., the NuGet gallery), your settings, and the project where the packages will be installed (all discussed above). Once you have an instance of the package manager, you can call `InstallPackageAsync(...)` to actually install packages (note that this method is awaitable, so you'll want to call it for every package that needs to be installed and then await all the returned tasks).

# Putting It All Together

As you can see, the installation process in the NuGet v3 APIs requires a lot of objects that have to work together. Given some of the definitions and implementations above, a full process might look something like the following (you may have to tweak it a little - this code is intended as an example to build on, not a drop-in package installation routine):

```
PackageIdentity identity = new PackageIdentity(...);
ISourceRepositoryProvider sourceRepositoryProvider = ...;  // See part 2
string rootPath = "...";
string packagesPath = "...";
ISettings settings = Settings.LoadDefaultSettings(rootPath, null, new MachineWideSettings());
NuGetProject project = new FolderNuGetProject(rootPath);
NuGetPackageManager packageManager = new NuGetPackageManager(sourceRepositoryProvider, settings, packagesPath)
{
    PackagesFolderNuGetProject = project
};
bool allowPrereleaseVersions = true;
bool allowUnlisted = false;
ResolutionContext resolutionContext = new ResolutionContext(
    DependencyBehavior.Lowest, allowPrereleaseVersions, allowUnlisted, VersionConstraints.None);    
INuGetProjectContext projectContext = new ProjectContext();
IEnumerable<SoureRepository> sourceRepositories = ...;  // See part 2
await packageManager.InstallPackageAsync(packageManager.PackagesFolderNuGetProject,
    identity, resolutionContext, projectContext, sourceRepositories,
    Array.Empty<SourceRepository>(),  // This is a list of secondary source respositories, probably empty
    CancellationToken.None);
```  

# Conclusion

Hopefully all three parts have given you enough to build on in order to integrate NuGet into your own applications. Integrated package management seems to me like it's an area with a lot of potential for customizing deployment and enabling advanced extensibility scenarios for many different types of application. That said, I don't often see a lot of literature regarding application-specific package management. I don't know if that's because it seems like overkill, it's too complicated, most applications just don't need that level of customization, or something else entirely. If you're looking into something like this, I'd be curious to hear why and for what purpose.