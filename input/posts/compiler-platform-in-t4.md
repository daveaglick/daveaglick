Title: Using the .NET Compiler Platform in T4 Templates
Lead: Metaprogramming with Roslyn.
Published: 4/23/2015
Tags: 
  - Roslyn
  - .NET Compiler Platform
  - T4
  - templating
---
<p><a href="https://msdn.microsoft.com/en-us/library/bb126445.aspx">T4 templates</a> provide a powerful way to generate code at design time (and sometimes at compile time if you set up Visual Studio appropriately). The traditional way of accessing the code of your solution from within a T4 template is to get the Visual Studio API (called <a href="https://msdn.microsoft.com/en-us/library/vstudio/envdte.dte(v=vs.100).aspx">DTE</a>). This has always seemed like a bit of a kludge to me and feels a little too far removed from the code and what it represents. We now have another option by using the .NET Compiler Platform from within a T4 template to parse, query, and output content based on the files in our solution.</p>

<p>In my particular case I wanted to scan all the source files in the same folder as the template, look for classes that derive from a specific base class (<code>Module</code>), iterate over all their public constructors, and then output extension methods for the <code>IPipeline</code> interface for each constructor that instantiates the class using that constructor and adds it to the pipeline. Here's what the template looks like:</p>


<pre class="prettyprint">&lt;#@ template debug=&quot;false&quot; hostspecific=&quot;true&quot; language=&quot;C#&quot; #&gt;
&lt;#@ assembly name=&quot;System.Core&quot; #&gt;
&lt;#@ assembly name=&quot;System.IO&quot; #&gt;
&lt;#@ assembly name=&quot;System.Runtime&quot; #&gt;
&lt;#@ assembly name=&quot;System.Text.Encoding&quot; #&gt;
&lt;#@ assembly name=&quot;System.Threading.Tasks&quot; #&gt;
&lt;#@ assembly name=&quot;$(TargetDir)Microsoft.CodeAnalysis.dll&quot; #&gt;
&lt;#@ assembly name=&quot;$(TargetDir)Microsoft.CodeAnalysis.CSharp.dll&quot; #&gt;
&lt;#@ assembly name=&quot;$(TargetDir)System.Collections.Immutable.dll&quot; #&gt;
&lt;#@ import namespace=&quot;System.Linq&quot; #&gt;
&lt;#@ import namespace=&quot;System.Text&quot; #&gt;
&lt;#@ import namespace=&quot;System.Collections.Generic&quot; #&gt;
&lt;#@ import namespace=&quot;System.IO&quot; #&gt;
&lt;#@ import namespace=&quot;Microsoft.CodeAnalysis&quot; #&gt;
&lt;#@ import namespace=&quot;Microsoft.CodeAnalysis.CSharp&quot; #&gt;
&lt;#@ import namespace=&quot;Microsoft.CodeAnalysis.CSharp.Syntax&quot; #&gt;
&lt;#@ output extension=&quot;.cs&quot; #&gt;
using System;
using System.Collections.Generic;
using System.IO;

&lt;# Process(); #&gt;
&lt;#+
	public void Process()
	{
		// Get a SyntaxTree for every file
		foreach(CSharpSyntaxTree syntaxTree in Directory.EnumerateFiles(Path.GetDirectoryName(Host.TemplateFile))
			.Where(x =&gt; Path.GetExtension(x) == &quot;.cs&quot;)
			.Select(x =&gt; CSharpSyntaxTree.ParseText(File.ReadAllText(x)))
			.Cast&lt;CSharpSyntaxTree&gt;())
		{
			// Get all class declarations in each file that derive from Module
			foreach(ClassDeclarationSyntax classDeclaration in syntaxTree.GetRoot()
				.DescendantNodes()
				.OfType&lt;ClassDeclarationSyntax&gt;()
				.Where(x =&gt; x.BaseList != null &amp;&amp; x.BaseList.Types
					.Any(y =&gt; y.Type is Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax 
						&amp;&amp; ((Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax)y.Type).Identifier.Text == &quot;Module&quot;)))
			{
				// Output the same namespace as the class
				SyntaxNode namespaceNode = classDeclaration.Parent;
				while(namespaceNode != null &amp;&amp; !(namespaceNode is NamespaceDeclarationSyntax))
				{
					namespaceNode = namespaceNode.Parent;
				}
				if(namespaceNode != null)
				{
					WriteLine(&quot;namespace &quot; + ((NamespaceDeclarationSyntax)namespaceNode).Name.ToString() + Environment.NewLine + &quot;{&quot;);
				}
			
				// Output the extensions class
				WriteLine(&quot;    public static class &quot; + classDeclaration.Identifier.Text + &quot;PipelineExtensions&quot; + Environment.NewLine + &quot;    {&quot;);
			
				// Get all non-static public constructors
				foreach(ConstructorDeclarationSyntax constructor in classDeclaration.Members
					.OfType&lt;ConstructorDeclarationSyntax&gt;()
					.Where(x =&gt; x.Modifiers.Count == 1 &amp;&amp; x.Modifiers[0].Text == &quot;public&quot;))
				{
					// Output the static constructor method
					WriteLine(&quot;        public static IPipeline &quot; + classDeclaration.Identifier.Text + constructor.ParameterList.ToString().Insert(1, &quot;this IPipeline pipeline, &quot;) + Environment.NewLine + &quot;        {&quot;);
				
					// Create and add the module
					WriteLine(&quot;            return pipeline.AddModule(new &quot; + classDeclaration.Identifier.Text + &quot;(&quot; + string.Join(&quot;, &quot;, constructor.ParameterList.Parameters.Select(x =&gt; x.Identifier.Text)) + &quot;));&quot;);
				
					// Close method
					WriteLine(&quot;        }&quot;);
				}
			
				// Close extensions class
				WriteLine(&quot;    }&quot;);			
			
				// Close namespace
				if(namespaceNode != null)
				{
					WriteLine(&quot;}&quot;);
				}
			}
		}
	}
#&gt;</pre>

<p>And this is some example output:</p>

<pre class="prettyprint">using System;
using System.Collections.Generic;
using System.IO;

namespace Wyam.Core.Modules
{
    public static class AppendPipelineExtensions
    {
        public static IPipeline Append(this IPipeline pipeline, string content)
        {
            return pipeline.AddModule(new Append(content));
        }
    }
}</pre>

<p>A couple things to note:</p>

* You must use the Roslyn assemblies from NuGet. If you try to build them yourself, they won't work out of the box in a T4 template because of the way Roslyn assemblies are delay signed.
* In this case I didn't even need to compile or get a semantic model, the syntax tree was enough for me. If you need to go further (such as using symbol information) you can always bring in the Roslyn compilation APIs.
* I prefer to write my T4 templates entirely in C# and output content using <code>WriteLine()</code>. You can obviously use a different approach such as interspersing control logic with template content.