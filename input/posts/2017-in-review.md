Title: 2017 In Review
Lead: A look back.
Published: 1/2/2018
Image: /images/2017-2018.jpg
Tags:
  - personal
---
2017 was an interesting year for me. Aside from what was going on in the national and world stages (which I won’t really address here, but which had a huge impact on me emotionally and personally), I also found myself with a lot of family obligations (I am now both the committee chair for my son's Cub Scout pack as well as our school's PTA treasurer). Even though my free time was at a premium, I think it was a good year for me development-wise so here’s a look back at what I accomplished.

# Open Source

While I had several good periods of productivity, in general I didn't quite match my output from 2016. My GitHub commit count was down to 768 contributions from 1,065 contributions in 2016. For the first time in a while I started to struggle from burn-out late in the summer and into the fall (you can totally see it creeping in on my commit graph). Then work got intense near the later part of the year and I lost a lot of my lunch break OSS time. I feel like I'm getting back on track though, so hopefully I can resume a higher level of contributions in 2018.

<img src="/posts/images/github-2017.png" class="img-responsive" style="margin-top: 6px; margin-bottom: 6px;">

## Wyam

By far my biggest OSS highlight was releasing [Wyam 1.0](/posts/wyam-10). It felt great to reach such an important milestone. Along the way, the Wyam community has really grown and I've enjoyed helping lots of developers and contributors get started. Wyam now powers several of the documentation sites for [.NET Foundation](https://www.dotnetfoundation.org) projects as well as [many other blogs and other sites](https://wyam.io/docs/resources/built-with-wyam). My big push for 2018 is to complete a port to .NET Core culminating in version 2.0.

## Scripty

Unfortunately OSS time is often zero-sum and the work I put into getting Wyam to 1.0 couldn't be spent on improving my other major project [Scripty](https://github.com/daveaglick/Scripty). Rest assured the project isn't dead. One of the first things I plan to do in 2018 is revisit Scripty and make some major improvements to get it working with the most recent versions of .NET Core/Standard. I also have some ideas for improvements and how to make it even more valuable as a platform for compile-time code generation.

## Buildalyzer, NetlifySharp, and others

Somehow I did find the time in 2017 to release two new projects: [Buildalyzer](https://github.com/daveaglick/Buildalyzer) and [NetlifySharp](https://github.com/daveaglick/NetlifySharp). Buildalyzer helps you perform design-time builds from and using any version of .NET. NetlifySharp is a .NET client for [Netlify](https://netlify.com).

I've also deprecated a couple of my OSS projects. [FluentBootstrap](https://github.com/daveaglick/FluentBootstrap) just isn't as relevant as it used to be. I'd love to see someone else take over the project, and I still use it myself in some projects, but I don't plan on spending any significant time on it.

In general I've started to think of my OSS projects as part of a _portfolio_. I tend to be susceptible to [nerd sniping](https://xkcd.com/356/) so I need some way to filter all the OSS project ideas I get. One way of deciding what to pursue is to ask "does this fit in with the portfolio?" Most of my projects are either related to static sites or code generation (the former concept being almost a subset of the latter). Wyam is a static generator, Scripty is a code generator, Buildalyzer is used for building .NET projects during generation, and NetlifySharp is used for talking to a static web host. I have a couple more ideas for projects I'd like to launch in 2018 that fit into this theme.

# Community

## Speaking

My speaking in 2017 slowed down a lot. I only gave one full talk at NoVa Code Camp on [Vue.js](https://vuejs.org), though it was very well received. I also gave a lightning talk at .NET Fringe on Wyam.

To be honest, I'm a little conflicted over this output. On the one hand I made a dedicated effort in the earlier part of the year to shift my speaking from user groups to conferences. Because of my work and family schedules, I often need to select which conferences to submit talks to well in advance, so I don't have the luxury of submitting talks to a bunch of potential events and then attending the ones where I've been selected. I picked 5 conferences of varying size that I thought I might have a decent chance at being selected and submitted talks to each. I was rejected by all of them. Needless to say that stung a little. On the other hand, I find preparing for talks a bit stressful (though ironically for an introvert, I actually enjoy the talking part). Without many talks to prepare for, I found I enjoyed spending that time doing other things like OSS.

I already have two user group talks lined up for early 2018 and I haven't decided yet what kind of speaking I want to focus on: if I want to go back to doing mostly user groups, if I want to give conference submissions another try, or if I want to lay off and focus on other things altogether.

## Blogging

My blogging output was up over 2016 with 17 posts not counting my 2016 year in review. Many of those were either deep technically or provided tutorial style guidance. I hope to continue a similar amount and quality of blog output in 2018.

# Up Next

Here are some goals for 2018:

* Port Wyam to .NET Core and release version 2.0.
* Continue writing technical and tutorial blog posts.
* Submit more PRs to outside projects and to .NET repositories like Roslyn and CoreFx.

Happy 2018 to everyone!