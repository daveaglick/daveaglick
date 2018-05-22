Title: Announcing Discover .NET
Lead: Awesome .NET open source and community resources.
Published: 5/22/2018
Image: /images/map.jpg
Tags:
  - open source
---
After what seems like an eternity in development, I am thrilled to announce the launch of [Discover .NET](https://discoverdot.net/). The site is an attempt to improve discoverability in the .NET ecosystem by collecting information on topics like projects, issues, blogs, groups, events, and resources.

<blockquote class="twitter-tweet" data-lang="en"><p lang="en" dir="ltr">Discoverability is definitely part of the equation. How can we expose other devs who would get value, especially those who aren’t on the social sites, to cool projects like yours? Still lots of room for improvement in that area.</p>&mdash; Dave Glick (@daveaglick) <a href="https://twitter.com/daveaglick/status/950883853715025920?ref_src=twsrc%5Etfw">January 10, 2018</a></blockquote>
<script async src="https://platform.twitter.com/widgets.js" charset="utf-8"></script>

I built this site for a few reasons, some community focused and others related to my own interests like static sites:
* Make it easier to learn about .NET stuff you may not have known about.
* Collect a comprehensive database of information on all things .NET.
* Demonstrate to myself and others what can be accomplished with data-centric static sites.
* Provide an example of how [Wyam](https://wyam.io/) can be used to power highly customized static sites. 

I’ll talk more about the technical nature of the site and those last two goals in a follow-up post, but for now I’d like to focus on the community aspects of the site. If you’d like skip the details on different areas of the site but want to know how to help, [skip ahead to the call to action](#call-to-action).

And a quick note: please don't take missing projects, blogs, events, etc. as even remotely personal. I've been slowly adding items for months and at some point I realized I would have to just ship the thing or it would never get out the door. I'll continue to add items, but now I also [need your help](https://discoverdot.net/suggest/) to make sure we catalog everything out there.

# Daily Discovery

This is where the idea for Discover .NET started and it grew from there (scope creep is a Real Thing That Happens). The daily discovery is a curated link to a project, blog, or other resource that you may not have seen. While some of the discoveries will be well known within the community, an emphasis will be placed on lesser-known resources. If you want to stay updated on discoveries, a feed is available.

# Projects and Issues

It became clear that gathering and presenting project information for the daily discovery could be extended to a sort of database across all .NET projects. One of the neat things about the site is that it integrates with GitHub and NuGet so that minimal information needs to be provided about a project to properly index it.

To make the project database more useful, a variety of sorts and filters were added including distinguishing between [Microsoft-sponsored projects](https://discoverdot.net/projects/?filter-microsoft), [.NET platform projects](https://discoverdot.net/projects/?filter-netplatform) (projects that are considered “part of the platform”), and [projects in the .NET Foundation](https://discoverdot.net/projects/?filter-netfoundation).

One of the more novel things about the site is how it deals with project issues. _Every_ open issue from every project is aggregated and presented on the site. Since Discover .NET is first and foremost designed to enhance community discoverability and participation, one of the goals of aggregating all the issues is to emphasize [help wanted issues](https://discoverdot.net/issues/?tab=helpwanted) and [recent issues](https://discoverdot.net/issues). [I’ve had an interest in doing something like this for years](https://github.com/up-for-grabs/up-for-grabs.net/issues/323) and am particularly proud of how well it turned out.

# Blogs and Posts

So much good information is communicated through blogs, but there are only a handful of ways to become exposed to blogs you may not otherwise have known to visit or keep up with. Curated post lists like [Dew Drop](https://www.alvinashcraft.com/) and [The Morning Brew](http://blog.cwa.me.uk/) are a great way to keep up, as are platforms like [Reddit](https://www.reddit.com/r/csharp/). However, all of these aim to distill blog posts to the most relevant and as far as I know there’s no good comprehensive collection of blogs and posts across the .NET community.

Discover .NET collects _every_ [blog and all their posts](https://discoverdot.net/blogs). This information is made available as [a list of recent posts from all blogs](https://discoverdot.net/#recent-news), [feeds you can subscribe to](https://discoverdot.net/feeds), and [searching capabilities](https://discoverdot.net/search).

# Broadcasts and Episodes

Podcasts and other types of broadcasts like YouTube tutorials and live coding screencasts are becoming more popular. In addition to blogs, Discover .NET also collects [broadcasts and their episodes](https://discoverdot.net/broadcasts).

# Recent News

To help keep you up to date on everything going on in .NET, recent posts and episodes from all blogs and broadcasts are presented on the homepage as well as available [via feed](https://discoverdot.net/feeds).

# Groups and Events

All of this online community is great, but this wouldn’t be a comprehensive resource without also including the real-world parts of the community. The Meetup API is used to pull all .NET related groups (using the “.NET” topic) and then combines them with data on other non-Meetup-based groups for a full picture of everything going on. [Groups are presented on a map and can be sorted and filtered by name or location](https://discoverdot.net/groups).

Likewise, the next event from Meetup groups as well as conferences and other types of events [are presented in a similar way](https://discoverdot.net/events).

# Resources

All of this data is great, but not everything valuable to the community fits into one of these clean categories. [The resources section](https://discoverdot.net/resources) includes other links like commercial products, web sites, and anything else that the community might find valuable.

# Search

I’m particularly fond of the [search feature](https://discoverdot.net/search) of the site. It lets you locate content across all the different data types. For example, [searching for “Blazor”](https://discoverdot.net/search?query=Blazor) yields some interesting issues, blog posts, and podcasts. More on how this works in a following post.

# Feeds and API

One hope I have for the site is that it extends beyond your browser. I’d love for you to be able to get the information you need when and where you want it. To this end, [several RSS and Atom feeds](https://discoverdot.net/feeds) are available. There’s also an [API](https://discoverdot.net/api) and I’d love for the community to use it and build interesting tools using all this data.

# Looking Ahead

This is an ongoing project and I have [lots of ideas](https://github.com/daveaglick/discoverdotnet/issues) for future improvements. A couple items I’d like to add soon are [support for multiple NuGet packages per-project](https://github.com/daveaglick/discoverdotnet/issues/15), [support for Chocolatey packages](https://github.com/daveaglick/discoverdotnet/issues/23), and a [Twitter bot](https://github.com/daveaglick/discoverdotnet/issues/28) that automatically posts content from the site. I’d love to hear what you think should be added, so [file an issue](https://github.com/daveaglick/discoverdotnet/issues/new) if you’ve got any ideas.

# Call To Action


I need your help! This is a site for and hopefully by the community. Gathering the initial data was really hard. Even though it’s easy to add any particular resource to the site, collecting hundreds of items took a lot of time. Now that the site is live, I’m hoping the community can help scale data collection. [Go here for instructions on how to suggest new content](https://discoverdot.net/suggest/). If you’re interested in taking an even more active roll, [drop me a line](https://twitter.com/daveaglick).