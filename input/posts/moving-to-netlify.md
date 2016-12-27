Title: Moving To Netlify
Lead: Why I'm moving my blog and other sites to Netlify.
Published: 12/27/2016
Image: /images/truck.jpg
Tags:
  - meta
---
There seems to be an increasing enthusasim for static sites recently. If you're not familiar with the term, a static site is one that consists only of resources that can be delivered directly to a client such as HTML pages, images, CSS files, etc. In other words, static sites don't require any sort of compilation or interpretation on the server. To satisfy this interest, many new static site hosts have sprung up. These hosts specialize in delivering static sites without any ceremony quickly and efficiently, often by pushing some of the resources to geographically distributed content delivery networks or otherwise optimizing delivery for static resources. Perhaps the most well known of these hosts is [GitHub Pages](https://pages.github.com/). I recently moved this blog and my other sites such as [Wyam](https://wyam.io) from there to [Netlify](https://www.netlify.com/), another such host. Normally a site host change wouldn't really be cause for a whole post. In this case, however, I feel that spreading the word about Netlify may help other folks who are looking for a good static site host. I swear this isn't a sponsored post or anything (though they do have an awesome free plan, so I'll admit to an interest in paying it forward).

# Reasons

There are a [number of reasons](https://www.netlify.com/features/) I've had my eye on Netlify:
- Several easy deployment options
- One-click HTTPS (which I plan to turn on very soon)
- Support for redirects with custom status codes
- Integrated forms processing
- Staging sites tied to git branches

But perhaps most importantly, I like the company. [Netlify has been very supportive of open source](https://www.netlify.com/blog/2016/12/22/an-open-source-tale/) and maintains or contributes to [a number of open source projects](https://www.netlify.com/open-source/). In addition, the founders have been heavily involved in the static site community and in my opinion are at least partly responsible for it's surge in interest by creating resources like [StaticGen](http://www.staticgen.com/) and [JAMStack](https://jamstack.org/). Since I also have [a strong interest in static sites and static generation](https://wyam.io) as well as open source, the corporate vision at Netlify aligns closely with my own.

Finally, I also have a long-term plan of integrating [Wyam](https://wyam.io) with the Netlify toolchain. While their service is capable of hosting static sites from any generator by uploading the output of a generation process, it also has some more advanced support for continuous integration pipelines like automatically building in a container after commit. These types of tools are like the Jekyll builds that GitHub Pages does but super-powered and with support for more static generators. I would love to see Wyam as one of the supported options once it supports cross platform .NET Core next year.

# Process

Moving hosts for a static site is generally an easier process than changing hosts for a more server-dependent site. Usually all you have to do is upload the static resources somewhere and change your DNS settings.

When you first set up a new site, there's a huge box right on their admin page that tells you to drag-and-drop your files.

<img src="/posts/images/netlify.png" class="img-responsive"></img>

This works exactly as intended and gets you up and running quickly. Alternativly, if you'd like to make updating the site part of your build process, you can [upload a new version of your site as a zip file](https://www.netlify.com/docs/api/#deploying-to-netlify) to a particular endpoint and Netlify will automatically unzip it and deploy it for you. The upload can even be done in a single curl command. There's also a [Node-powered CLI tool](https://www.netlify.com/docs/cli/) that can be used to upload a site and perform other maintenance activities.

I ended up using the zip file upload process since it was easy to script from a [Cake Build](http://cakebuild.net/) script and now my site is automatically built using [Wyam](https://wyam.io) by [AppVeyor](https://www.appveyor.com/) and deployed on every commit.

Changing DNS settings was also easy and they have a number of different [supported configurations](https://www.netlify.com/docs/custom-domains/) depending on how your DNS is set up.

In the near future I intend to use their HTTPS support to enable SSL on all my sites. I'm also planning at looking at their staged deployment support to improve my continuous integration workflow. If you're currently using GitHub Pages, Azure, or some other host for your static site, I'd encourage you to at least take a look at what Netlify might offer you.