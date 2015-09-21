Title: Announcing @dotnetissues
Lead: A Twitter bot to keep you up to date with important .NET repositories.
Published: 9/21/2015
Tags:
  - Twitter
  - open source
---

As more and more of the .NET framework, runtime, etc. get put in the open on GitHub, the community has the opportunity to participate in the day-to-day evolution of core parts of our platform. However, given the large number of projects (which is great!) and the volume of interaction, it can be hard to keep up. This is especially true because of the all-or-nothing notification model in GitHub. If you watch a repository, you will get notified of every since action including comments, commits, etc. What I wanted was a way to get notified of *new* issues so that I could keep an eye on things and then elect to subscribe to additional notifications for just the ones that interested me.

To achieve this, I created [a Twitter bot](https://twitter.com/dotnetissues) that scans core .NET respositories and tweets all new issues:

<blockquote class="twitter-tweet" lang="en"><p lang="en" dir="ltr">aspnet\Configuration#292 <a href="https://t.co/KfVNvpgrUr">https://t.co/KfVNvpgrUr</a>&#10;Add `Exists` to IConfigurationSection</p>&mdash; DotNet Issues (@@dotnetissues) <a href="https://twitter.com/dotnetissues/status/645925456026406912">September 21, 2015</a></blockquote>
<script async src="//platform.twitter.com/widgets.js" charset="utf-8"></script>

It currently looks at the following repositores (let me know in the comments if I should add any additional ones):

- [dotnet/roslyn](https://github.com/dotnet/roslyn)
- [dotnet/coreclr](https://github.com/dotnet/coreclr)
- [dotnet/corefx](https://github.com/dotnet/corefx)
- [aspnet/dnx](https://github.com/aspnet/dnx)
- [aspnet/Routing](https://github.com/aspnet/Routing)
- [aspnet/Hosting](https://github.com/aspnet/Hosting)
- [aspnet/Identity](https://github.com/aspnet/Identity)
- [aspnet/Razor](https://github.com/aspnet/Razor)
- [aspnet/Security](https://github.com/aspnet/Security)
- [aspnet/Mvc](https://github.com/aspnet/Mvc)
- [aspnet/EntityFramework](https://github.com/aspnet/EntityFramework)
- [aspnet/DependencyInjection](https://github.com/aspnet/DependencyInjection)
- [aspnet/Logging](https://github.com/aspnet/Logging)
- [aspnet/Configuration](https://github.com/aspnet/Configuration)
- [aspnet/Announcements](https://github.com/aspnet/Announcements)
- [NuGet/Home](https://github.com/NuGet/Home)

This lets you monitor issue creation in your Twitter timeline (which is great for me since I usually have it open during the day anyway). It also has the side benefit of making it easy to tweet about new issues by quoting from the bot.

To achieve an even greater level of automation, I currently have an [IFTTT](https://ifttt.com/) recipe configured that emails me whenever the bot tweets. This workflow is working great for me. I get email notifications for new issues, I can open them if they look interesting, and then subscribe if I want to keep up to date with them. This provides exactly the level of notification I was looking for.

Under the hood, the bot is running as an [Azure WebJob](https://azure.microsoft.com/en-us/documentation/articles/web-sites-create-web-jobs/) via the [Azure Scheduler service](http://azure.microsoft.com/en-us/services/scheduler/) at 5 minute intervals. It uses [LinqToTwitter](https://github.com/JoeMayo/LinqToTwitter) for Twitter REST API access and [Octokit](https://github.com/octokit/octokit.net) for GitHub API access. Thanks also to [this blog series on LinqToTwitter](https://www.dougv.com/related/linq2twitter/) by Doug Vanderweide, it was very helpful in getting up and running with the Twitter API quickly.

Code for the bot is general, so you could use it to do something similar for any repository. [IssueTweeter is hosted at GitHub](https://github.com/daveaglick/IssueTweeter) (naturally). Just let me know if anyone is interested in using it for another bot, and I'd be happy to help set it up.