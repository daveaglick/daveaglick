Title: Wyam 1.0
Lead: Over two years in the making.
Published: 9/14/2017
Image: /images/fireworks.jpg
Tags:
  - Wyam
  - static site generator
  - open source
---
After a little over two years of development, I'm thrilled to announce that Wyam 1.0 is now released! [Wyam is a .NET static generator](https://wyam.io) that unapologetically places an emphasis on flexibility and extensibility. It's the static generator *I* wanted two years ago and I'm thrilled that so many have also found it valuable and interesting along the way.

Here are some numbers (as of the writing of this post):

* **1337** (not kidding!) commits
* **37** contributors
* **442** issues reported on the project
* **123** pull requests
* **461** GitHub stars
* **112** GitHub forks
* **38** ranking on [StaticGen](https://www.staticgen.com)

# Community

One of the things I'm most proud of is the community that's formed around the project. We have an active [Gitter room](https://gitter.im/Wyamio/Wyam), issues and feature discussions regularly have good engagement, and I've made a ton of friends working on it. One of my goals going forward is to continue to expand and grow the community beyond the active but moderate size it is today.

# What Does 1.0 Mean?

More than anything, it means that I'm comfortable with the architecture and stability of Wyam. It's had two years of usage and iteration and I think it's finally time to close the book on this chapter of development. That's not to say there aren't any more bugs, I'm sure there's lots, but we seem to have found and fixed all the ones that were regularly bothering people.

# Versioning Going Forward

While I appreciate semantic versioning and everything it tries to accomplish, I've never been a purist. That's especially true since any change could potentially be a breaking change and deciding which version place to increment can be a real challenge. Leading up to 1.0 I often broke the API regularly at first, and then less and less frequently as it got closer. Now that this milestone has been reached, I intend to keep breaking changes to a minimum and deprecate functionality (but not remove it) as required. I'll also stick close to the sprit of semantic versioning, if not follow it exactly. In other words, patches shouldn't break anything, minor release probably won't break anything, and major release probably will break stuff. All that said, I also think there's a value in making major release have some external meaning. In other words, I may increment the major release number when some important milestone is reached, even if no breaking changes have occurred.

# What's Next?

I'll continue to fix bugs, add new features, merge PRs, etc. but my primary focus for the entire 1.0 life cycle is going to be porting to .NET Core. All Wyam extension libraries will get ported to .NET Standard and a new set of runtime applications will be developed that target the .NET Core runtime. I may or may not also keep a console application that also targets .NET Framework. I'll almost certainly come up with a new deployment and update mechanism that doesn't use [Squirrel](https://github.com/Squirrel/Squirrel.Windows) (it's been fine, if not a little flaky, but I want something fully cross platform). I don't have any hard dates, but I'd like to get this done quickly. 2.0 will land once all the .NET Standard and .NET Core activity is complete.

Beyond that I have a lot of *big plans*. There's huge potential for integration and tooling around static site generation and I'd like to focus some effort in that area. There's also a [huge backlog](https://github.com/Wyamio/Wyam/issues) of awesome ideas for small and large enhancements to Wyam itself.

# Thank You

Finally, I'd like to take a moment to say "thank you" to everyone who's been involved in this project so far. Whether you've written about Wyam, used it, filed issues, contributed code, or just bounced ideas in the Gitter room, you're all a valued member of this community. For the last two years I've dedicated myself to this project, bordering on obsession. Not a day goes by that I don't work on it or at least think about it. Even if I were the only one interested I would still be building it, but I can't express what a comfort it is knowing that all this effort is interesting and helpful to others.