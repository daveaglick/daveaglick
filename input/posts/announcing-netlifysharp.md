Title: Announcing NetlifySharp
Lead: A .NET API client for Netlify.
Published: 12/7/2017
Image: /images/plasma.jpg
Tags:
  - static site generator
  - Netlify
  - open source
---
[Netlify](https://www.netlify.com/) is an awesome static site host with tons of developer-friendly features including a [comprehensive API](https://www.netlify.com/docs/api/). Today I'm announcing [NetlifySharp](https://netlifysharp.netlify.com/), a new .NET API client for that API that lets you control your Netlify sites from .NET.

Building NetlifySharp was a fun experience for me. I've written plenty of REST client code, but I've never written a .NET client for a REST API that's designed to be consumed by other developers. Questions like "how can I allow customization?" were interesting to ponder. In the end I think I struck a pretty good balance between exposing an easy to use API while allowing more control if desired.

One aspect of this project that was mostly new to me is Open API. I was familiar with the concept but hadn't gone much beyond that. Since Netlify publishes an [Open API specification](https://open-api.netlify.com/) it was the perfect opportunity to learn. I ended up doing more than that though - as I got tired of writing model code for the API request and response bodies, I ended up using [Scripty](https://github.com/daveaglick/Scripty) to write a lightweight Open API parser to generate the model definitions for me from the specification. It's certainly not general enough to release as an all-purpose Open API client generator (we already have [NSwag](https://github.com/RSuter/NSwag) and [AutoRest](https://github.com/Azure/autorest) for that) it served my purposes quite well.

Regarding [NSwag](https://github.com/RSuter/NSwag) and [AutoRest](https://github.com/Azure/autorest), I looked at both to see if they would fit my needs. Why spend time writing a client API when you can issue a few commands and generate one? Unfortunatly, neither really felt natural to me. I'm sure the clients would have worked fine but they were lacking that personal touch that comes from a bespoke artisinal API.

# Usage

All operations are performed through a `NetlifyClient` instance. Use [a personal access token](https://app.netlify.com/account/applications) to create the client:

```
NetlifyClient client = new NetlifyClient("123456789abcdef");
```

The `NetlifyClient` contains methods for each of the endpoints. All endpoints are asynchronous and use a fluent interface. You must call `.SendAsync()` to initiate communication with the Netlify API.

For example, to get a list of all configured sites for the account:

```
IEnumerable<Site> sites = await client.ListSites().SendAsync();
```

To create a new site:

```
Site site = await client
    .CreateSite(
        new SiteSetup(client)
        {
            Name = "mynewsite"
        })
    .SendAsync();
```

You'll notice that the `NetlifyClient` instance was required in the `SiteSetup` constructor in the above example. All models require the client to be provided when directly instantiating them. If they're created as a result of an API call (like the sites in the first example) then the client is already set. This is so every model can initiate their own API requests through the client.

For example, to delete an existing site:

```
Site deleteme = await client.GetSite("sitetodelete.netlify.com").SendAsync();
await deleteme.DeleteSite().SendAsync();
```

More information is available [in the docs](https://netlifysharp.netlify.com).

# Current Status

Right now only the sites endpoints are implemented. That means you can query for sites, create and delete site, upload deployments, and perform other site operations. The other endpoints will be coming soon.

# But, Why?

<iframe src="https://giphy.com/embed/1M9fmo1WAFVK0" width="480" height="270" frameBorder="0" class="giphy-embed" allowFullScreen></iframe><p><a href="https://giphy.com/gifs/why-ryan-reynolds-1M9fmo1WAFVK0">via GIPHY</a></p>

So a legitimate question you may have at this point is why build something like this at all? If there's a REST API and a CLI, why do we even need a .NET client? The answer is automation. The Netlify API offers a lot of interesting functionality like creating and deleting sites on the fly, reading form submissions, deploying new versions, etc. By using NetlifySharp we can add these capabilities to a [static site generator](https://wyam.io) or [build automation tool](https://cakebuild.net).

For example, the use case I currently have in mind (and the reason I wrote this now) is to create new Netlify sites on demand as part of a complex documentation generation process that publishes documentation for different versions of a library. As the documentation generation process is fully automated, it can check if a site for a given version already exists and if it doesn't we can generate the documentation, create the Netlify site to host it, and deploy it all from a build script.

Another interesting use case I'm considering is using the [Netlify form handling](https://www.netlify.com/docs/form-handling/) capabilities to replace the commenting system on my static blog. The static generation process could use NetlifySharp to query for form submissions and generate comments from them.

If you end up doing something cool with NetlifySharp, let me know!
