Title: Development On The Go
Lead: The iOS tools I use to make the most of every minute of downtime.
Published: 3/23/2017
Tags: 
  - programming
  - open source
  - tools
---
Fitting your open source side hustle into a busy schedule can be really hard. It seems like every time you get started on a new feature, doing issue triage, writing documentation, or any of the other multitude of activities that keep an OSS project healthy you're pulled away by family, work, or other personal obligations. I've always had a family-first mentality, so for me creating a healthy balance means making the most of every minute. More often than not, that means doing whatever work I can wherever I happen to get a free moment to do it. Not everything requires Visual Studio and a compiler. Creating documentation, responding to issues, reviewing pull requests, and writing blog posts can all be performed from your phone. In fact, I'm writing *this* blog post from my phone right now. Here's the tools I use to enable this kind of mobile open source workflow. Before I begin though, it's worth noting that both my phone and tablet are iOS so that's what I'm going to be writing about. There are probably very good counterparts on Android and Windows Phone, I'm just not aware of them.

# Git Access
---

## Working Copy

* [Web Site](https://workingcopyapp.com)
* [App Store](https://itunes.apple.com/us/app/working-copy-powerful-git-client/id896694807)

<img src="/posts/images/working-copy.png" class="img-fluid"></img>

Working Copy is my number one go-to development app. It allows you to clone any git repository and provides everything you might expect including clone, push/pull, merge, rebase, and log. It includes an integrated folder browser and file editor that has syntax editing and preview for a number of different formats including Markdown and C#. I've found the experience of creating or editing a Markdown file on my phone and then pushing it into a repository that's rigged for CI/CD to be magical. It also works great on both my phone and my tablet.

## Clone

* [Web Site](http://clone.hammockdistrict.com)
* [App Store](https://itunes.apple.com/us/app/clone-git-client-advanced/id1037881290)

I've experimented with Clone which works similarly to Working Copy. In my early trials it seemed a little less feature rich, but might provide a good alternative if you want to try something else.

# GitHub
---

## CodeHub

* [Web Site](http://codehub-app.com)
* [App Store](https://itunes.apple.com/us/app/codehub-a-client-for-github/id707173885)

<img src="/posts/images/codehub.png" class="img-fluid"></img>

While Working Copy is my go-to app for working with repository content, CodeHub is equally as important for working with all the other stuff that goes along with a GitHub repository. It's great at things like managing issues, reviewing pull requests, and commenting. As a bonus, it's also [open source and written in C#](https://github.com/thedillonb/CodeHub).

## iOctocat

* [Web Site](https://ioctocat.com)
* [App Store](https://itunes.apple.com/us/app/ioctocat-mobile-client-for-github/id669642611)

iOctocat is another excellent GitHub client. It does basically everything CodeHub does and the choice between the two probably comes down to personal preference.

# Text Editing
---

## Textastic

* [Web Site](https://www.textasticapp.com)
* [App Store](https://itunes.apple.com/us/app/textastic-code-editor-6/id1049254261)

<img src="/posts/images/textastic.png" class="img-fluid"></img>

While Working Copy already has a fantastic editor, sometimes you need a little more power. Textastic provides format-specific interfaces for editing a variety of file types. It's Markdown support is great and includes a cool little special character selector mechanism that makes writing code blocks and other Markdown syntax really easy. It integrates with other apps, so it's easy to open a repository in Working Copy, send a file to Textastic for editing, and then save it back into the repository.

## Coda

* [Web Site](https://panic.com/coda-ios/)
* [App Store](https://itunes.apple.com/us/app/coda/id500906297)

Coda is an interesting app. It started out mostly as a text editor geared towards coding, but has since become something of a lightweight coding environment (more on other apps like this below). As with Textastic it offers format-specific editing for a variety of file types. It also includes other features like file synchronization and in my opinion suffers a little from feature bloat. If one-app-to-rule-them-all is your thing though, check it out as an alternative editor.

# Coding
---

## Continuous

* [Web Site](http://continuous.codes)
* [App Store](https://itunes.apple.com/us/app/continuous-net-c-and-f-ide/id1095213378)

<img src="/posts/images/continuous.png" class="img-fluid"></img>

Continuous is something of a miracle. Frank Krueger essentially forked Roslyn and changed it so that code could be compiled within the strict restrictions of an iOS app. The result is an amazing C# and F# IDE that works great on phones and really shines on tablets. It's got features you might expect from a desktop IDE like auto-completion and it's hard to explain how awesome it feels when that little suggestion window pops up on your phone.

## TryRoslyn

* [Web Site](https://tryroslyn.azurewebsites.net)

This isn't an app, but did you know TryRoslyn has a responsive web interface that works great on your phone? I didn't until I happened to follow a link to it from somewhere and now I constantly use it to test out little snippets.

# Other
---

## iSource

* [App Store](https://itunes.apple.com/us/app/isource-browser/id370764473)

Sometimes you want to take a look at the source behind a web page. This is super-easy on most desktop browsers, but surprisingly hard on mobile. Thankfully this app does exactly that. It's essentially a normal iOS browser but with the added capability of letting you take a look at the source and HTTP headers of the page you're viewing.