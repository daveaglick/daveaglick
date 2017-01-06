Title: Roslyn-Based DSLs vs. Standard C# Scripts
Lead: Examining the advantages of each and when you'd want to use them.
Published: 1/6/2017
Image: /images/antlers.jpg
Tags:
  - Roslyn
  - configuration
  - DSL
  - scripting
---
One of the many benefits of the Roslyn compiler-as-a-platform approach is that we can use it within our own applications to enable interesting scenarios like code-based configuration or scriptable behaviors. Roslyn provides several facilities for making this possible including a compilation API, access to syntax and semantic information, and a dedicated scripting API. In addition, Roslyn also powers the execution of C# scripts (typically ending in the `.csx` extension) by providing a script runner executable that's basically a thin wrapper around the scripting API I just mentioned. This gives developers many different options for how to introduce the power of code-driven functionality to their codebase. This post takes a look at two such options and why you might want to use one over the other. We'll also consider some of the fundamental reasons why there are tradeoffs at all and what could be done to improve the situation.

This particular topic has been on my mind a lot in the last year as I worked on [Wyam](https://wyam.io) which uses Roslyn to provide a special DSL (essentially a purpose-specific superset of C#) for configuration. I've also been working on other projects with heavy in-application Roslyn usage such as [Scripty](https://github.com/daveaglick/Scripty), which uses Roslyn for compile-time code generation and [Cake](http://cakebuild.net/) which uses Roslyn to drive the build script. I also had many excellent conversations at the MVP Summit in the fall regarding this and related issues. Then this conversation happened on Twitter:

<blockquote class="twitter-tweet" data-conversation="none" data-cards="hidden" data-partner="tweetdeck"><p lang="en" dir="ltr"><a href="https://twitter.com/mholo65">@mholo65</a> <a href="https://twitter.com/gep13">@gep13</a> <a href="https://twitter.com/cakebuildnet">@cakebuildnet</a> <a href="https://twitter.com/omnisharp">@omnisharp</a> would be very easy if it moved away from a custom globals/host object to standard scripting</p>&mdash; Filip W (@filip_woj) <a href="https://twitter.com/filip_woj/status/816253671335923712">January 3, 2017</a></blockquote>
<script async src="//platform.twitter.com/widgets.js" charset="utf-8"></script>

I greatly respect and admire all of the folks involved in the Twitter discussion and can see both perspectives being discussed. That's why I'm not going to pick one approach over the other as "the right way" (nevermind that there's never "the right way" in development anyway). But first, before we get into the details of this discussion, let's back up a bit.

# Hosting

The fundamental question at the heart of this post is how to set up your Roslyn environment in order to *host* some code. *Host* in this case means compile, evaluate, provide state (in both directions), and otherwise support some arbitrary block of source that your application needs to consume (we'll call this the *script*, not to be confused with Roslyn *scripting* which we'll get to in a bit). The core of the host is what sorts of "extras" your application is going to provide to the script. This usually constitutes things like global properties and methods (which aren't actually global as we'll see in a minute), NuGet packages and assemblies, preprocessor directives, and so on.

For illustration, let's examine how a script host can provide "global" properties and methods to the script. Assume your application can process scripts that look like this:

```
// Make sure I'm in a car or truck or something
if(WheelCount < 4)
{
  SetWheels(4);
}
```

Where do the property `WheelCount` and the method `SetWheels()` come from? This is where the host comes in. Script code is rarely executed in isolation, usually most hosts will wrap the code inside a *host object* that includes any "globals" as part of the object. By the time our host process this script and prepares it for evaluation, the complete set of code to evaluate may look something like:

```
public class Host
{
  private readonly HostContext _context;

  public Host(HostContext context)
  {
    _context = context;
  }

  public int WheelCount => _context.WheelCount;

  public void SetWheels(int wheels) => _context.WheelCount = wheels;

  public void Run()
  {
    // Here's where the actual script goes
    // Make sure I'm in a car or truck or something
    if(WheelCount < 4)
    {
      SetWheels(4);
    }
  }
}
```

Then when it comes time for the host to evaluate the script, an instance of this `Host` class is created, passing the context for the evaluation, and the `Run()` method is called.

So back to our original purpose of this post. There are essentially two primary mechanisms in Roslyn to take a script, generate and compile the host object, and manage it's evaluation.

# Compilation APIs (And Roslyn-Based DSLs)

The first approach is to use the Roslyn compilation APIs directly. In this approach you would literally form a string with the code that constitutes the host class (inserting the underlying script where necessary). Then you use Roslyn APIs to compile it, generate an in-memory assembly, use reflection to instantiate the compiled host object, and then evaluate it yourself.

This approach provides a lot of control because you can influence how it works at any stage in the process. Want some exotic preprocessor directives in the script? No problem, just do some simple string searching and manipulation before creating your host object source code and telling Roslyn to compile it. Want to provide some non-standard language features? Once again, just manipulate the script string before doing anything with Roslyn. You can build entire non-standard DSLs this way as long as the eventual string you pass to Roslyn for compilation is valid C# (or VB).

When would you want to use this over the native Roslyn scripting capabilities discussed below?
- When you want complete control over the host object.
- When you want to expose arbitrary "global" (from the perspective of the script) properties and methods.
- When you want to support custom preprocessor directives (for example, loading NuGet packages).
- When you want to manipulate the input script string, opening the door to non-standard special syntax, etc.
- When you want to run syntactic or semantic analysis on the script using Roslyn APIs prior to evaluation.
- When your host is part of a larger bootstrapping operation that requires support for non-script related functionality (like processing command line arguments).

# Scripting APIs

Roslyn also provides a set of [scripting APIs](https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples) which essentially wrap the compilation APIs into a standard set of functionality. They can be thought of as a particular implementation of the compilation APIs discussed above. This is what the Roslyn scripting executable uses to evaluate `.csx` script files and Visual Studio and other tooling knows how to operate with such scripts because the behavior is well-defined. Evaluating a script with the scripting APIs also provides some additional functionality like the `#a` preprocessor directive to load assemblies and the ability to "lift" code like static methods or class definitions out of the host object. The tradeoff for this standard set of script functionality is control. It's no longer advisable to manipulate the underlying script code (such as you might for a DSL) because that would break the intrinsic interoperability that you get from using the standard scripting convention. You also can't create the code for the host object because that's handled by the scripting functionality in Roslyn directly. I.e., no more arbitrary global properties and methods.

I want to focus on that last point for just a minute because the scripting APIs actually *do* allow you to specify a global object. There's a big gotcha here though. If you decide to use a global object, that will only ever work when the script is evaluated by your application. If you try and evaluate your otherwise standard C# script in the Roslyn script runner, Visual Studio, or elsewhere, you'll get compilation errors because those script runners don't know anything about the globals you're expecting to be available. That greatly reduces the promise of write-anywhere scripts that the Roslyn scripting capability supports. More on this at the end of the post.

One final point is that there is a way to "get back" some of the advantages of using the compilation APIs, specifically global properties and methods. An effective pattern for making this work inside a standard script is to place anything that you would consider global inside a static object. That way, all the script has to do is reference the assembly that contains the global object (using the Roslyn script `#a` preprocessor directive) and the "global" state can be accessed through static object(s). For illustration, the previous example that used a custom host and the compilation APIs might look something like this if it were a standard C# script:

```
#a "CarLib.dll"

using CarLib;

// Make sure I'm in a car or truck or something
if(Car.WheelCount < 4)
{
  Car.SetWheels(4);
}
```

So when does this approach make more sense over writing your own host?
- When you want an easier editing experience by providing Intellisense everywhere.
- When you want to be able to evaluate your script anywhere using the standard Roslyn C# script runner or an alternate runner like [dotnet-script](https://github.com/filipw/dotnet-script) or [scriptcs](https://github.com/scriptcs/scriptcs).
- When you don't want to deal with the complexities of writing your own host and want Roslyn to handle things like referencing assemblies for your script.

# Some Examples

Perhaps the distinction between the two approaches will become clearer with some concrete examples from my own projects.

[Wyam](https://wyam.io) can be [configured using a C# script](https://wyam.io/docs/usage/configuration). In this case, I elected very early on to write a custom host instead of use the scripting APIs. I did consider the decision, but went the way I did for several reasons. Wyam has a number of special preprocessor directives that need to be process *before* the script itself is evaluated. This includes functionality the the standard Roslyn script host would provide like assembly referencing, but also some extra functionality like loading and referencing NuGet packages (more on this in the final section). The Wyam configuration file also does some tricky rewriting of the syntax tree before evaluation to enable special non-C# syntax that makes the scripts a little easier to write for non-developers. In this way, the Wyam configuration scripts are actually a DSL superset of C# and not actually standard C#. The Roslyn scripting APIs would never be able to compile them and would just emit compile errors. On the other hand, I've gotten feedback that the Wyam configuration scripts can be hard to write in the absence of tooling like Intellisense. Of course I could always give up some of this extra functionality to make the configuration scripts adhere to the Roslyn C# script standard, but so far those bits of functionality are more helpful than Intellisense would be (IMO).

Another project I work on is [Scripty](https://github.com/daveaglick/Scripty). This one is a little different in that the whole goal of the project is to support Roslyn scripts. However, I also elected to use a custom host for this project. My original thinking was that it would be easier to manage the many "global" objects if I could control the host object. However, unlike the feedback for Wyam regarding Intellisense which has mostly been of the "wouldn't it be nice" variety, the feedback for this tool as been "we need Intellisense". Rather than attempt to create special tooling just for this project, it may make sense to transition to using the Roslyn scripting APIs instead of a custom host and provide "global" state via static objects.

# Bridging The Gap

In my opinion, the biggest tradeoff comes down to more control with the compilation APIs and a custom host vs. write-anywhere standards compliance with the scripting APIs. But let's consider for a moment some of the things that could potentially be done to make the standard scripting APIs more supporting of customization. Note that I'm just throwing out ideas at this point. None of this has been formally suggested to the Roslyn team (at least by me) and many of these suggestions are probably infeasible for one reason or another.

One of the main reasons folks seem to write their own host is to gain NuGet support. That is, they want a preprocessor directive like `#n` to specify a NuGet package, install it, and reference the contained assemblies before compiling and evaluating the script. An important point is that this has to happen *before* compiling the script since the compilation will fail if the expected packages and assemblies aren't referenced as part of the compilation. Therefore, using some sort of static object *inside* the script to gather NuGet packages and install them won't work. The obvious solution is to add some sort of NuGet directive to the standard scripting APIs similar to the existing `#a` and `#load` directives. My understanding is that there's an intent to do just this, but it hasn't been a high priority yet. The sooner the better though, because many custom hosts are starting to define their own often totally unique syntax for this type of directive. Unifying NuGet loading syntax in C# scripts is important for the future success of C# and .NET as a scripting language and platform.

Another way the standard script support could be made more flexible is if you could define either a global object or a host object directly in the script. Consider that when you write a standard C# script you can either evaluate it using the application Roslyn provides for this purpose (or one of the other applications mentioned above that can evaluate scripts) or you can use the Roslyn scripting APIs to evaluate the script yourself within your own application. The latter is much simpler than using the compilation APIs to design an entire host, but does provide some limited customization including the ability to specify a global object. This means that running a standard Roslyn script with the scripting APIs through the Roslyn script runner or using tooling like Visual Studio wouldn't know anything about the global objects you use for the script when you evaluate it with the scripting APIs. Thus, no Intellisense and possible compilation errors, even if sticking to standard C# scripts. This could be mitigated if the script itself could provide a hint about what kind of global object it expects. Something like a `#global` directive. Then, external script runners and tooling could just create a default instance of the global object type for use in compilation and Intellisense while the real application that's intended to evaluate the script could pass the actual instance of the global object to the scripting APIs. This would allow the script to use global objects consistently in both tooling and your application.

Going even further, perhaps the script could reference a particular implementation of a well-defined interface that could control a large portion of the scripting experience. Then the tooling and standard script runners could be even smarter about script customization when evaluating or working with such scripts. A good example is if the well-defined interface contained a handler for unknown preprocessor directives. That would allow the Roslyn scripting APIs (and thus the third-party script runners that use it) to instantiate the host interface and use it during compilation and evaluation all the time. Doing so would eliminate many of the reasons for writing a custom host in the first place.

# Conclusion

There's no doubt that Roslyn enables some very exciting scenarios for introducing scripting to your own applications. The choice between setting up your own host or using the host that the scripting APIs provide can be tricky, with tradeoffs in both directions. It's a complex topic and hopefully I've done enough to explain the background and why this is even something you need to think about. This is an issue I care about given that so many of my own projects use Roslyn in one way or another, and I'm happy to discuss it further in the comments.