Title: MSBuild Loggers And Logging Events
Lead: How to easily write cross platform MSBuild loggers.
Published: 10/4/2018
Image: /images/journal.jpg
Tags:
  - msbuild
---
I recently learned all about how MSBuild logging works and was surprised at how powerful it is. I was also disappointed how little information there is on the topic (though [the docs](https://docs.microsoft.com/en-us/visualstudio/msbuild/logging-in-msbuild) are quite good). In this post I'll discuss what MSBuild logging is and how you can write your own cross-platform logger that can be plugged into any build process.

# Logging Events

When MSBuild executes it emits a sequence of events that describe the current phase and provide a whole bunch of information about it. This includes things like starting a task or target, raising a message, and warning and error output. [The current set of individual events](https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.framework.ieventsource#events) is:

* `BuildFinished`
* `BuildStarted`
* `CustomEventRaised`
* `ErrorRaised`
* `MessageRaised`
* `ProjectFinished`
* `ProjectStarted`
* `StatusEventRaised`
* `TargetFinished`
* `TargetStarted`
* `TaskFinished`
* `TaskStarted`
* `WarningRaised`

Don't let that relatively sparse set of events lead you to think there isn't much data to be had. Each one of these events is raised with it's own arguments, which can get quite large. For example, the `TargetStarted` event passes a [`TargetStartedEventArgs`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.framework.targetstartedeventargs) class that includes:

* `BuildEventContext` with lots of data about the target location
* `Message`
* `ParentTarget`
* `ProjectFile`
* `TargetFile`
* `TargetName`

Writing a logger is all about responding to these events in different ways. In fact, the console output that you're used to seeing from MSBuild is actually generated from a [normal logger](https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.logging.consolelogger) that converts these logging events into meaningful strings.

# Writing A Logger

[To create your own logger](https://docs.microsoft.com/en-us/visualstudio/msbuild/build-loggers) you can either implement the [`ILogger`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.framework.ilogger) interface or derive from [`Logger`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.utilities.logger) (I recommend the latter).

Your logger will need to register for the events it wants to handle. This is done in the [`Initialize`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.utilities.logger.initialize) method which gives your logger an [`IEventSource`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.framework.ieventsource) instance. This event source contains the events that you should register handlers for (the same ones listed above, including a meta-event named [`AnyEventRaised`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.framework.ieventsource.anyeventraised) that calls your handler for all events).

For example, here's a simple logger that writes the start and end of each target out to the console:

```csharp
using Microsoft.Build.Framework;

public class TargetLogger : Logger
{
	public override void Initialize(IEventSource eventSource)
	{
		eventSource.TargetStarted +=
      (sender, evt) => Console.WriteLine($"{evt.TargetName} started");

		eventSource.TargetFinished +=
      (sender, evt) => Console.WriteLine($"{evt.TargetName} finished");
	}
}
```

# Adding Your Logger To a Build

Once you've written your logger you need to compile it to an assembly and tell MSBuild to use it with the `/logger` switch from the [MSBuild command-line interface](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference):

```cmd
msbuild /logger:TargetLogger,C:\Loggers\TargetLogger.dll ...
```

# Passing Parameters

One thing that's kind of neat about the MSBuild logging API is that you can pass whatever parameters you want from the command-line through to your logger. These are exposed as a [`Parameters`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.framework.ilogger.parameters) property in the [`ILogger`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.framework.ilogger) interface. That property will contain whatever string gets used on the command-line after a `;` when specifying the logger:

```cmd
msbuild /logger:TargetLogger,C:\Loggers\TargetLogger.dll;MyParameters,Foo,Bar ...
```

Note that it's up to you to parse it however is appropriate from your `Initialize` method.

# Writing A Cross Platform Logger

A challenge that I ran into was how to write a logger that could be used for both the Visual Studio version of MSBuild and the one that ships with the .NET Core SDK. These are essentially the same MSBuild, but each one targets a different runtime. The Visual Studio version of MSBuild targets .NET Framework 4.6 while the .NET Core SDK version of MSBuild targets either .NET Standard 2.0 or a close version of .NET Core (this seems to change with each SDK release). So the question is: what should your own logger target?

If you target `net46` and try to use your logger from the .NET Core SDK you'll get a runtime error. Likewise, if you target something like `netstandard2.0` you'll get a runtime error from the Visual Studio MSBuild. It turns out there is _one_ target that both versions of MSBuild have in common: `netstandard1.3`. If you target your logger to `netstandard1.3` you'll be able to use a single assembly for either MSBuild. However, if you need your logger to use APIs that aren't in .NET Standard 1.3 then you'll need to multi-target your logger and use whichever assembly is appropriate for the version of MSBuild you're using it with.

# Multi-Processor Logging

So far I've just discussed logging a nice linear sequence of events. That all [goes out the window](https://docs.microsoft.com/en-us/visualstudio/msbuild/logging-in-a-multi-processor-environment) when performing multi-processor builds. I'm not going to dive into that, at least not in this post, but it's worth keeping in mind.

# Logging Out Of Process

The last thing I want to talk about is the potential for responding to MSBuild logging events from another process, either on the same system or even over a network. MSBuild doesn't have a built-in capability for this, so I wrote a library called [MsBuildPipeLogger](https://msbuildpipelogger.netlify.com/) that can do this over an anonymous or named pipe. It abstracts the pipe mechanics from you, so you just need to create an instance of a server class and then add the `MsBuildPipeLogger.Logger` to MSBuild. The MsBuildPipeLogger server then allows your application to receive MSBuild logging events as the build proceeds. The MSBuildPipeLogger server also implements `IEventSource` so that you can connect a normal MSBuild logger to it as if it were running in-process directly from MSBuild.