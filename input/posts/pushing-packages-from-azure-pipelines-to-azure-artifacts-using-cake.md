Title: Pushing Packages From Azure Pipelines To Azure Artifacts Using Cake
Published: 11/6/2018
Image: /images/cake.jpg
Tags:
  - Azure
  - Cake
  - devops
  - NuGet
---
This is a short post about using [Cake](https://cakebuild.net) to publish packages from [Azure Pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines) to [Azure Artifacts](https://azure.microsoft.com/en-us/services/devops/artifacts) that took me the better part of a day to figure out. For completness I'll walk through my entire process but if you just want to know how to do it, skip to the end.

I've been a very happy user of [AppVeyor](https://www.appveyor.com/) and [MyGet](https://www.myget.org/) for my open source work. At my day job we use an on-premesis [Bamboo](https://www.atlassian.com/software/bamboo) server which also sends packages to MyGet. In both cases, publishing a package from a Cake build script is relativly straightforward and basically involves getting an API key from MyGet and feeding that to the Cake [`NuGetPush`](https://cakebuild.net/api/Cake.Common.Tools.NuGet/NuGetAliases/08163C34) alias. Now that I'm investigating moving some of the workloads at my day job to [Azure DevOps services](https://dev.azure.com/), I'm finding this simple task isn't so straightforward.

# Personal Access Token

My first attempt was to get an [Azure DevOps personal access token](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate) with package management grants and feed that to the `NuGetPush` Cake alias just like I was used to doing with MyGet. Unfortunatly that resulted in error messages that look like this:

```
Unable to load the service index for source https://pkgs.dev.azure.com/xyz/_packaging/xyz/nuget/v3/index.json.
Response status code does not indicate success: 401 (Unauthorized).
```

After that, I took the most resonable first troubleshooting step and...posted to Twitter:

<blockquote class="twitter-tweet" data-partner="tweetdeck"><p lang="en" dir="ltr">Good grief. I always feel so dumb when trying to do anything with Azure. Can&#39;t figure out how to push a package from Azure Pipelines to Azure Artifacts using an API key. And no, I don&#39;t want to setup and use a special credential provider. This was so easy with AppVeyor/MyGet.</p>&mdash; Dave Glick (@daveaglick) <a href="https://twitter.com/daveaglick/status/1059801965415272448?ref_src=twsrc%5Etfw">November 6, 2018</a></blockquote>

Unfortunatly the answer wasn't what I wanted to see:

<blockquote class="twitter-tweet" data-conversation="none" data-cards="hidden" data-partner="tweetdeck"><p lang="en" dir="ltr">Thanks for the feedback. There are some under-the-hood reasons why we don&#39;t support apikey. But, Azure Pipelines has a &quot;NuGet&quot; task (or &quot;.NET Core&quot; if you prefer) that will automatically authenticate to Azure Artifacts for both push and restore.</p>&mdash; Alex Mullans (@alexmullans) <a href="https://twitter.com/alexmullans/status/1059811282851905536?ref_src=twsrc%5Etfw">November 6, 2018</a></blockquote>

It turns out that you can't just use a personal access token to publish to Azure Artifacts.

# Credential Provider

My next step was to take a look at the [VSTS Credential Provider](https://github.com/Microsoft/artifacts-credprovider) which is essentially [the only way the documentation indicates you can publish packages](https://docs.microsoft.com/en-us/azure/devops/artifacts/get-started-nuget#publish-a-package). Thankfully the credential provider is on NuGet as [Microsoft.VisualStudio.Services.NuGet.CredentialProvider](https://www.nuget.org/packages/Microsoft.VisualStudio.Services.NuGet.CredentialProvider) so you can add it as a tool to your Cake script:

```
#tool "nuget:?package=Microsoft.VisualStudio.Services.NuGet.CredentialProvider&version=0.37.0"
```

Once we've got it installed, we need to tell NuGet where to find it. Fortunatly there's an environment variable called `NUGET_CREDENTIALPROVIDERS_PATH` that NuGet uses to find credential providers. We can set it from our Cake script like this:

```
var credentialProviderPath = GetFiles("**/CredentialProvider.VSS.exe").First().FullPath;
Information("Setting NUGET_CREDENTIALPROVIDERS_PATH to " + credentialProviderPath);
System.Environment.SetEnvironmentVariable("NUGET_CREDENTIALPROVIDERS_PATH", credentialProviderPath, EnvironmentVariableTarget.Machine);
```

Less fortunatly, this doesn't seem to work at all. In fact, I couldn't get NuGet to recognize the `NUGET_CREDENTIALPROVIDERS_PATH` environment variable no matter how it was set (and I tried everything, including using the `NuGetPushSettings.EnvironmentVariables` property). That led to just copying the credential provider alongside `nuget.exe`:

```
var credentialProviderPath = GetFiles("**/CredentialProvider.VSS.exe").First().FullPath;
var nugetPath = GetFiles("**/nuget.exe").First().GetDirectory();
CopyFiles(new [] { credentialProviderPath }, nugetPath);
```

This allowed NuGet to find the credential provider, but at that point I couldn't figure out how to automatically get it to authenticate:

```
CredentialProvider.VSS: Getting new credentials for source:https://pkgs.dev.azure.com/xyz/_packaging/xyz/nuget/v3/index.json, scope:vso.packaging_write vso.drop_write
CredentialProvider.VSS: Couldn't get an authentication token for https://pkgs.dev.azure.com/xyz/_packaging/xyz/nuget/v3/index.json.
Unable to load the service index for source https://pkgs.dev.azure.com/xyz/_packaging/xyz/nuget/v3/index.json.
Response status code does not indicate success: 401 (Unauthorized).
```

Most of the documentation talks about using the credential provider interactivly, either by displaying a UI or prompting for credentials on the command line. I'm sure there's a way to make this work from a script, but I was getting pretty frustrated with the credential provider at this point.

# System.AccessToken

I was tipped off by my Cake buddies to a couple blog posts from [Kevin Smith](https://kevsoft.net/2018/08/06/configuring-private-vsts-nuget-feeds-with-cake.html) and [Max Vasilyev](https://tech.trailmax.info/2017/01/publish-to-vsts-nuget-feed-from-cakebuild/) about using OAuth tokens for publishing to VSTS. It turns out Azure Pipelines exposes a special pipeline variable named `System.AccessToken` that contains an OAuth key for the VSTS/Azure DevOps REST API. [You have to manually activate though from your YAML file](https://docs.microsoft.com/en-us/azure/devops/pipelines/process/variables#systemaccesstoken):

```
variables:
  SYSTEM_ACCESSTOKEN: $(System.AccessToken)
```

That _should_ provide access to a `SYSTEM_ACCESSTOKEN` environment variable from inside your scripts, but...wait for it...:

```
Could not resolve SYSTEM_ACCESSTOKEN
```

Bet you saw that coming. For some reason, I couldn't figure out how to set the enviornment variable globally, but I was able to set it at the script level:

```
steps:
 - script: build -target Publish
   env:
     SYSTEM_ACCESSTOKEN: $(System.AccessToken)
```

Once that's done, you can register a new NuGet feed from inside your Cake script using the access token and then use it when publishing a package. Here's my working package publishing task inside my Cake script:

```
Task("Publish")
    .IsDependentOn("Pack")
    .WithCriteria(() => isRunningOnBuildServer)
    .Does(() =>
    {
        // Get the access token
        var accessToken = EnvironmentVariable("SYSTEM_ACCESSTOKEN");
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new InvalidOperationException("Could not resolve SYSTEM_ACCESSTOKEN.");
        }

        // Add the authenticated feed source
        NuGetAddSource(
            "VSTS",
            "https://pkgs.dev.azure.com/sipcusa/_packaging/Sipc.Utility/nuget/v3/index.json",
            new NuGetSourcesSettings
            {
                UserName = "VSTS",
                Password = accessToken
            });

        foreach (var nupkg in GetFiles(buildDir.Path.FullPath + "/*.nupkg"))
        {
            NuGetPush(nupkg, new NuGetPushSettings 
            {
                Source = "VSTS",
                ApiKey = "VSTS"
            });
        }
    });
```

Note the use of "VSTS" for the source name, `UserName`, and `ApiKey`. That value is basically a dummy value - NuGet requires _something_ for those properties but it doesn't really care what. The important part is that the `SYSTEM_ACCESSTOKEN` environment variable is being used as the `Password` for the `NuGetSourcesSettings` and that the name of the new source matches the `Source` property in the `NuGetPushSettings`.

<script async src="https://platform.twitter.com/widgets.js" charset="utf-8"></script>
