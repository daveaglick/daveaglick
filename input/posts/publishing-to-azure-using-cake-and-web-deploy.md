Title: Publishing To Azure Using Cake And Web Deploy
Lead: Scripted deployment synchronization for static sites
Published: 4/4/2017
Image: /images/nightcars.jpg
Tags:
  - Azure
  - Cake
  - devops
---
As you may know, I am a [big fan of static sites](https://wyam.io) and am always interested in new ways to manage and deploy them. I previously blogged about [using FTP to synchronize files with Azure](/posts/synchronizing-files-with-azure-web-apps-over-ftp) and this post explores an alternate way to do something similar using Web Deploy.

When you publish a web application from inside Visual Studio, you're often using a technology called [Web Deploy](https://www.iis.net/learn/publish/using-web-deploy) under the hood. Web Deploy is both a server and a client technology, meaning the server must also support it. It's typically supported on IIS, which means the Azure App Service and Web Apps also supports it. While these instructions could be easily applied to deploying a static site to any IIS server with Web Deploy support, I'm going to focus on Azure from here on out.

To use Web Deploy you'll need a couple bits of information including your publish endpoint (usually a URL), your username, and your password. This information is automatically downloaded into Visual Studio when you configure a publish profile, but we want to access it directly. To find it, click the "Get publish profile" link at the top of the Overview page for your Azure Web App:

<img src="/posts/images/get-publish-profile.png" alt="Get publish profile" class="img-responsive" style="margin-top: 6px; margin-bottom: 6px;">

This will download an XML file with the information you need (even though it's named `[sitename].PublishSettings`). The file will look something like this:

```xml
<publishData>
  <publishProfile
    profileName="sitename - Web Deploy"
    publishMethod="MSDeploy"
    publishUrl="sitename.scm.azurewebsites.net:443"
    msdeploySite="sitename"
    userName="$sitename"
    userPWD="abc123somereallylongstringhere"
    destinationAppUrl="http://sitename.azurewebsites.net"
    SQLServerDBConnectionString=""
    mySQLDBConnectionString=""
    hostingProviderForumLink=""
    controlPanelLink=""
    webSystem="WebSites">
    <databases />
  </publishProfile>
</publishData>
```

You can ignore most of that information. What we're primarily looking for is the value of `msdeploySite` (which should be the same as your site in Azure), `userName` (again, typically the same as your Azure site name with a `$` prefix), and `userPWD`.

Once you have this information you have what you need to call the Web Deploy libraries. One way to work with Web Deploy is [via PowerShell](https://www.iis.net/learn/publish/using-web-deploy/powershell-scripts-for-automating-web-deploy-setup) but I love [Cake](http://cakebuild.net/) so that's what we're going to use here. Specifically, we're going to use a Cake addin called [Cake.WebDeploy](https://github.com/SharpeRAD/Cake.WebDeploy) by Phillip Sharpe. It wraps the Web Deploy libraries and makes them easy to use from inside a Cake build script.

Your Cake build script should look something like:

```
#addin "nuget:https://api.nuget.org/v3/index.json?package=Cake.WebDeploy"
var siteName = "my-azure-sitename";
var deployPassword = EnvironmentVariable("DEPLOY_PASSWORD");

// ...

Task("Deploy")
    .Does(() =>
    {
        DeployWebsite(new DeploySettings()
        {
            SourcePath = "./output",
            SiteName = siteName,
            ComputerName = "https://" + siteName + ".scm.azurewebsites.net:443/msdeploy.axd?site=" + siteName,
            Username = "$" + siteName,
            Password = deployPassword
        });
    });
```

Notice that `SourcePath` in the deploy settings is pointing to a folder. Whatever files exist under that folder will be synchronized with Azure. The really nice thing here is that Web Deploy knows how to compare files so it doesn't have to upload everything if only a few files changed. This is also great for static sites because you can just put whatever folder the output of your static generator uses and it'll get published.

Another thing to make notice of is the `ComputerName`. I tried all sorts of things for this property and never could get the right URL. Thankfully, [Mattias Karlsson](https://twitter.com/devlead) came to the rescue and gave me the magic URL to use.

Finally, note that I'm storing the password I got from the publish settings file I downloaded as an environment variable. That keeps your credentials out of the build script, which you'll probably be checking into source control.

And there you have it. Easy synchronization of a set of statically generated files to an Azure Web App.