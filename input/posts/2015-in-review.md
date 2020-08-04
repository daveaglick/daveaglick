Title: 2015 In Review
Lead: A personal look back (and an important announcement).
Published: 1/1/2016
Tags:
  - personal
---
A lot happened in 2015, both in terms of global news stories and in our little world of Microsoft developers. But it was also a very special and productive year for me personally. I'd like to take a moment to look back at what happened and what I accomplished in my own life.

# Open Source
---

I've been involved in open source for a long time. My first project was published on a service called Assembla before GitHub took over the open source world. Even so, I was never extremely prolific. Inspired by Microsoft's moves in this direction near the end of 2014, I decided that 2015 was going to be the year that I really engaged the OSS community. I'd say I did pretty well:

<img src="/posts/images/github-2015.png" class="img-fluid" style="margin-top: 6px; margin-bottom: 6px;">

## FluentBootstrap

I released [FluentBootstrap](/posts/introducing-fluentbootstrap) near the end of 2014. Though the development pace slowed some (more on that in a bit), it still had a good amount of activity in 2015. That includes bringing on several contributors and becoming popular enough to generate a number of StackOverflow questions.

## LINQPad.CodeAnalysis

I was so enthusiastic about Roslyn that I wanted to play around with it well before the official releases were complete. I was especially excited about the visualization that Visual Studio 2015 provided for Roslyn syntax trees. Unfortunately, VS 2015 wasn't released yet and I wasn't able to upgrade (work policies, etc.) I also wasn't thrilled about needing Visual Studio just to play around. The result was an [open source library that added similar syntax tree visualization to LINQPad](/posts/announcing-linqpad-codeanalysis). It was (and still is) offered as an open source addin that you can download via NuGet to add syntax tree visualization to your LINQPad projects. However, shortly before LINQPad 5 was release, I got a message from Joe Albahari asking if he could include it out of the box. He did some great cleanup work using APIs that aren't available to addin developers, but if you click the "Tree" tab after running a LINQPad query in version 5, that's mostly my library under the hood.

<img src="/posts/images/linqpad-about.png" class="img-fluid" style="margin-top: 6px; margin-bottom: 6px;">

## @dotnetissues

I love that there are so many new Microsoft open source projects on GitHub. I also love keeping tabs on their new issues and pull requests - it's a great way to keep up with what's going on and learning how they work. Unfortunately, GitHub's notification system is all or nothing. You can get notices for every activity, including comments, but not just new stuff. I'd prefer to see all the new issues and then choose which to follow more closely. To address this I created the @dotnetissues Twitter account. It tweets all new issues and pull requests for many of the most important Microsoft GitHub projects. By hooking it up to IFTTT, you can get email notices for issues in the repositories and then only subscribe to issues you care about.

## RazorGenerator, DelegateDecompiler, Up For Grabs, Roslyn, Etc.

In addition to my own open source projects, I set a goal for 2015 to actively contribute to other projects as well. I think I did pretty well in that regard. Here were some of my bigger OSS contributions for the year:

- Helped move RazorGenerator from CodePlex to [GitHub](https://github.com/RazorGenerator).
- Did lots of work on the excellent [DelegateDecompiler](https://github.com/hazzik/DelegateDecompiler) library.
- Redesigned the [Up For Grabs](http://up-for-grabs.net/) website (I hope to do more with them in the coming year).
  <img src="/posts/images/up-for-grabs.png" class="img-fluid" style="margin-top: 6px; margin-bottom: 6px;">
- Submitted pull requests to Microsoft guided projects like [Roslyn](https://github.com/dotnet/roslyn), [automatic-graph-layout](https://github.com/Microsoft/automatic-graph-layout), and [corefxlab](https://github.com/dotnet/corefxlab) (and got myself an awesome Cup<T> mug for that first one)!
  <img src="/posts/images/cup-t.png" class="img-fluid" style="margin-top: 6px; margin-bottom: 6px;">
  
## Wyam

This all brings me to my most significant open source activity in 2015. In the summer [I released](/posts/announcing-wyam) a static content and site generation tool called [Wyam](https://wyam.io). Since that release (and leading up to it as well), the majority of my OSS time has been spent building it out. I have *so many* ideas that I expect I'll be working on it for a long time to come. I've also come to view it a little by as "my calling" in the OSS world. I believe strongly in the potential of static site generation and I think there are still a lack of really good tools for doing this in a controllable, extensible, and generic way.

# Community
---

I'm a shy introvert (which are [two different things](http://knowledgenuts.com/2014/03/07/the-difference-between-being-shy-and-being-introverted/)) so engaging folks in real-life interaction can be challenging for me (it doesn't help that I recently found out I'm partly deaf in mid-range frequencies - something I've suspected for a while). That said, I do enjoy it and recognize that there is a lot of great networking, knowledge sharing, and community-building to be done. I also try to stay active on social networks (especially [Twitter](https://twitter.com/daveaglick)) where I can interact without any difficulty.

## Speaking

Another goal I set for myself was to do more speaking. In a previous career I did a fair amount of speaking, but it was mostly on esoteric defense and mathematical topics. Now that my day job is more mainstream and I have some time for recreational development as well, I wanted to share my knowledge in these areas. To that end, I spoke at several events:

- Gave a talk on Roslyn at the NoVa Code Camp in the spring.
- Gave a lighting talk on Wyam at [.NET Unboxed](http://www.letsunbox.net/) (which didn't actually go so well, turns out 5 minutes isn't very long, especially when you spend 2 of them dealing with projector problems).
- Gave a talk on Roslyn at the DC .NET User's Group.

## Locally

I've really enjoyed going to local Nerd Dinners with the local .NET development community. There are a bunch of really great .NET/MS developers in the DC area and getting to know them this last year has been a highlight.

## Blogging

I wanted to blog some more this year, and managed to do pretty well. I wrote 17 blog posts (some of which were open source announcements). More importantly, I've seen better engagement on my blog and have gotten feedback from several folks that posts I wrote have helped them. That's awesome,and should help keep me going for another year of blogging.

# Professional
---

As the principal developer at a small non-profit, I lead a small team working on line of business applications to address our organization's unique needs. It's not glamorous work like you might find at Microsoft, Google, etc. but it's *work that matters*(tm) and is close to home, allowing me a lot of time with my family. We had two major releases this year, both built on Microsoft stacks with SQL Server, IIS, ASP.NET MVC, Entity Framework, and Kendo UI. Both went well, which is always a relief.

# Personal
---

I also had a number of major personal milestones this year. I have three kids, aged 8, 4, and 2 so this year felt like the first time I actually got a breather as our youngest entered his toddler phase. That didn't last too long though because we moved in the spring (though only down the block). I suspect it'll still be a while before I can come home and get any recreational coding done here (most of that happens during lunch breaks or early mornings at my work desk right now). This isn't a complaint - my family is my number-one priority (it's not even close) and I would give up everything else for them in a heartbeat.

# Up Next
---

I look forward to another ultra-productive 2016. My family will remain my top priority now and always, but within that constraint I hope to carve out even more time for open source and community efforts. I plan to double-down on my open source activities and be more engaged with projects outside my own. I'd like to blog some more too, but it remains to be seen how well that goal fits in with more open source coding (there are only so many hours in the day). If it comes down to it, I think writing code is more valuable both for me and for the community than spending time on a blog post. Finally, I'd like to continue speaking and have a goal for 3 - 4 engagements.

My big open source project for 2016 is to improve Wyam to the point that it's stable and easy to use. There is so much unrealized potential and I want to see more people benefit from what it can do for them and how it can make their blogging, documentation, and other site generation activities easier. My goal is no less than making Wyam as powerful and popular as the top static generators like Jekyll, Hugo, Middleman, etc.

Other than all that, I plan to savor the ride. Coding is fun. Community is fun. I'm thankful to have another year to enjoy them.

# One More Thing (A Last-Minute Update)
---
As I was waiting for AppVeyor to build my site, I got this in my email:

<img src="/posts/images/mvp.png" class="img-fluid" style="margin-top: 6px; margin-bottom: 6px;">

I am still in a bit of shock. I've worked very hard this last year on open source and community efforts, and am extremely humbled and grateful for this recognition. One of my biggest goals for 2016 will be to live up to the trust placed in me by Microsoft and the community and to help continue to share my code and my knowledge with .NET developers everywhere. Thank you!