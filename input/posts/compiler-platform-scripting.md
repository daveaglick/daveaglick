Title: Introduction to Scripting with the .NET Compiler Platform (Roslyn)
Lead: So easy a caveman can do it.
Published: 2/18/2015
Tags:
  - Roslyn
  - .NET Compiler Platform
  - scripting
---
<p>Scripting support in the .NET Compiler Platform (formerly known as Roslyn) has been a long time coming. It was originally introduced more than a year ago and then removed while the team considered what the ideal API should look like. It was recently reintroduced into the <a href="https://github.com/dotnet/roslyn/tree/master/src/Scripting">master source branch on GitHub</a>, though as of this blog post it still isn't available on the <a href="https://www.myget.org/gallery/roslyn-nightly">nightly MyGet feed</a> or on <a href="https://www.nuget.org/profiles/RoslynTeam">NuGet</a>. In this post I will explain how to obtain and build the new scripting bits (including on a system without Visual Studio 2015 - it can actually be built using only Visual Studio 2013), introduce some of the scripting functionality, and show some scenarios where this might be helpful in your own applications. I also want to caveat this post by saying that it may go out of date quickly. The .NET Compiler Platform is under heavy development and it is changing frequently, including the public API. While I wouldn't expect any sweeping changes in the scripting support at this point, many of the details are subject to change.</p>

<h2>Obtaining and Building</h2>

<p>Since the scripting support isn't available in one of the binary distribution channels like the <a href="https://www.myget.org/gallery/roslyn-nightly">nightly MyGet feed</a> or on <a href="https://www.nuget.org/profiles/RoslynTeam">NuGet</a>, you'll need to obtain and build the .NET Compiler Platform from source in order to use them. Luckily the development team has been working hard to ensure this part is straightforward. The first step is to clone the <a href="https://github.com/dotnet/roslyn">repository from GitHub</a> using the following command (or your favorite Git GUI): <code>git clone https://github.com/dotnet/roslyn.git</code>.</p>

<p>The readme file says that to build the libraries you'll need Visual Studio 2015, but that's not actually true. You can build the .NET Compiler Platform just fine with only Visual Studio 2013 using the special <code>Roslyn2013.sln</code> solution file they've provided. In fact, it's just a simple two-command process. Make sure to run these commands from a Visual Studio command prompt. First you have to restore the NuGet packages and then build the solution. You might get some warnings about missing FxCop, but those are safe to ignore. From the <code>\src</code> directory in the repository run:</p>

<pre class="prettyprint">powershell .nuget\NuGetRestore.ps1
msbuild Roslyn2013.sln</pre>

<p>This will compile a bunch of libraries to the <code>\Binaries\Debug</code> directory. You don't need all of them for scripting. Assuming you want to script in C#, the libraries you need are:</p>

* Microsoft.CodeAnalysis
* Microsoft.CodeAnalysis.CSharp
* Microsoft.CodeAnalysis.Desktop
* Microsoft.CodeAnalysis.CSharp.Desktop
* Microsoft.CodeAnalysis.Scripting
* Microsoft.CodeAnalysis.Scripting.CSharp
* System.Collections.Immutable
* System.Reflection.Metadata

<p>I also ran into a problem with <code>VBCSCompiler.exe</code> crashing during the build, which appears like it might be related to <a href="https://github.com/dotnet/roslyn/issues/110">this issue</a>. In any case, upgrading to the latest .NET 4.5.2 runtime resolved my problem. Of course, if you have Visual Studio 2015 installed you can also try building the other <code>Roslyn.sln</code> solution.</p>

<h2>Your First Script</h2>

<p>Writing a simple script in the .NET Compiler Platform is really easy. For the remainder of this post I will discuss scripting in C#. There is a nearly identical API for scripting in Visual Basic if that's your thing. The <code>CSharpScript</code> class has a bunch of static methods that provide a good entry point to the API. Perhaps the most straightforward of these is <code>CSharpScript.Eval()</code> which will evaluate C# statements and return a result. For example:</p>

<pre class="prettyprint">var value = CSharpScript.Eval("1 + 2");
Console.Write(value); // 3</pre>

<p>This returns an <code>int</code> with a value of <code>3</code>. See, easy right? If you want more control, there's also <code>CSharpScript.Create()</code> which returns a <code>CSharpScript</code> object suitable for further manipulation before evaluation and <code>CSharpScript.Run()</code> which evaluates the script and returns a <code>ScriptState</code> object with the return value and other state information useful for REPL scenarios.</p>

<h2>Getting Variables</h2>

<p>As you saw above, it's easy to get the return value from the script using the <code>CSharpScript.Eval()</code> method. But what about other variables that get created during evaluation? We can get those as well by using the <code>ScriptState</code> object you get back from calling <code>CSharpScript.Run()</code>. It contains a member called <code>Variables</code> (of type <code>ScriptVariables</code>) that enumerates <code>ScriptVariable</code> objects with the name, type, and value for each variable the script created. For example:</p>

<pre class="prettyprint">ScriptState state = CSharpScript.Run("int c = 1 + 2;");
var value = state.Variables["c"].Value;
Console.Write(value); // 3</pre>

<h2>References and Namespaces</h2>

<p>If you want to do anything reasonably advanced, you'll probably need to include references and import namespaces for additional assemblies. Thankfully this is also really easy. There are a number of ways to do it, but the easiest is to use a <code>ScriptOptions</code> object which is accepted by any of the three <code>CSharpScript</code> static methods. The default <code>ScriptOptions</code> includes the <code>System</code> assembly and namespace and will search for additional assemblies in the current runtime directory. To modify this, start with <code>ScriptOptions.Default</code> and use it's fluent interface to setup additional references and namespaces (you can also create your own <code>ScriptOptions</code> if you don't want the default <code>System</code> assembly and namespace). Use <code>ScriptOptions.AddReferences()</code> to add references and <code>ScriptOptions.AddNamespaces()</code> to add namespaces (there are also several variations on these methods). <code>ScriptOptions.AddReferences()</code> accepts a variety of different ways of referring to assemblies, including the .NET Compiler Platform type <code>MetadataReference</code> if you're used to using that from the other portions of the platform. Here is an example of including <code>System.IO</code> support in a script:</p>

<pre class="prettyprint">ScriptOptions options = ScriptOptions.Default
	.AddReferences(Assembly.GetAssembly(typeof(Path)))
	.AddNamespaces("System.IO");
var value = CSharpScript.Eval(@"Path.Combine(""A"", ""B"")", options);
Console.Write(value); // A\B</pre>

<h3>Dynamic Support</h3>

<p>Getting support for <code>dynamic</code> in your script can be a challenge if only because it's not obvious which assemblies need to be referenced to support it. The answer is that you need <code>System.Core</code> and <code>Microsoft.CSharp</code>. That's all that's strictly needed, but if you also want support for <code>ExpandoObject</code> you'll need an extra reference and namespace support for <code>System.Dynamic</code>. Here is an example script with <code>dynamic</code> support (note that there's no magic to the types I pass to <code>Assembly.GetAssembly()</code>; these just happen to be types I know are defined in the required assemblies). Note also that I had to use <code>CSharpScript.Run()</code> since you can't directly return values from a script so I had to store my value in a variable and get it from the <code>ScriptState</code> object as we saw earlier.</p>

<pre class="prettyprint">ScriptOptions options = ScriptOptions.Default
	.AddReferences(
		Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject)),  // System.Code
		Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)),  // Microsoft.CSharp
		Assembly.GetAssembly(typeof(System.Dynamic.ExpandoObject)))  // System.Dynamic
	.AddNamespaces("System.Dynamic");
ScriptState state = CSharpScript.Run(@"
	dynamic dyn = new ExpandoObject();
	dyn.Five = 5;
	var value = dyn.Five;", options);
Console.Write(state.Variables["value"].Value); // 5</pre>

<h2>Setting Globals</h2>

<p>We've seen a number of ways of getting data out of the script, but what about getting data into the script? This is one of my favorite features of the scripting API because it's so easy to use. To set the data available to the script, you just have to pass an arbitrary object to one of the three <code>CSharpScript</code> static methods. The members of this object will then be available to your script as globals. For example:</p>

<pre class="prettyprint">// Defined elsewhere
public class Globals
{
	public int X;
	public int Y;
}

var value = CSharpScript.Eval("X + Y", new Globals { X = 1, Y = 2 });
Console.Write(value); // 3</pre>

<p>Note that the scripting engine respects protection levels so it's not possible to directly pass an anonymous object to the script because anonymous objects are usually scoped to the method in which they appear.</p>

<h2>Creating a Delegate</h2>

<p>Finally you may want to compile the script, but store it for later reuse. Thankfully, there is also an easy way to create a delegate from any method in your script by calling <code>ScriptState.CreateDelegate()</code>.</p>

<pre class="prettyprint">ScriptState state = CSharpScript.Run("int Times(int x) { return x * x; }");
var fn = state.CreateDelegate<Func<int, int>>("Times");
var value = fn(5);
Console.Write(value);  // 25</pre>

<h2>Conclusions</h2>

<p>By now you've hopefully thought of some use cases for the new scripting capability. My personal favorite at the moment is using this to drive configuration files. Instead of configuring your application using XML or JSON, why not set up a scripting environment and let your users (or you) write code to configure the application. If this interests you, I should also mention <a href="https://github.com/config-r/config-r">ConfigR</a> which does exactly this while abstracting away many of the details. Any post on C# scripting would also be incomplete without a mention of <a href="https://github.com/scriptcs/scriptcs">scriptcs</a> which provides a REPL for you to use on the command line for executing your own script files.</p>

<p>There's also a lot more to the .NET Compiler Platform scripting support than what I've discussed here. It's even possible to compile your script to a syntax tree and then use the rest of the .NET Compiler Platform capabilities to explore, analyze, and manipulate the script. A good place to continue exploring is the <a href="http://source.roslyn.io/">enhanced Roslyn source view site</a>.</p>