Title: Some Thoughts on Microdependencies, NPM, and Node
Lead: What can we learn from left-pad gate?
Published: 3/24/2016
Tags:
  - open source
  - npm
  - node
  - microdependencies
---
Unless you've been hiding under a rock, you've heard about [the controversy over left-pad](http://www.theregister.co.uk/2016/03/23/npm_left_pad_chaos/) in which a Node developer, angry over a trademark dispute, pulled all of his packages from NPM. It turns out that one of these packages, [left-pad](https://github.com/azer/left-pad), was actually a dependency for a bunch of *other* packages and so builds everywhere came grinding to a halt. Now [everyone](http://www.haneycodes.net/npm-left-pad-have-we-forgotten-how-to-program/) [has](https://developers.slashdot.org/story/16/03/23/0652204/how-one-dev-broke-node-and-thousands-of-projects-in-11-lines-of-javascript) [an](https://medium.com/@mproberts/a-discussion-about-the-breaking-of-the-internet-3d4d2a83aa4d#.fqb2motdz) [opinion](https://medium.com/@robbyrussell/d-oh-my-zsh-af99ca54212c#.3lzvqpv3f) on the topic, many of which are very thought-provoking. I figure what the heck, since everyone else is writing about it I might as well too.

Since the story first broke Tuesday evening, I've been wondering about the bigger implications and what it means for some of the concepts and technologies involved. While this specific situation was obviously concerning for everyone who suffered a broken build, I think the broader implication is that it's left us wondering about the house of cards that's been built underneath many of the open source projects we've come to depend on. In this case, left-pad was deemed *too big to fail*, it was [quickly re-implemented by a different author](http://blog.npmjs.org/post/141577284765/kik-left-pad-and-npm), and [un-un-published](https://twitter.com/seldo/status/712414400808755200). Life (and the builds) went on. But what about the next time? Or what if the package only breaks a handful of builds? Are these *mircodependencies* to blame? Is NPM?

# Microdependencies

Node has an interesting open source community. It's extremely active and as of this writing there are 258,326 packages on NPM (compare that to 52,318 on NuGet). Many of these packages are what you might call microdependencies. That is, they are relatively small and serve a very specific function.

In the fallout from the removal of left-pad, a lot of people pointed the finger at microdependencies and claimed that if developers weren't so reliant on such small single-purpose libraries that maybe this wouldn't have happened. To be sure, the left-pad package seems a little silly in retrospect. I mean, it's basically a one-liner. Why not just write it yourself? I'm not sure it's so simple though...

# No Standard Library

Part of the reason there are so many of these microdependencies is because JavaScript/Node lacks any kind of reasonable standard library. It would be like trying to program in .NET without any of classes in the BCL, just the language syntax and primitive types. To fill this gap the community *created* a standard library and built it out of NPM packages.

This is where I think we need to be careful. For example, as a joke someone published a [left-pad package to NuGet](https://www.nuget.org/packages/left-pad/). Then someone pointed out (helpfully, for those that didn't know) that you don't need a package for this, you can just use the `String.PadLeft()` method:

<blockquote class="twitter-tweet" data-lang="en"><p lang="en" dir="ltr"><a href="https://twitter.com/csharpfritz">@csharpfritz</a> You can also do &quot;Hello&quot;.PadLeft(totalWidth, char)</p>&mdash; Jeff Fritz (@csharpfritz) <a href="https://twitter.com/csharpfritz/status/713004139127652352">March 24, 2016</a></blockquote>
<script async src="//platform.twitter.com/widgets.js" charset="utf-8"></script>

But that's kind of my whole point - in the Node world, there is no `String.PadLeft()` method. Without one Node developers are forced to either implement this themselves (which, yes, wouldn't be that hard *in this one case*) or rely on a package to gain access to such functionality. If I'm building an application I want to focus on the functionality of my application. What I don't want to do is go around implementing hundreds of utility methods. If it's easier to bring in a bunch of very small dependencies to handle all that (because remember, I don't have a standard library), then there's a good chance that's exactly what I'm going to do.

Going even further, **I'm not even sure this is bad**. There's something appealing about the "standard library" for your application being implemented by hundreds or thousands of contributors, constantly updated, and tailored specifically to what you need to do. *As a concept*, there's probably some merit to this. I don't know if it's better or worse than the traditional approach of bundling a standard library with the language, but I don't think we should completely disregard it as silly or bad either.

# NPM

All of this makes me wonder, if microdependencies as your standard library has merit then maybe the problem is with the *implementation* of that concept and not the concept itself? Perhaps the issue here is NPM.

There's a joke that if you want to download the entire Internet, just start a new Node project. NPM is also known for being slow, partly because of the way it has to resolve transitive dependencies for hundreds (or thousands) of packages. For all this, it does work though. And as with any robust package manager there are ways of caching packages locally.

# Final Thoughts

So where am I going with all this? I don't really know. I don't have a conclusion. **What I do know is that I don't want to quickly dismiss the situation Node developers find themselves in, or assume that it's poorly concieved just because it's different than the model I'm used to.**

There are some broad lessons here for developers on any stack too. **Taking dependencies has risks**, whether than dependency is one line or one thousand. **If you don't control your code then someone else does**, and their choices may not be the ones you would make. On the other hand, it's almost impossible to do everything yourself given the complexity of modern languages and systems. I guess the overall message is to **be aware of the risks you're taking via the things that you use and work to mitigate those risks**. At the end of the day, that's not about Node, or dependencies, or even software - it's about good engineering, or maybe even just life.

 

