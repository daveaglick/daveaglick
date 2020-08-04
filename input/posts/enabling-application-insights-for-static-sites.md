Title: Enabling Application Insights for Static Sites
Published: 5/23/2017
Image: /images/lightbulb2.jpg
Tags:
  - Azure
  - devops
  - static site generator
---
Azure has a really cool service called [Application Insights](https://azure.microsoft.com/en-us/services/application-insights/). It lets you instrument both the server and the client for all kinds of metrics and data. Unfortunately, all the documentation about how to enable it makes a lot of assumptions, like [having Visual Studio](https://docs.microsoft.com/en-us/azure/application-insights/app-insights-asp-net#before-you-start) as part of your tooling. I wanted to turn on Application Insights for [a static site that I was hosting on Azure App Server](https://wyam.io/docs/deployment/azure) and literally could not find a single guide on how to do so. I finally got it working through trial and error and came up with this set of hacks. Keep in mind, this really is a bit of a hack - I'm sure there's a better way, I just don't know what it is. That said, maybe this will help someone else in the same situation.

# Enable Application Insights

The first step is to create an Application Insights account from the Azure Portal. Find the Application Insights service...

<img src="/posts/images/appinsights1.png" class="img-fluid"></img>

 and then add a new Application Insights application.

<img src="/posts/images/appinsights2.png" class="img-fluid"></img>

# Connect Application Insights To Your App Service

Next you'll need to connect your new Application Insights application to your App Service:

<img src="/posts/images/appinsights3.png" class="img-fluid"></img>

# Install The Application Insights Extension

This is where things get a little tricky. If we were building and deploying a normal ASP.NET web site, there is [a very good guide](https://docs.microsoft.com/en-us/azure/application-insights/app-insights-asp-net) on how to instrument the site with Application Insights. But we're not. Instead, we're going to instrument our live static site using an App Service Extension:

<img src="/posts/images/appinsights4.png" class="img-fluid"></img>

Once you've opened the extensions panel, select "Add" and then find the Application Insights extension. Once you've added the extension and it has time to install, it'll create a whole bunch of new folders and files within your App Service site.

# Download Files From wwwroot

A typical static site [will deploy](/posts/publishing-to-azure-using-cake-and-web-deploy) to the `wwwroot` folder in the App Service. When the Application Insights extension was installed, it changed and added some files to this folder (as well as others further up the App Service folder tree). We need to preserve these changes within our static site so that the next upload deployment doesn't destroy them.

To view all the files in your App Service without using FTPS (though you can also use FTPS for this part if you want), go to "Advanced Tools":

<img src="/posts/images/appinsights5.png" class="img-fluid"></img>

Then open the debug console:

<img src="/posts/images/appinsights6.png" class="img-fluid"></img>

It will show a listing of your entire App Service site. Using the file tree, browse to `/site/wwwroot`. From there you'll need to download `ApplicationInsights.config` and `web.config`:

<img src="/posts/images/appinsights7.png" class="img-fluid"></img>

Copy both files into your static site (presumably you already have a `web.config`, just overwrite it since this one is a merge of what you previously deployed combined with the new Application Insights stuff the extension added).

You'll also need to download the entire `App_Data` and `bin` folders with all of their contents. As with the previous files, copy those folders and their files to your static site. The goal is for all of this to be output during your static generation process so it gets re-uploaded to Azure on your next deployment.

# Verify

Run a generation and deployment to verify everything worked. If it did, you should see live metrics in your App Service under the Application Insights blade (with the option to open Application Insights directly to see more).