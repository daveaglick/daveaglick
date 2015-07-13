Title: Easy Performance and Query Logging in ASP.NET with Serilog and MiniProfiler
Published: 7/13/2015
Tags:
  - ASP.NET
  - ASP.NET MVC
  - Serilog
  - MiniProfiler
  - logging
---
I've been playing around with [Serilog](http://serilog.net/) and have been really, really impressed. If you're not familiar with it, Serilog provides a very nice logging API with the ability to log entire object graphs in addition to flag messages. It's also really extensible and has a lot of community support.

It was very easy to integrate Serilog into my ASP.NET MVC application with [SerilogWeb.Classic](https://github.com/serilog-web/classic), but I found myself wanting a little more information. Specifically, I would love to get a log of performance metrics and database queries (I use Entity Framework) that could be correlated with my other logging information. I could always set up timers on the request and hook into the Entity Framework pipeline for this, but I've already spent the time to instrument my important code with [MiniProfiler](http://miniprofiler.com/). What I really wanted was a way to take the information I get from MiniProfiler and write it to Serilog.

It turns out that with a little digging, this is pretty easy. First, we'll need a way to get the data out of MiniProfiler. The best I could find is to call `MiniProfiler.ToJson()` after calling `MiniProfiler.Stop()`. This outputs a JSON string with the complete set of MiniProfiler data including all subtimings and database queries (which you get if you're using one of the MiniProfiler Entity Framework extensions like [MiniProfiler.EF6](https://www.nuget.org/packages/MiniProfiler.EF6/)).

Since JSON is structured and Serilog likes structured data, the next question was how to get this data into Serilog in a format that maintains the structure? Thankfully someone has already written a Serilog extension to do exactly that. [Destructurama.JsonNet](https://github.com/destructurama/json-net) adds destructuring support for JSON.NET dynamic objects to Serilog. Using this extension, we can easily convert the MiniProfiler JSON string to a JSON.NET object and then feed it into Serilog.

The final code looks like this:

```
public class MvcApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
		Log.Logger = new LoggerConfiguration()
		    .Destructure.JsonNetTypes()
		    // ...
		
	    // ...
	}
	
	protected void Application_BeginRequest()
    {
        MiniProfiler.Start();
		// ...
    }
	
	protected void Application_EndRequest()
    {
        MiniProfiler.Stop();
        if (MiniProfiler.Current != null && MiniProfiler.Current.Root != null)
        {
            Log
			    .ForContext("MiniProfiler", JsonConvert.DeserializeObject<dynamic>(MiniProfiler.ToJson()))
			    .Verbose("Completed request in {Timing} ms", MiniProfiler.Current.Root.DurationMilliseconds);
        }
		// ...
	}
	
	// ...
}

