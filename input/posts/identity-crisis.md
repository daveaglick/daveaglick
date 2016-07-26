Title: Identity Crisis
Lead: Changing your name in the digital age.
Published: 3/19/2015
Tags:
  - personal
---
<p>Inspired by notable tech personalities like <a href="http://www.hanselman.com/">Scott Hanselman</a> and <a href="http://simpleprogrammer.com/">John Sonmez</a> who place a lot of emphasis on "personal brand", I've recently been thinking about my own personal brand. Because the username I used was common enough that I wasn't always the first to claim it, I ended up with a lot of slightly different usernames across different platforms. I was <code>somedave</code> on GitHub and Stack Overflow, <code>@somedaveg</code> on Twitter, <code>somedavedg</code> on Reddit, etc. and my website was <code>somedave.com</code>. I'm not nearly notable enough for this to make much of a difference (fun experiment, try Googling just "Scott"), but it bothered me none the less. When someone puts my username in a GitHub issue, I want that to also point to my Twitter handle, etc. Not to mention, my various usernames didn't really identify <em>me</em>. There was no real indication of who was behind them other than someone named Dave. And maybe one day, it actually will make a difference. So I set out to set things right.</p>

<p><strong>I am now known everywhere as <code>daveaglick</code> and my site is at <code>daveaglick.com</code>.</strong></p>

<p>Granted, it's a little long. I obviously would have preferred something more concise like <code>dglick</code> or <code>daveglick</code>, but I'm a little late to the Twitter handle party at this point and I really wanted to find a username that was available everywhere. I had been thinking about this for a while; it wasn't a spur of the moment decision. I had planned which services I needed to change, made sure the name was available everywhere, and then changed it in a deliberate sequence. In case you're thinking about doing the same thing, here's the steps I took.</p>

<h3>Find a Name</h3>

<p>First I went to <a href="http://checkusernames.com/">http://checkusernames.com/</a> to verify the name was available on a wide range of services. There are other sites like this, but they all do essentially the same thing by checking for username availability across a wide spectrum of social and other sites. I also cross-checked with a domain registrar (I use Namecheap) to make sure I could also get the <code>.com</code> and <code>.net</code> of the same name. I focused on names that were both consist across sites and domains and that included my own name clearly.</p>

<h3>Set Up Email</h3>

<p>Once I had found a name that I was sure would be available, I proceeded to register the domains and set up a new email address. I figured this was important to do first because as I change profiles at the various sites I'll need to input my new email address. I use Exchange Online, and thankfully it supports multiple domains and multiple email addresses per user so getting this set up to support both my old address and my new one was pretty easy. I wanted to go all the way, so I even changed my primary account name with Office 365 and deleted my email profiles from a my desktop and mobile devices and then added them back with my new account name.</p>

<h3>Migrate My Website</h3>

<p>I felt it was important to also get my new website up and running before pointing my social profiles in that direction. The hardest part of this process was recoding the site. Since my site is all custom code, I had to scrub it for any uses of my old name and replace it with my new one where appropriate. This wasn't too bad, and using a simple file content search helped a lot. I also renamed the Visual Studio solution and project files, changed the default namespace, etc. so that the code reflected the new home. I do use Disqus, so I had to change that as well. While I could change my Disqus username without a problem, and they even provide some handly domain migration tools, you can't change the shortname (their version of a site id) for an existing site. That part had to stay as-is in the code, but since it's in JavaScript, no one will see it and it should be invisible. Then I created a new site in Azure, directed my new domain to it, and republished the site. No surprises.</p>

<p>The second part of publishing the new site was setting up redirects for all the old ones. Namecheap makes it easy to do this, but you have to remember to add a trailing slash to the redirect destination otherwise the full path won't come along for the ride. I also had a redirect set up with Wordpress for my old blog, so I changed that too for good measure. It still would have worked if I hadn't by redirecting from Wordpress to my old domain and then to my new domain, but one redirect is better than two. I used 301 redirects so that search engines pick up on the change and should adjust accordingly.</p>

<h3>GitHub</h3>

<p>My next migration was GitHub. This one had me the most nervous because I'm starting to get some traction for some of my projects and I was worried about the implications. However, GitHub has a very nice policy of redirecting repos and other content when you change your username. This isn't permanent, and they warn that it'll stop if someone registers your old name and then created repositories with similar names, but it's as good as could be expected. Changing my GitHub username was easy, but updating all of the repositories on my local system to point to the new address and use my new email address was time consuming. You don't have to strictly do this step (because of the redirects), but I felt like it was better to do it now and get it over with then have potential confusion later.</p>

<h3>Twitter</h3>

<p>Next up was Twitter. Changing a username on Twitter is also really easy, and your followers and other profile information stay intact, but it doesn't "redirect" older mentions to your new handle. Since Twitter now indexes everything and provides longer-term search, who knows when an old Tweet might come up that references your old handle? My solution to this was to register my old Twitter handle as a new account soon as I changed my existing profile to the new handle. Then I could add some information to the profile for the old handle pointing people to my new one. Again, probably not necessary given my relative obscurity, but hey - why not?</p>

<h3>Other Sites</h3>

<p>Then there were a bunch of other sites: Goodreads, Stack Overflow, Gravatar, LinkedIn, Facebook, etc. I was pleasantly surprised to find that most sites provide capabilities for changing your identifier without much hassle. I did have trouble with a few though. Codeplex just doesn't let you change it, in fact their profile options are basically non-existent. Reddit doesn't let you change your username either, and their recommendation is to create a new account and loose all your karma, etc. Thankfully I'm not a big Reddit user, so this wasn't a big deal. Also, NuGet doesn't have a facility to do this, but <a href="https://github.com/NuGet/NuGetGallery/issues/319">there's a GitHub issue suggesting you can contact them</a>. My request for a change is pending.</p>

<h3>Not Quite Done</h3>

<p>It took me the better part of an afternoon, but at this point I'm almost there. I still need to search the web, Stack Overflow, and other forums I participate in to locate links that point to my old GitHub profile or web site and edit them to point to the correct place. I'm also sure I'll think of some other clean up to do as well, but for the most part this wasn't too complex. It just took some planning and time.</p>