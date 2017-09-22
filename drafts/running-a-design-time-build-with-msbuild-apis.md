Title: Running A Design-Time Build With MSBuild APIs
Lead: Getting source files, references, and build properties without invoking the compiler.
Published: 9/22/2017
Image: /images/rollercoaster.jpg
Tags:
- MSBuild
---
The MSBuild APIs are now [on NuGet](https://www.nuget.org/packages?q=microsoft.build) and target `netstandard` which is awesome, but unfortunately they won't work out of the box with certain [platform and project type combinations](https://github.com/dotnet/docfx/issues/1752). The reasons are complex, but basically come down to MSBuild [not being a self contained project system](https://github.com/dotnet/project-system#what-is-a-project-system). In order for the MSBuild APIs to work with certain project types like SDK-style, MSBuild needs to be told where to find the extra stuff that it needs (like the SDK targets) before it can process the project file. Additionally, if all you care about is metadata like the resolved source files, references, and project properties then you don't actually have to run the compiler. Once you can open the project in MSBuild you can tell it to perform a [design-time build](https://github.com/dotnet/project-system/blob/master/docs/design-time-builds.md) which gives you this information without the performance hit of actually compiling the project. Doing so can be valuable for a number of different use cases including [code generation](https://github.com/daveaglick/Scripty), [documentation engines](https://wyam.io), and [build orchestration](https://cakebuild.net). This post explains some tricks for getting MSBuild to work for any project and then using it to get project metadata without triggering a compilation.

# Some Background

Before we dive into how to use the MSBuild APIs, let's take a quick step back and review (or introduce) some MSBuild concepts.

* Project files such as `.csproj` are actually MSBuild files that tell MSBuild how to build your project by defining targets, tasks, and all the other associated activity that goes into the build process.
* MSBuild is not a declarative language. That means that you have to evaluate an MSBuild file to determine what it means. You can't just look at the file and infer what source files are involved in build, where the references are located, etc.
* An MSBuild *task* is a reusable build action, often implemented in code, that MSBuild can execute in the process of evaluating an MSBuild file.
* An MSBuild *target* is a collection of tasks (often just 1 but can be many more) designed to perform a specific function within the build. Reusable targets are often defined in a `.targets` file. This is partly why MSBuild needs to know where the specific .NET SDK is located for new SDK-style projects: the SDK contains `.targets` files with the targets and tasks for the new project system.
* There are many different kinds of MSBuild project files. I haven't been able to locate an exhaustive list, but the most common are `.csproj` (for C# projects) and `.vbproj` (for Visual Basic). There are also different varieties of these project files. For example, a C# `.csproj` for the new .NET build system will have an `SDK` attribute and uses different default tasks and targets to simplify the project file structure (often referred to as a "SDK-style projects").
* The MSBuild APIs are .NET libraries that allow you to open, evaluate, and otherwise work with MSBuild files, tasks, and targets from within your own code.

# Opening With MSBuild

The first step is opening our project with the MSBuild API. You'll need the [Microsoft.Build](https://www.nuget.org/packages/Microsoft.Build/) package which includes support for opening and evaluating MSBuild project files. Once you've installed the package, we would normally write this and be done:

```
Project project = new Project(@"C:\Code\MyProject.csproj");
```

## Finding The Tools

Here's where things start to get tricky though. If you're trying to open a .NET Framework project that defines `ToolsVersion="15.0"` (in other words, Visual Studio 2017) you'll probably get an exception like this:

```
The tools version "15.0" is unrecognized. Available tools versions are "12.0", "14.0", "2.0", "3.5", "4.0".
```

What that means is that the MSBuild libraries in the Microsoft.Build package don't have any built-in knowledge of Visual Studio 2017 and where to find `msbuild.exe`. We have to help them out for these types of projects. Thankfully there's already some utilities to solve this particular problem. Install the [Microsoft.Build.Utilties.Core](https://www.nuget.org/packages/Microsoft.Build.Utilities.Core/) package and then use this code:

```
string toolsPath = ToolLocationHelper.GetPathToBuildToolsFile("msbuild.exe", ToolLocationHelper.CurrentToolsVersion);
ProjectCollection projectCollection = new ProjectCollection();
projectCollection.AddToolset(new Toolset(ToolLocationHelper.CurrentToolsVersion, Path.GetDirectoryName(toolsPath), projectCollection, string.Empty));
Project project = projectCollection.LoadProject(@"C:\Code\MyProject.csproj");
```

Notice that we're now creating a `ProjectCollection` and using that to get the project. That's because you can only set tool resolution information at the `ProjectCollection` level. We were actually always creating a `ProjectCollection`, it's just that the `Project` constructor we used earlier creates a default global one for us.

### But Wait!

It turns out [there's a bug](https://github.com/Microsoft/msbuild/issues/2369) with the 15.3 release of Visual Studio. The `ToolLocationHelper` can't find the tools with this version of Visual Studio so we have to help it out with a fallback. There's a package named [Microsoft.VisualStudio.Setup.Configuration.Interop](https://www.nuget.org/packages/Microsoft.VisualStudio.Setup.Configuration.Interop/) that can help get the Visual Studio installation location, but it's .NET Framework 3.5 only so it won't work in a `netstandard` library. As an alternative, we can poll for well known locations of Visual Studio (this was taken from the amazing [MSBuildStructuredLog](https://github.com/KirillOsenkov/MSBuildStructuredLog) project):

```
var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

return new[]
{
    Path.Combine(programFilesX86, @"Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"),
    Path.Combine(programFilesX86, @"Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\amd64\MSBuild.exe"),
    Path.Combine(programFilesX86, @"Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"),
    Path.Combine(programFilesX86, @"Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\amd64\MSBuild.exe"),
    Path.Combine(programFilesX86, @"Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"),
    Path.Combine(programFilesX86, @"Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\amd64\MSBuild.exe"),
    Path.Combine(programFilesX86, @"MSBuild\14.0\Bin\MSBuild.exe"),
    Path.Combine(programFilesX86, @"MSBuild\14.0\Bin\amd64\MSBuild.exe"),
    Path.Combine(programFilesX86, @"MSBuild\12.0\Bin\MSBuild.exe"),
    Path.Combine(programFilesX86, @"MSBuild\12.0\Bin\amd64\MSBuild.exe"),
    Path.Combine(windows, @"Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"),
    Path.Combine(windows, @"Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"),
}.Where(File.Exists).ToArray();
```

So to put it all together we end up with something that looks like this:

```
public Project GetProject(string fileName)
{
    ProjectCollection projectCollection = new ProjectCollection();
    projectCollection.AddToolset(new Toolset(ToolLocationHelper.CurrentToolsVersion, GetToolsPath(), projectCollection, string.Empty));
    Project = projectCollection.LoadProject(fileName);
}

public static string GetToolsPath()
{
    string toolsPath = ToolLocationHelper.GetPathToBuildToolsFile("msbuild.exe", ToolLocationHelper.CurrentToolsVersion);
    if (string.IsNullOrEmpty(toolsPath))
    {
        toolsPath = PollForToolsPath().FirstOrDefault();
    }
    if (string.IsNullOrEmpty(toolsPath))
    {
        throw new Exception("Could not locate the tools (MSBuild) path.");
    }
    return Path.GetDirectoryName(toolsPath);
}

public static string[] PollForToolsPath()
{
    var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
    var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

    return new[]
    {
        Path.Combine(programFilesX86, @"Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"),
        Path.Combine(programFilesX86, @"Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"),
        Path.Combine(programFilesX86, @"Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"),
        Path.Combine(programFilesX86, @"MSBuild\14.0\Bin\MSBuild.exe"),
        Path.Combine(programFilesX86, @"MSBuild\12.0\Bin\MSBuild.exe")
    }.Where(File.Exists).ToArray();
}
```

## Setting Global Properties

Now we're getting somewhere. The MSBuild libraries can open the project, but are failing on import:

```
The imported project "C:\Microsoft.CSharp.Core.targets" was not found.
Confirm that the path in the <Import> declaration is correct, and that
the file exists on disk.
C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\Microsoft.CSharp.CurrentVersion.targets
```

That's because we need to define a bunch of global MSBuild properties that the host (such as Visual Studio) would normally provide. For example, this first error happens because the initial target `Microsoft.CSharp.CurrentVersion.targets` (which we know how to get to from the tools path we calculated earlier) contains the following definition:

```
<CSharpCoreTargetsPath Condition="'$(CSharpCoreTargetsPath)' == ''">$(RoslynTargetsPath)\Microsoft.CSharp.Core.targets</CSharpCoreTargetsPath>
```

Notice how it's using the property `RoslynTargetsPath` to locate the `Microsoft.CSharp.Core.targets` targets file? That's one of a handful of MSBuild global properties we need to provide as the "host":

* `SolutionDir`
* `MSBuildExtensionsPath`
* `MSBuildSDKsPath`
* `RoslynTargetsPath`

...

## SDK-Style Projects

Everything we've done so far has helped us get to the point where we can open a legacy MSBuild project, but we'll have a different problem if we try to open a new SDK-style MSBuild project:

```
```

# Configuring A Design-Time Build

# Getting The Data