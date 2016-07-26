Title: Announcing Scripty
Lead: An alternative to T4 for compile-time code generation using the power of Roslyn scripting
Image: images/hero.jpg
Published: 6/7/2016
Tags:
  - open source
  - Roslyn
  - T4
  - scripting
---
I've written a lot of T4 templates, and while they work well enough for compile-time code generation, they're never much fun to write. Recently however I've noticed an even bigger problem with T4 templates now that Visual Studio is becoming less and less a required part of the build process (more on this in a minute). Thankfully, the Roslyn team has done an excellent job of packaging the Roslyn compiler into an easy to consume scripting package. By combining that scripting support with some Visual Studio extensibility, we can provide a code generation alternative that relies on Roslyn scripts written in plain old C# (VB.NET script support coming soon).

# Why Not T4?

While T4 works well enough, the syntax isn't very elegant (at least to me). It's also a *templating* language first and foremost, which means that what you write is what gets output and you have to escape the template in order to write code or control execution. This model is fine for things like web pages, but I much prefer to do code generation *in code* and output what I want to be generated using normal code statements. This is obviously personal preference, but I've heard from many other people who feel the same.

Personal preference aside, a bigger problem is that T4 is hard to use outside Visual Studio. There really isn't an official external engine for T4 templates (there are tips for how to tap into the one Visual Studio uses, but yech). The Mono project [has also written one](https://www.nuget.org/packages/Mono.TextTemplating/) for use in MonoDevelop, but it has it's own issues. Most importantly, many T4 scripts rely on something called the DTE object model to tap into information about the project(s) and solution it belongs to. DTE is tightly coupled to Visual Studio and isn't available outside Visual Studio. To put it another way: if you have a T4 template that uses DTE (as many do), it's impossible to run in outside Visual Studio. Let that sink in. No scripted builds, no continuous integration, no Visual Studio Code, etc.

# Scripty As An Alternative

The goal of Scripty is to provide a code generation capability that anyone case use easily by leveraging the languages they're already using. Additionally, the hope is that it can provide an alternative to T4 in any situation where T4 templates are being used today. Here's what a Scripty script looks like:

```
foreach(Document document in Project.Documents)
{
    Output.WriteLine($"// {document.FilePath}");
}
```

This will output a comment for each of the files in your project. Syntax look familar? That's because it's just plain old C#. The `Document` and `Project` classes are from the Roslyn project model, the same one you use if you're building Roslyn-based code analysis tools. Scripty scripts are just [standard Roslyn C# scripts](https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples) with some special global properties to make them suitable for powering code generation. All the standard C# scripting conventions still apply such as using the `#r` preprocessor directive to load assemblies and the  `#load` directive to load external script files. They are also generally given `.csx` extensions just like normal C# scripts.

<img src="/posts/images/scripty.gif" alt="Scripty" class="img-responsive" style="margin-top: 6px; margin-bottom: 6px;">

# How To Get It

The easiest way to get up and running is to install the [Scripty.MsBuild NuGet package](https://www.nuget.org/packages/Scripty.MsBuild/) into an existing project. Then just create a script file with a `.csx` extension, add it to your project, and watch the magic happen on your next build. Alternatively, you can install the [Scripty Visual Studio extension](https://visualstudiogallery.msdn.microsoft.com/52c02bb2-1d79-476e-82fb-5abfbfe6b3e4) to add custom tool support for code generation outside the build process. It's also possible to run Scripty on the command line with the [Scripty package](https://www.nuget.org/packages/Scripty/) or to embed the scripty engine with the [Scripty.Core package](https://www.nuget.org/packages/Scripty.Core/), though these are more advanced scenarios.

More information, documentation, and source code is available at the GitHub repository: [https://github.com/daveaglick/Scripty](https://github.com/daveaglick/Scripty).

If you need help, have a feature request, or notice a bug (bugs?! what bugs?!), just submit a GitHub issue.