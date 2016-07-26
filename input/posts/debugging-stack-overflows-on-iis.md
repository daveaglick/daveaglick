Title: Debugging Stack Overflows on IIS
Lead: 1990 called and they want their debugger back.
Published: 1/13/2015
Tags:
  - IIS
  - debugging
  - ASP.NET
---
<p>I recently had a problem with an ASP.NET application on IIS where IIS would kill my Application Pool process whenever a specific query was executed. There was no warning and while I could see the process disappear from my logging tool in real-time, I didn't get any error messages or exceptions. Thus began an epic adventure of debugging rules, crash dumps, and WinDbg. It's worth noting that I really have no idea what I'm doing here. Like most modern .NET developers I gave up this sort of thing years ago in favor of the debugging available through Visual Studio and other modern tools. However, it turns out that the only way to get detailed information in cases where IIS kills your process is to do it the hard way. There's probably also multiple hard ways to do it - this is just the one that worked well for me. Hopefully this saves you some time should you ever need to follow me down the rabbit hole.</p>

<h2>Check The Event Logs</h2>

<p>Before you start the process of getting crash dumps, etc. it might be helpful to check out the Windows Event Log. This should give you some small indication of what's going on, or at least give you something to Google. In my case I didn't have anything in the Application log, but in the System log I found an Information message with a source of Application Popup every time my process got killed:</p>

```
Application popup: w3wp.exe - System Error : A new guard page for the stack cannot be created.
```

<p>I also had a Warning from the source WAS:</p>

```
A process serving application pool 'MyAppPool' suffered a fatal communication error with the Windows Process Activation Service. The process id was '5288'. The data field contains the error number.
```

<p>These two messages gave me enough for a search and pointed me in the right direction. In this case all signs pointed to a stack overflow, but your specific Event Log messages might indicate something different.</p>

<h2>Enable Debugging Rules</h2>

<p>The next step is to enable debugging rules to generate crash dumps. You'll need the Debug Diagnostic Tool for this part, which <a href="http://www.microsoft.com/en-us/download/details.aspx?id=42933">can be obtained here</a>. Once you've installed it, you'll need to restart IIS with a <code>iisreset</code> command from the command prompt. Then launch the Debug Diagnostic Tool and walk through the wizard for setting up debugging rules. This essentially creates watchers for various events and then takes various actions (such as dumping logs) when they occur. Specifically you want to create a crash rule:</p>

<img src="/posts/images/debug-diagnostic-1.png" class="img-responsive"></img>
<img src="/posts/images/debug-diagnostic-2.png" class="img-responsive"></img>

<p>Then when you get to the Advanced Configuration page, click on Exceptions and then Add Exception. This will let you select from a list of exceptions including Stack Overflow. Also make sure to change the Action Type to Full Userdump:</p>

<img src="/posts/images/debug-diagnostic-3.png" class="img-responsive"></img>

<p>Finally, select a location for the log files. Make sure it's got plenty of space because the files can get pretty big.</p>

<h2>Open The Dump In WinDbg</h2>

<p>Now run your application until it crashes. You should get some large <code>.dmp</code> files in the folder you selected during the wizard. For the next part you'll need to obtain WinDbg. It's important to get the version that matches the build profile of your application, not your system. So if your ASP.NET application is built for x86, you should get the x86 WinDbg tool even if your server is x64. The raw installers can be obtained <a href="http://rxwen-blog-stuff.googlecode.com/files/windbg_6.12.0002.633_x86.zip">here for x86</a> and <a href="http://rxwen-blog-stuff.googlecode.com/files/windbg_6.12.0002.633_64_installer.zip">here for x64</a>.</p>

<p>Once you install WinDbg, open it up and select Open Crash Dump from the File menu. Then you'll need to type the following two commands into the crash dump window:</p>

```
.loadby sos clr
!clrstack
```

<p>If everything goes well, this should print out the full stack when the process crashed (it might take a while). That should get you on your way to debugging what went wrong.</p>

<h2>Additional Resources</h2>

<p>In the course of trying to figure all of this out I came across some valuable resources:</p>

* <a href="http://stackoverflow.com/questions/5053708/how-to-debug-w3wp-exe-process-was-terminated-due-to-a-stack-overflow-works-on">This Stack Overflow question</a> got the ball rolling.
* <a href="http://blog.whitesites.com/Debugging-Faulting-Application-w3wp-exe-Crashes__634424707278896484_blog.htm">This blog post</a> gives some good information about what to do once you get the crash dump loaded in WinDbg.
* <a href="https://support.microsoft.com/kb/919789?wa=wsignin1.0">This Microsoft support page</a> has some step-by-step instructions for configuring Debug Diagnostic Tool in different environments.
* <a href="http://geekswithblogs.net/.NETonMyMind/archive/2006/03/14/72262.aspx">This blog post</a> has a good reference on the commands that are available in WinDbg.