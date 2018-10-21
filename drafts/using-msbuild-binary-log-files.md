Title: Using MSBuild Binary Log Files
Lead: Going beyond Structured Log Viewer.
Published: 10/4/2018
Image: /images/blocks.jpg
Tags:
  - msbuild
---
[In a previous post](/posts/msbuild-loggers-and-logging-events) I looked at MSBuild logging and logging events. In this post I want to talk about [MSBuild binary log files](http://msbuildlog.com/) and what you can do with them beyond viewing in the excellent [Structured Log Viewer](https://github.com/KirillOsenkov/MSBuildStructuredLog).

# Binary Log Files

As discussed in the [previous post](/posts/msbuild-loggers-and-logging-events), MSBuild emits a series of [different events](https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.framework.ieventsource?view=netframework-4.7.2#events) while it's executing. The MSBuild binary logger captures all of these events and serializes them to disk in a binary format. The result is a complete picture of every logging event which can be analyzed or replayed later to understand exactly what happened during a build.

To create a binary log you can either [use the `/bl` switch from the CLI](http://msbuildlog.com/#commandline) or use the [Project System Tools Visual Studio extension](https://marketplace.visualstudio.com/items?itemName=VisualStudioProductTeam.ProjectSystemTools). In both cases you'll end up with a `.binlog` file that contains all the serialized logging events from that build.

# Structured Log Viewer

By far the easiest way to view your binary log is with the [Structured Log Viewer](https://github.com/KirillOsenkov/MSBuildStructuredLog). This tool has been a lifesaver for me more than once and I use it _all the time_. Now that I've got that out of the way though, it's not really what this post is about. I want to look at other ways in which we can use the MSBuild binary log files.

# MSBuild API

MSBuild has an API for reading binary log files that ships with the [Microsoft.Build](https://www.nuget.org/packages/Microsoft.Build/) package. Unfortunately, [until recently](https://github.com/Microsoft/msbuild/pull/3814) it was `internal` and you have to use a little proxy (or otherwise use reflection) to get access to it:

```
public class BuildEventArgsReaderProxy
{
    private readonly Func<BuildEventArgs> _read;

    public BuildEventArgsReaderProxy(BinaryReader reader)
    {
        object argsReader;
        Type buildEventArgsReader =
            typeof(BinaryLogger).GetTypeInfo().Assembly
                .GetType("Microsoft.Build.Logging.BuildEventArgsReader");
        ConstructorInfo readerCtor = buildEventArgsReader
            .GetConstructor(new[] { typeof(BinaryReader) });
        if (readerCtor != null)
        {
            argsReader = readerCtor.Invoke(new[] { reader });
        }
        else
        {
            readerCtor = buildEventArgsReader
                .GetConstructor(new[] { typeof(BinaryReader), typeof(int) });
            argsReader = readerCtor.Invoke(new object[] { reader, 7 });
        }
        MethodInfo readMethod = buildEventArgsReader.GetMethod("Read");
        _read = (Func<BuildEventArgs>)readMethod
            .CreateDelegate(typeof(Func<BuildEventArgs>), argsReader);
    }

    public BuildEventArgs Read() => _read();
}
```

Once you've got access to the `BuildEventArgsReader.Read()` method, either directly, through a proxy like above, or via some other reflection hackery, you can use it to get one `BuildEventArgs` event at a time from the binary log file until none are left:

```
string logFile = @"C:\MyLogFile.binlog";
using (Stream stream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.Read))
{
    GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
    BinaryReader binaryReader = new BinaryReader(gzipStream);
    BuildEventArgsReaderProxy buildEventArgsReader =
        new BuildEventArgsReaderProxy(binaryReader);

    // We don't actually need this, but have to advance the BinaryReader past it
    int fileFormatVersion = binaryReader.ReadInt32();

    while(true)
    {
        BuildEventArgs buildEvent = buildEventArgsReader.Read();
        if(buildEvent == null)
        {
            break;
        }
        // Do something with the buildEvent
    }
}
```

There are two little gotchas to point out in the code above. The first is that we use a `GZipStream` instead of just reading directly from the `FileStream`. That's because binary log files are gzipped to save space when they're originally written. The second gotcha is that we have to read a version flag from the file before reading it. We could probably be smarter about matching the version from the file to the read operations, but this code has worked for me in most situations.

# Structured Log Viewer API

BinLogReader

# Replaying Logging Events

Can send them to any MSBuild logger

# Structured Log Viewer Composition

Getting back to the [Structured Log Viewer](https://github.com/KirillOsenkov/MSBuildStructuredLog), it actually has it's own API for reading and constructing a tree representation of all the logging events that closely matches what you see in the Structured Log Viewer interface. It just happens that this tree structure is also a pretty good representation that you can use from your own code as well.

# Buildalyzer