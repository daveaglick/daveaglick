Title: Introducing FluentBootstrap
Lead: Extensions, helper classes, and other goodies to help you use the Bootstrap CSS framework.
Published: 11/19/2014
Tags:
  - FluentBootstrap
  - Bootstrap
  - ASP.NET
  - ASP.NET MVC
---
<strong><a href="http://www.fluentbootstrap.com">FluentBootstrap Web Site</a></strong><br />
<strong><a href="https://github.com/daveaglick/FluentBootstrap">GitHub</a></strong><br />
<strong><a href="http://www.nuget.org/packages?q=fluentbootstrap">NuGet</a></strong><br />

<hr />

<p>I'm been using <a href="http://getbootstrap.com">Bootstrap</a> for a long time, and other CSS frameworks (like the <a href="http://yahooeng.tumblr.com/post/96098168666/important-announcement-regarding-yui">now orphaned YUI</a>) even longer. I like the way a good CSS framework gives you a lot of boilerplate and provides a jumping-off point for more complex design and functionality without having to start from scratch. As I started to integrate my ASP.NET MVC projects with Bootstrap more frequently (and then Microsoft made it part of the default ASP.NET MVC template), I became frustrated with the lack of integration between the HTML and CSS with the model and code. FluentBootstrap was born about a year ago as a way to provide strongly-typed <a href="http://www.asp.net/mvc/overview/older-versions-1/views/creating-custom-html-helpers-cs">HtmlHelpers</a> for Bootstrap components (especially Bootstrap-styled forms). However, as I continued adding support for more and more of the Bootstrap framework, I decided just to take it all the way and support code-based helpers for all Bootstrap components. This took a long time to complete (and it's not totally done - I still need to support the JavaScript components), but today I am pleased to announce the first public release of FluentBootstrap!</p>

<p>Put simply, FluentBootstrap lets you output this HTML:</p>
<pre class="prettyprint">&lt;nav class=&quot;navbar-static-top navbar-default navbar&quot; 
 id=&quot;navbar&quot; role=&quot;navigation&quot;&gt;
 &lt;div class=&quot;container-fluid&quot;&gt;
  &lt;div class=&quot;navbar-header&quot;&gt;
   &lt;a class=&quot;navbar-brand&quot; href=&quot;#&quot;&gt;FluentBootstrap&lt;/a&gt;
   &lt;button class=&quot;collapsed navbar-toggle&quot; data-target=&quot;#navbar-collapse&quot;
    data-toggle=&quot;collapse&quot; type=&quot;button&quot;&gt;
    &lt;span class=&quot;sr-only&quot;&gt;Toggle Navigation&lt;/span&gt;
    &lt;span class=&quot;icon-bar&quot;&gt;&lt;/span&gt;
    &lt;span class=&quot;icon-bar&quot;&gt;&lt;/span&gt;
    &lt;span class=&quot;icon-bar&quot;&gt;&lt;/span&gt;
   &lt;/button&gt;
  &lt;/div&gt;
  &lt;div class=&quot;collapse navbar-collapse&quot; id=&quot;navbar-collapse&quot;&gt;
   &lt;div class=&quot;navbar-left navbar-nav nav&quot;&gt;
    &lt;li&gt;&lt;a href=&quot;/&quot;&gt;Introduction&lt;/a&gt;&lt;/li&gt;
    &lt;li&gt;&lt;a href=&quot;/Installation&quot;&gt;Installation&lt;/a&gt;&lt;/li&gt;
    &lt;li&gt;&lt;a href=&quot;/Usage&quot;&gt;Usage&lt;/a&gt;&lt;/li&gt;
    &lt;li&gt;&lt;a href=&quot;/Development&quot;&gt;Development&lt;/a&gt;&lt;/li&gt;
   &lt;/div&gt;
  &lt;/div&gt;
 &lt;/div&gt;
&lt;/nav&gt;</pre>

<p>By writing this code instead:</p>
<pre class="prettyprint">@using (var navbar = Html.Bootstrap(this).Navbar(&quot;FluentBootstrap&quot;)
    .SetPosition(NavbarPosition.StaticTop).Begin())
{
    @navbar.NavbarLink(&quot;Introduction&quot;, &quot;/&quot;)
    @navbar.NavbarLink(&quot;Installation&quot;, &quot;/Installation&quot;)
    @navbar.NavbarLink(&quot;Usage&quot;, &quot;/Usage&quot;)
    @navbar.NavbarLink(&quot;Development&quot;, &quot;/Development&quot;)
}</pre>

<p>There's a lot more information over on the <a href="http://www.fluentbootstrap.com">FluentBootstrap web site</a>, so check it out. Right now I'm particularly interested in any feedback you have or bugs that you find. I'm also interested in where you'd like to see the project go. For example, would you rather see more model binding functionality, T4MVC support, etc.?</p>