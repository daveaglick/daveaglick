Title: Running A Design-Time Build With MSBuild APIs
Lead: Getting source files, references, and build properties without invoking the compiler.
Published: 9/27/2017
Image: /images/excavators.jpg
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

The first step is opening our project with the MSBuild API. You'll need the [Microsoft.Build](https://www.nuget.org/packages/Microsoft.Build/) package which includes support for opening and evaluating MSBuild project files along with some other packages:

* [Microsoft.Build](https://www.nuget.org/packages/Microsoft.Build/)
* [Microsoft.Build.Framework](https://www.nuget.org/packages/Microsoft.Build.Framework/)
* [Microsoft.Build.Utilities.Core](https://www.nuget.org/packages/Microsoft.Build.Utilities.Core/)
* [Microsoft.Build.Tasks.Core](https://www.nuget.org/packages/Microsoft.Build.Tasks.Core/)

Once you've installed the packages, we would normally write this and be done:

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

It turns out [there's a bug](https://github.com/Microsoft/msbuild/issues/2369) with the 15.3 release of Visual Studio. The `ToolLocationHelper` can't find the tools with this version of Visual Studio so we have to help it out with a fallback. There's a package named [Microsoft.VisualStudio.Setup.Configuration.Interop](https://www.nuget.org/packages/Microsoft.VisualStudio.Setup.Configuration.Interop/) that can help get the Visual Studio installation location, but it's .NET Framework 3.5 only so it won't work in a `netstandard` library. As an alternative, we can poll for well known locations of Visual Studio (idea from the amazing [MSBuildStructuredLog](https://github.com/KirillOsenkov/MSBuildStructuredLog) project).

So to put it all together we end up with something that looks like this:

```
public Project GetProject(string projectPath)
{
    ProjectCollection projectCollection = new ProjectCollection();
    projectCollection.AddToolset(new Toolset(ToolLocationHelper.CurrentToolsVersion, GetToolsPath(), projectCollection, string.Empty));
    Project project = projectCollection.LoadProject(projectPath);
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
Confirm that the path in the <Import> declaration is correct, and that the file exists on disk.
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

Given the tools path, we can calculate the other paths like this (assuming we found the tools in a Visual Studio directory):

```
public Dictionary<string, string> GetGlobalProperties(string projectPath, string toolsPath)
{
    string solutionDir = Path.GetDirectoryName(projectPath);
    string extensionsPath = Path.GetFullPath(Path.Combine(toolsPath, @"..\..\"));
    string sdksPath = Path.Combine(extensionsPath, "Sdks");
    string roslynTargetsPath = Path.Combine(toolsPath, "Roslyn");

    return new Dictionary<string, string>
    {
        { "SolutionDir", solutionDir },
        { "MSBuildExtensionsPath", extensionsPath },
        { "MSBuildSDKsPath", sdksPath },
        { "RoslynTargetsPath", roslynTargetsPath }
    };
}
```

And now our `GetProject()` method looks like this after passing the global properties to the `ProjectCollection`:

```
public Project GetProject(string projectPath)
{
    string toolsPath = GetToolsPath();
    Dictionary<string, string> globalProperties = GetGlobalProperties(projectPath, toolsPath);
    ProjectCollection projectCollection = new ProjectCollection(globalProperties);
    projectCollection.AddToolset(new Toolset(ToolLocationHelper.CurrentToolsVersion, toolsPath, projectCollection, string.Empty));
    Project project = projectCollection.LoadProject(projectPath);
}
```

## SDK-Style Projects

Everything we've done so far has helped us get to the point where we can open a legacy MSBuild project, but we'll have a different problem if we try to open a new SDK-style MSBuild project:

```
The SDK 'Microsoft.NET.Sdk' specified could not be found.
```

That's because projects that use the new SDK-style builds have their targets and tasks located in a completely different location. For these types of projects the SDK is distributed separately [via .NET Core](https://www.microsoft.com/net/download/core). That means we need a new way to find the files. It turns out that the most reliable way I could find to do this is to shell out to `dotnet --info` (based on [code from OmniSharp](https://github.com/OmniSharp/omnisharp-roslyn/blob/78ccc8b4376c73da282a600ac6fb10fce8620b52/src/OmniSharp.Abstractions/Services/DotNetCliService.cs)):

```
public string GetCoreBasePath(string projectPath)
{
    // Ensure that we set the DOTNET_CLI_UI_LANGUAGE environment variable to "en-US" before
    // running 'dotnet --info'. Otherwise, we may get localized results.
    string originalCliLanguage = Environment.GetEnvironmentVariable(DOTNET_CLI_UI_LANGUAGE);
    Environment.SetEnvironmentVariable(DOTNET_CLI_UI_LANGUAGE, "en-US");

    try
    {
        // Create the process info
        ProcessStartInfo startInfo = new ProcessStartInfo("dotnet", "--info")
        {
            // global.json may change the version, so need to set working directory
            WorkingDirectory = Path.GetDirectoryName(projectPath),
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        // Execute the process
        using (Process process = Process.Start(startInfo))
        {
            List<string> lines = new List<string>();
            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    lines.Add(e.Data);
                }
            };
            process.BeginOutputReadLine();
            process.WaitForExit();
            return ParseCoreBasePath(lines);
        }
    }
    finally
    {
        Environment.SetEnvironmentVariable(DOTNET_CLI_UI_LANGUAGE, originalCliLanguage);
    }
}

public string ParseCoreBasePath(List<string> lines)
{
    if (lines == null || lines.Count == 0)
    {
        throw new Exception("Could not get results from `dotnet --info` call");
    }

    foreach (string line in lines)
    {
        int colonIndex = line.IndexOf(':');
        if (colonIndex >= 0 
            && line.Substring(0, colonIndex).Trim().Equals("Base Path", StringComparison.OrdinalIgnoreCase))
        {
            return line.Substring(colonIndex + 1).Trim();
        }
    }

    throw new Exception("Could not locate base path in `dotnet --info` results");
}
```

Once you have the base path for .NET Core, you can set the global properties like this:

```
public Dictionary<string, string> GetCoreGlobalProperties(string projectPath, string toolsPath)
{
    string solutionDir = Path.GetDirectoryName(projectPath);
    string extensionsPath = toolsPath;
    string sdksPath = Path.Combine(toolsPath, "Sdks");
    string roslynTargetsPath = Path.Combine(toolsPath, "Roslyn");

    return new Dictionary<string, string>
    {
        { "SolutionDir", solutionDir },
        { "MSBuildExtensionsPath", extensionsPath },
        { "MSBuildSDKsPath", sdksPath },
        { "RoslynTargetsPath", roslynTargetsPath }
    };
}
```

Which means our project loading code looks like this for SDK-style projects:

```
public Project GetCoreProject(string projectPath)
{
    string toolsPath = GetCoreBasePath();
    Dictionary<string, string> globalProperties = GetGlobalProperties(projectPath, toolsPath);
    ProjectCollection projectCollection = new ProjectCollection(globalProperties);
    projectCollection.AddToolset(new Toolset(ToolLocationHelper.CurrentToolsVersion, toolsPath, projectCollection, string.Empty));
    Project project = projectCollection.LoadProject(projectPath);
}
```

### But Wait (Again)!

At this point you'll still be getting that same exception about not being able to locate the SDK. It turns out there's a strange behavior where MSBuild looks at environment variables to find the SDK. I haven't been able to track down exactly why this is, but trust me on this one. You'll need to add the following before loading the `Project` to get SDK-style projects to load (we'll get them from the global properties we already set):

```
Environment.SetEnvironmentVariable(
    "MSBuildExtensionsPath",
    globalProperties["MSBuildExtensionsPath"]);
Environment.SetEnvironmentVariable(
    "MSBuildSDKsPath",
    globalProperties["MSBuildSDKsPath"]);
```

### The Runtime Matters

Note that you'll *also* need to use the .NET Core SDK if you're running MSBuild from a .NET Core runtime, even if the project you're compiling targets .NET Framework. I'm not entirely sure why, but when running MSBuild APIs from a .NET Core runtime some different targets are used that don't exist in the Visual Studio .NET Framework build chain.

## Putting It All Together

At this point we can open both legacy MSBuild projects and those using the new .NET Core SDK. The final bit of code we need is to distinguish between the two. This is relatively easy: MSBuild projects are XML and the root `<Project>` element will contain an `Sdk` attribute if it's a SDK-style project and won't if it's not:

```
bool sdkStyle = false;
using (XmlReader reader = XmlReader.Create(projectPath))
{
    if (reader.MoveToContent() == XmlNodeType.Element && reader.HasAttributes)
    {
        if (reader.MoveToAttribute("Sdk"))
        {
            sdkStyle = true;
        }
    }    
}
```

Note that even though we're reading the project as XML to determine if it uses the `Sdk` attribute, you still need to load it by file name into the MSBuild `Project` even though the `ProjectCollection.LoadProject()` method accepts an `XmlReader`. This is because loading it by file name sets some additional properties that MSBuild requires to properly locate targets files. If you loaded the XML directly, you'd need to identify and set all of those global properties yourself.

# Configuring A Design-Time Build

Now that we can finally open any type of MSBuild project, let's figure out how to execute a [design-time build](https://github.com/dotnet/project-system/blob/master/docs/design-time-builds.md). Recall from the introduction that this special type of build will give us information about the project without actually compiling it. It turns out that triggering a design-time build is really easy compared to getting MSBuild to work. All it takes is a few extra global properties:

* `DesignTimeBuild` triggers the alternate targets for a design-time build.
* `BuildProjectReferences` tells MSBuild to ignore other project references.
* `SkipCompilerExecution` tells MSBuild not to invoke the compiler.
* `ProvideCommandLineArgs` outputs the command line that would have get sent to the compiler, which we can use to extract the final computed source files, references, and build properties.

For example, you can set these by adding the following after using the code earlier to get the global properties for the type of project:

```
// ...
Dictionary<string, string> globalProperties = GetGlobalProperties(projectPath, toolsPath);
globalProperties["DesignTimeBuild"] = "true";
globalProperties["BuildProjectReferences"] = "false";
globalProperties["SkipCompilerExecution"] = "true";
globalProperties["ProvideCommandLineArgs"] = "true";
// ...
```

# Getting The Data

## Compiling The Project

So far we've just been configuring a build and telling MSBuild how to evaluate the project file but we actually have to run the build to get our output. You can't run a build directly on the MSBuild `Project` instance, instead you have to create an independent `ProjectInstance` object and execute the build on that:

```
ProjectInstance projectInstance = project.CreateProjectInstance();
if (!projectInstance.Build("Compile", null))
{
    throw new Exception("Could not compile project");
}
```

## But Wait (Yes, Again)!

At this point if you try and run the build, it might succeed. Or it might fail. There are a lot of moving parts involved and it's worth taking a step back and considering what we've actually rigged up here. We're consuming MSBuild APIs we got off of NuGet and telling them to look for support libraries, including the actual MSBuild runtime, on the file system somewhere. Included in the stuff we're probably loading from the file system are other libraries that contain the compiled task code, support functions, etc. And on top of that you're own application may be running on different runtimes such as .NET Framework or .NET Core. This is a recipe for assembly loading nightmares. Here are some tips to make this process more universal. Note that you may need to do all of these things or none of them depending on your application runtime, the target of the project you're trying to compile, and the MSBuild artifacts available in the file system.

The first this we'll need to do is be sure your host application does not have the "Prefer 32-bit" flag checked. That's because many of the MSBuild artifacts on disk may be compiled for 64-bit (especially those in the .NET Core SDK). This ensures your application will be able to load them.

We'll also want to make sure we're loading *our* version of the core tasks library that matches the MSBuild API we've been using and not the version from disk. We've already added the NuGet package (it was one of the ones I listed in the beginning), but we haven't referenced anything in it yet so the .NET runtime won't load it into memory for us. To cause it to load into memory, add the following before calling `.Build()`:

```
// Instantiate a task from the core tasks library
Copy copy = new Copy();
```

I also ran into trouble in some situations with finding and loading the Roslyn tasks assembly. I was able to get around this by adding the following:

```
Assembly.LoadFile(Path.GetFullPath(Path.Combine(globalProperties["RoslynTargetsPath"], "Microsoft.Build.Tasks.CodeAnalysis.dll")));
```

Finally, I hit some issues with the availability and binding of certain `netstandard` packages when using the MSBuild APIs from a .NET Framework console application. You may or may not need to add the [System.IO.FileSystem](https://www.nuget.org/packages/System.IO.FileSystem/) package and the following binding redirects:

```
<dependentAssembly>
    <assemblyIdentity name="Microsoft.Build.Framework" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
    <bindingRedirect oldVersion="0.0.0.0-99.9.9.9" newVersion="15.1.0.0" />
</dependentAssembly>
<dependentAssembly>
    <assemblyIdentity name="System.IO.FileSystem" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
    <bindingRedirect oldVersion="0.0.0.0-4.0.3.0" newVersion="4.0.3.0" />
</dependentAssembly>
```

## Troubleshooting

At this point hopefully you'll be able to build successfully, but if not you may need to attach a logger:

```
StringBuilder logBuilder = new StringBuilder();
ConsoleLogger logger = new ConsoleLogger(LoggerVerbosity.Normal, x => logBuilder.Append(x), null, null);
projectCollection.RegisterLogger(logger);
// ...
ProjectInstance projectInstance = Project.CreateProjectInstance();
if (!projectInstance.Build("Compile", new ILogger[] { logger }))
{
    Console.WriteLine(logBuilder);  // Or however you want to log
    throw new Exception("Could not compile project");
}
```

If something goes wrong, the log output should give you some clues.

## Getting The CscCommandLineArgs

It's been a long process, but we're almost to the part where we can extract the source files, references, and build properties from the `CscCommandLineArgs` build item. When the build is complete, we'll end up with multiple entries in `ProjectInstance.Items` with an `ItemType` of `CscCommandLineArgs`. Each one represents another argument to the complier, the value of which is stored in the `EvaluatedInclude` property. Those arguments that don't start with a slash are our source files, relative to the project path. Those that do start with a slash could be references or other properties. For example, all references start with `/reference:`.

```
List<string> sourceFiles = new List<string>();
List<string> references = new List<string>();
foreach(ProjectItemInstance commandLineArg in projectInstance.Items.Where(x => x.ItemType == "CscCommandLineArgs"))
{
    if (!commandLineArg.EvaluatedInclude.StartsWith("/"))
    {
        sourceFiles.Add(
            Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(projectPath),
                    commandLineArg.EvaluatedInclude)));
    }
    else if (commandLineArg.EvaluatedInclude.StartsWith("/reference:"))
    {
        references.Add(commandLineArg.EvaluatedInclude.Substring(11));
    }
}
```

This may raise the question, "why would I want this data?" It's valuable anytime you need to process a project file since the source files and references are really the main concern of MSBuild. Almost everything else in a MSBuild project is just there to get the right answers for these two things. I plan on using them to enhance [Scripty](https://github.com/daveaglick/Scripty) and allow it to work within any host project as well as add support for SDK-style projects to [Wyam](https://wyam.io) when generating documentation. They could also be used to construct a Roslyn [AdhocWorkspace](https://channel9.msdn.com/Blogs/MVP-Windows-Dev/Learn-Roslyn-Now-E08-The-AdHocWorkspace).

# Introducing Buildalyzer

If you've made it all the way to the end, congratulations. I hope you learned a lot. And now I'm going to tell you that you actually didn't need to read or understand any of that because I've written a little open source utility library called [Buildalyzer](https://github.com/daveaglick/Buildalyzer) ([NuGet package here](https://www.nuget.org/packages/Buildalyzer)) that does this exact thing:

```
Analyzer analyzer = new Analyzer();
IReadOnlyList<string> sourceFiles = analyzer.GetProject(@"C:\MyCode\MyProject.csproj").GetSourceFiles();
```

The project could probably use a little polish, which I'll add as I go, but it should be usable and working. Let me know if you hit any bugs or it doesn't work for some combination of runtime and project. It works on my machine and has tests for different combinations, but as you can see this is a complex process. Enjoy - pull requests accepted :).

