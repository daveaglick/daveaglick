Title: A Tale of Two APIs
Lead: A strategy for dealing with multiple public interfaces for libraries.
Published: 1/22/2015
Tags:
  - programming
  - API
---
<p>When developing libraries it occasionally becomes necessary to expose a different public interface to different groups of users. The most common scenario is one in which your library needs to be accessed in one way by applications that use it, but another way by other libraries that extend it. You want extension developers to have access to all the behind-the-scenes details, but exposing those properties and methods to applications would be confusing or even damaging by promoting improper use. In other words, you want the <code>internal</code> properties and methods to be exposed to one set of developers but not another. In this post I'll examine a strategy for exposing different public APIs to different sets of users.</p>

<p>One way of accomplishing this is to make the extensions <a href="https://msdn.microsoft.com/en-us/library/0tke9fxk.aspx">friend assemblies</a> by using the <code>InternalsVisibleTo</code> attribute. While tempting, this is a bad idea. Friend assemblies work well for unit testing scenarios and for some enterprise-style development where there is a tight known coupling between components and all the code is well controlled. In any other case, they prevent proper extension by requiring the base library to know about all the extension libraries at compile-time. The web is strewn with disillusioned developers sharing anecdotes of how architectures that rely on <code>InternalsVisibleTo</code> caused them pain and agony.</p>

<p>So if we don't want to make our <code>internal</code> members <code>public</code> for everyone and we also don't want to expose them to specific libraries with <code>InternalsVisibleTo</code>, what can we do? The answer is extension methods. One interesting trait of extension methods is that they aren't available unless their namespace is in scope. This allows us to create sets of extension methods that are only visible if certain namespaces have been explicitly imported.</p>

<p>Consider the following class:</p>

<pre class='prettyprint'>namespace MyLibrary
{
    public class Car
    {
        public int NumberOfTires { get; internal set; }
    }
}</pre>

<p>Assume that the number of tires is set by some sort of car factory class internal to the library and we don't want normal library users to change it. However, let's say we did want extension developers to have access to the tire count so that they could create alternate factory classes. Using this approach, the answer would be to create an extension method that would allow changing the number of tires in a special namespace:</p>

<pre class='prettyprint'>namespace MyLibrary.Internal
{
    public static class CarExtensions
    {
        public static void SetNumberOfTires(this Car car, int numberOfTires)
        {
            car.NumberOfTires = numberOfTires;
        }
    }
}</pre>

<p>Because the <code>SetNumberOfTires()</code> extension method is still in the <code>MyLibrary</code> project, it has access to the <code>internal</code> <code>NumberOfTires</code> setter. In essence, the extension method is proxying the <code>internal</code> property and making it <code>public</code>. All an extension library has to do in order to use it is to add <code>using MyLibrary.Internal;</code> to any code that needs access.</p>

<p>There are a couple drawbacks to this approach. The first is that by exposing <code>internal</code> code through <code>public</code> extension methods, those bits aren't actually hidden from outside use anymore. While segregating the extensions into a special namespace makes sure they won't pollute the public API, this strategy shouldn't be used if you truly want those properties or methods to remain unavailable to outside code. Another drawback is that the API used to access the <code>internal</code> code doesn't directly match the <code>internal</code> code. For example, you'll end up with a lot of <code>.GetXyz()</code> and <code>.SetXyz()</code> extensions since you can't create extension properties. Also, you obviously can't expose entire classes this way (though I suppose you could put interfaces or proxy classes in the internal namespace for this purpose). Finally, it requires duplicating portions of your code. For every <code>internal</code> property or method you want to expose, you also have to write and maintain a matching extension method. However, if you can live with these limitations and feel that a clean API for different sets of consumers is more important than the maintenance burden, this might just do the trick.</p>