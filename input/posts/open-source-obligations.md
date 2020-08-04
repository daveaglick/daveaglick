Title: Open Source Obligations
Lead: What is required from creators, contributors, consumers, and coders?
Published: 11/26/2014
Tags: 
  - programming
  - open source
---
<p>With the Thanksgiving holiday just around the corner here in the US, we traditionally consider everything that we're thankful for. I have the typical list that includes my family, friends, health, etc. but I've also been thinking about how thankful I am from a professional perspective this year. From the <a href="http://microsoft.github.io/">very top</a> on down, there has never been a better time to be a developer. With abundant conferences, meet-ups, online social networks, resources, and open code, we developers are in the midst of a global community that is larger, more active, and more heterogeneous than ever before. This is certainly true for developers on any technology stack, but <a href="http://news.microsoft.com/2014/11/12/microsoft-takes-net-open-source-and-cross-platform-adds-new-development-capabilities-with-visual-studio-2015-net-2015-and-visual-studio-online/">recent events</a> have made it particularly apparent to those working on the .NET platform. I am specifically thankful for the abundance of open source code and other resources made possible by the many selfless contributions of others. However, there are many different opinions with respect to the way in which we should be participating in the open source processes. These convictions are only strengthened by the fact that there are very real financial, time, and personal implications for those involved. I've seen a number of recent discussions on Twitter and elsewhere regarding the obligations of the various participants and I'd like to examine this issue in a little more detail. This post is going to be very opinionated and I invite you to disagree with me. With open source software becoming increasingly vital to commercial enterprise, it's important to have an open and civilized discussion about these topics.</p>



<h1>The Participants</h1>

<p>Before going any further, let's establish the different kinds of open source participants. I've broken them down into four categories (they all start with "C"! How clever! And cute! And cliché!):</p>

* <strong>Creators</strong><br>These are the developers who create open source projects. For many/most projects they remain the sole contributor and exert a great deal of control over the direction of the project.
* <strong>Contributors</strong><br>These are folks who have contributed to a project in some concrete way. This could be by submitting pull requests, updating documentation, creating web sites, etc.
* <strong>Consumers</strong><br>These are developers who use an open source project.
* <strong>Coders</strong><br>This final group consists of those developers who are outside the scope of open source. They neither contribute to nor use open source software. This group is getting smaller by the day as open source software is migrating into the enterprise and whole technology stacks are converting to open source licensing models.

<p>Keep in mind that most people will fall into multiple categories across different projects. You might be the creator of one open source library, contribute to four others, and use 30 more. If you visualize these different groups, they might look like this:</p>

<img src="/posts/images/open-source-participants.png" class="img-fluid"></img>

<p>Notice how the creators and contributors make up a small corner of the overall developer population whereas the consumers constitute the majority of developers. It's worth noting that this distribution is based only on anecdotal evidence and conjecture. I don't have any actual evidence to back it up. I actually tried to find some statistics on open source participant makeup but couldn't. If anyone knows of any sources or studies, I'd be interested to see them.</p>

<p>The question is: what obligations do each of these groups have to the other groups and to the entire open source community? For example, what <em>must</em> a creator do and what <em>would be nice</em> for them to do? I also think it's very important to understand the distinction between those two things. There are certain obligation that must be fulfilled, such as dealing with the legal issues related to different licensing choices. Then there are those things that everyone (including the person responsible) would like to see done, but that for whatever reason (time, money, complexity, etc.) may not be practical. And there are also things that are just totally unreasonable. Before I go giving you my opinions, I want to stress that that's exactly what these are. There is no "open source manual" that lays out rules for what you should and shouldn't be doing. I suspect that most of this will be common sense, but some of it may be controversial (though thankfully I'm not popular enough to warrant too much controversy - the benefit of shouting at an empty room I guess!).</p>

<p>I'd also like to take a moment to acknowledge the real costs incurred by open source creators and contributors. Actively participating in open source is not easy and almost always requires paying a large price in terms of personal time. In addition, there are often real monetary costs associated with open source such as web hosting fees, domain registration, tooling, etc. What is interesting is the motivation of open source creators and contributors. Why, when presented with such costs, would anyone agree to essentially give the result away? What is it that they hope to get in return for this price? I suspect that there are several motivating factors. Some are driven by the desire to give back to the community and expect nothing more than the satisfaction of having done so. Some are hoping to learn, and for them the process is the reward and not the product. Some are looking to boost their ego or standing within the community, and this is okay too - we shouldn't begrudge someone for using their skills to try and get noticed (not everyone is great at networking). Whatever the motivation, I want to make sure there is a recognition of the sacrifices involved. Whether or not I think a particular group of open source participants is <em>obliged</em> to do certain things, I do think <strong>everyone has a general obligation to treat open source participants with respect at all times</strong>.</p>

<h1>Creators</h1>

<h3>Choose A License</h3>

<p>This is one of those things that I suspect most creators of open source software would probably just prefer not to deal with. It doesn't have anything to do with your code or with functionality and it can be confusing when considering all the possible licenses. It's also necessary. The license conveys, in clear unambiguous (though sometimes complex) language what is expected of those using your software. If you want credit for your work, that should be in your license. If you want to prevent forks, that should be in your license. If you don't really care what people do with your software, that should be in your license. Regardless of what sort of constraints and conditions you want to place on your open source software, you should at least put a little thought into choosing an appropriate license that satisfies them. And if you don't select a license you're essentially placing your software in the public domain, at which point you loose the ability to either enforce or even complain about any use of your software.</p>

<p>Obligation: <strong>Required</strong></p>

<h3>Don't Duplicate Functionality</h3>

<p>I've seen a number of Twitter threads recently that appear to discourage the formation of new open source projects when there is already an exiting project with the same or similar functionality. "Why didn't they talk to me first," "this is ripping off my work," and "they're just re-inventing the wheel" are all common refrains. I can understand and sympathize with these sentiments, even though I don't agree with them. After all, they've put a lot of time and effort into their own project and want to see it adopted and used by as many people as possible. I suspect many may disagree with my thoughts on this issue, but for me it comes down to the freedom inherit in the open source process. We have to understand that there are many reasons why people decide to create and release open source software. Perhaps the creator of the derivative work wanted to build something from scratch as a learning tool. Perhaps they weren't aware of the existing project. Perhaps they were aware but didn't agree with certain design decisions. Even if the intention was only to create a competing project, I really don't see a problem with this. The "competition" should result in more innovation from both projects, and the open source community will eventually pick the one that meets their needs the best. There should not be a "first to market wins" mentality and open source creators should be free to create whatever the heck they want (within the bounds of any licenses that apply).</p>

<p>Obligation: <strong>None</strong></p>

<h3>Fix Bugs</h3>

<p>This is the issue that gets me most aggravated. No matter how big the project, no open source creator should be under any obligation to fix or change anything about their project unless they want to. If there is a lingering bug that is blocking your work, by all means submit a bug report. You are not <em>entitled</em> to a fix however, and the project creator can elect to fix your bug when, and if, they want to. This should be obvious, but I continue to see an entitlement attitude amongst open source consumers when it comes to getting their bugs fixed. That said, as a creator it's certainly a nice thing for your users if you do fix reported bugs.</p>

<p>Obligation: <strong>None</strong></p>

<h3>Merge Pull Requests</h3>

<p>This is different from bug fixes because in the pull request case there has to be a recognition that the submitter has already put in some time and effort of their own to help improve your project. Of course, as a creator you may not agree with their approach or even reject the premise of the pull request. If this is the case, then the creator is certainly free to reject such pull requests. There is no obligation on the part of a creator to accept <em>all</em> pull requests. However, in order to respect the time and energy of the contributor, a creator should at least make a good faith effort to review the pull request and merge it if possible. If time or resources are a problem, the creator could even consider giving frequent contributors more access and permission to the code.</p>

<p>Obligation: <strong>If Possible</strong></p>

<h3>Write Documentation</h3>

<p>This one is a little tricky. On the one hand, open source software is much less valuable to the community without at least a little documentation. On the other, why should a creator be <em>obliged</em> to create such documentation? In fact, pushing for such an expectation as a community may actually restrict available open source code because many creators just wouldn't bother. I think this comes down to motivation. If the creator just created something to learn or for their own use and is "throwing it over the fence" for no other reason than that someone else might be interested, I don't see why they should also have to write documentation. On the other hand, if the motivation of the creator includes gaining adoption of their code, then it's obviously in their best interest to create documentation. In the end, I think it's a good idea but not an obligation on the part of creators.</p>

<p>Obligation: <strong>If Possible</strong></p>

<h1>Contributors</h1>

<h3>Communicate With Creators</h3>

<p>If you are planning on contributing to an open source project (I.e., creating a pull request) it should go without saying that you need to communicate with the creator(s) to be successful. This doesn't necessarily mean that every code contribution be preceded by an email or tweet, just that you have to realize your contribution isn't in isolation. If the creator responds with comments or requests for your contribution, answer them to the best of your ability. Respect the work that has been put into the project already and try to be part of the team.</p>

<p>Obligation: <strong>Required</strong></p>

<h3>Follow Established Conventions</h3>

<p>When contributing to open source, whether through pull requests, documentation contributions, or even bug reports, you must follow the previously established conventions. Even if you like doing things a different way or have a different coding style remember that this is not your project. The creator is under no obligation to accept your contribution (see above) and almost certainly will not if you completely ignore the style and other decisions that they've made for their project.</p>

<p>Obligation: <strong>Required</strong></p>

<h3>Resist Forking</h3>

<p>Forking is when a new project is created using the code base from another project as a starting point. Note that forks are somewhat different than building a derivative or duplicative project (as discussed above) because they are built on the hard work of a previous project as the starting point. There is a difference between creating a fork for your own personal use and creating one with a goal of supplanting the original project. The former is fine whereas the latter raises some red flags. What is the motivation of the fork? Was it created primarily to change leadership of the project or are there differing functional goals? That said, <strong>I don't think there should be any expectation on the part of a creator that their project not be forked</strong> (otherwise, they should have chosen a license that restricted or prohibited such activity). However, forks can be very bad for the open source community at large as they create confusion and can dilute the pool of available contributors. For this reason, and this reason alone, I would think carefully before forking a project. Is there any way to address your concerns within the context of the original project? If not, I don't believe contributors are under any additional obligation not to fork a project. After all, that's one of the underlying premises of open source - it's open.</p>

<p>Obligation: <strong>Avoid If Possible</strong></p>

<h1>Consumers</h1>

<h3>Understand The License</h3>

<p>The license is a legally-binding document that describes exactly what you are and are not allowed to do with the software. It is the sole source of control the creator has over the use of their creation, and it must be understood and adhered to.</p>

<p>Obligation: <strong>Required</strong></p>

<h3>Submit Bug Reports</h3>

<p>I added this item after seeing the following quote on Twitter (I forget by whom): "the license fee for open source is participation - bug reports, contributions, etc." I appreciate and like the sentiment and believe it's an idea vision of how the open source process should work. However, this article is about <em>obligations</em> and I'm not convinced that a consumer of open source should be required to submit bug requests (or do anything else other than adhere to the license). If they prefer to suffer silently with the bugs that they find, or even move on to another library or commercial product, that is their prerogative.</p>

<p>Obligation: <strong>None</strong></p>

<h1>Conclusions</h1>

<p>It's clear to me from re-reading this that I tend to take a very loose viewpoint when it comes to what the participants in an open source community are expected to do. In general, if it isn't covered by the license I think it's fair game. And I'm not just coming from a legal standpoint - the whole reason open source works is because it is so <em>open</em>. Now don't get me wrong, I don't advocate things like forking someone else's project just so you can get impressions on ads without adding value. As I stated in the beginning, there is a general obligation on all of us to treat each other with respect. That said, I think we have to expect and accept that when we participate in the open source process, what we want to happen with our contributions isn't always what does happen. That has to be okay. We can't all be in control of an open process. If that's not okay, then there's always the shareware or closed-source-but-free models.</p>

<p>What do you think? Did I miss any major categories? Do you disagree with any of my conclusions? Let me know in the comments.</p>
