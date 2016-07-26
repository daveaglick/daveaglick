Title: Announcing RazorDatabase
Lead: An in-memory collection populated by Razor views and custom metadata.
Published: 9/24/2014
Edited: 6/24/2015
Tags:
  - RazorDatabase
  - open source
  - Razor
---
<p>On multiple web projects recently I've needed to display content that relies on metadata about other content. One example is displaying a list of recent news releases with summaries on the home page of a site. Another example would be something similar for blog entries. You can extend this idea even further by considering metadata oriented pages like a list of tags for all the blogs on a site, or a list of all news articles that pertain to a specific subject.</p>

<p>This isn't a new idea, but most existing approaches rely on databases to store and query this type of information. Whether a database is used directly or abstracted via a CMS, I really don't like that solution because it causes a disconnect between the metadata and the content itself. It also makes it harder to test, makes it more difficult to track and associate changes to the database with changes to the other content and code, and requires additional architecture and resources.</p>

<p>Also, using a CMS has always kind of given me the willies. I know how to code, and I know how to create web sites, so why am I going to farm that out to some tool that will enforce particular conventions, constrain what I can do, and sandbox my environment. Unless some sort of end-user or intermediate-user is going to be editing the site outside of the code then there's really no technical reason why you need a CMS. In the end though, I've used them like everyone else to help manage content and provide access to ample metadata.</p>

<p>So I started to ask the question, how can I get my metadata into me code? The answer is RazorDatabase. It reads pre-compiled Razor views in your ASP.NET MVC web project and stores all of the metadata it finds in collections that you can query at run time. This lets you code your entire site using the ASP.NET MVC web stack but still gives you the ability to have dynamic content sourced from the actual views. While it does require a little bit of setup, once everything is rigged up it becomes pretty simple to work with the site as you might with a traditional CMS.</p>

<p>In fact, this entire blog is built using RazorDatabase. Go ahead and check out <a href="https://github.com/daveaglick/daveaglick">the source code for this blog</a> to see how it all works. And when you're ready, it's <a href="https://github.com/daveaglick/RazorDatabase">available on GitHub</a> or <a href="https://www.nuget.org/packages/RazorDatabase/">as a NuGet package</a>. I'm very interested to hear if anyone else finds this as valuable as I have.</p>

<div role="alert" class="alert alert-info">    <div><strong>Update June, 2015:</strong> Since writing this post, I've converted my blog over to the <a href="http://wyam.io" class="alert-link">Wyam static site generator</a> and it no longer uses RazorDatabase.</div>
</div>