Title: Using MSBuild Binary Log Files
Lead: Going beyond Structured Log Viewer.
Published: 10/4/2018
Image: /images/blocks.jpg
Tags:
  - msbuild
---
[In my previous post](/posts/msbuild-loggers-and-logging-events) I looked at MSBuild logging and logging events. In this post I want to talk about [MSBuild binary log files](http://msbuildlog.com/) and what you can do with them beyond viewing in the excellent [Structured Log Viewer](https://github.com/KirillOsenkov/MSBuildStructuredLog).

# Binary Log Files

As discussed in the [previous post](/posts/msbuild-loggers-and-logging-events), MSBuild emits a series of [different events](https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.framework.ieventsource?view=netframework-4.7.2#events) while it's executing. The MSBuild binary logger captures all of these events and serializes them to disk in a binary format. The result is a complete picture of every logging event which can be analyzed or replayed later to understand exactly what happened during a build.

To create a binary log you can either [use the `/bl` switch from the CLI](http://msbuildlog.com/#commandline) or use the [Project System Tools Visual Studio extension](https://marketplace.visualstudio.com/items?itemName=VisualStudioProductTeam.ProjectSystemTools). In both cases you'll end up with a `.binlog` file that contains all the serialized logging events from that build.

# Structured Log Viewer

By far the easiest way to view your binary log is with the [Structured Log Viewer](https://github.com/KirillOsenkov/MSBuildStructuredLog). This tool has been a lifesaver for me more than once and I use it _all the time_. Now that I've got that out of the way though, it's not really what this post is about. I want to look at other ways in which we can use the MSBuild binary log files.

# Using MSBuild API

It turns out that MSBuild actually has an API for reading binary log files that deserializes them in sequence for you.

# Replaying Logging Events

Can send them to any MSBuild logger

# Structured Log Viewer Composition

# Buildalyzer