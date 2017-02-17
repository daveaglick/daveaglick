Title: Synchronizing Files With Azure Web Apps Over FTP
Lead: Easy deployment to Azure using FTP or FTPS
Published: 2/17/2017
Image: /images/servers.jpg
Tags:
  - Azure
  - FTP
---
I've recently been experimenting with Azure for static site hosting. While there are lots of great static site hosts ([my personal favorite](/posts/moving-to-netlify) still remains [Netlify](https://www.netlify.com)), [Azure Web Apps](https://azure.microsoft.com/en-us/services/app-service/web/) offer some attractive features to enterprises or organizations already invested in Azure. One would think that easily deploying a static site to Azure would be relatively straightforward. Unfortunately, I found that this wasn't the case at all. While Azure Web Apps have some advanced [deployment options](https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-deploy) like [Kudu](https://github.com/projectkudu/kudu/wiki/Deployment) for git deployments and [Web Deploy](http://www.iis.net/learn/publish/using-web-deploy/introduction-to-web-deploy) for deployments from Visual Studio, both require some setup and configuration, are designed with "application" scenarios in mind, and aren't as straightforward as a simple [FTP upload](https://en.wikipedia.org/wiki/File_Transfer_Protocol). Unfortunately, even FTP uploads to Azure Web Apps have some issues that I'll discuss below. To address this scenario, I ended up writing a little bit of code to automatically synchronize a local static site with an Azure Web App FTP server that ignores unchanged files.

# Challenges of FTP For Static Sites

Using FTP may seem like an attractive option for uploading a static site. It's easy to understand, lots of client libraries and software exists, and you don't need to do anything particularly special from a devops perspective. For small sites this works great. You just point your FTP client to Azure, delete the old stuff, and upload the new stuff. However, this process starts to break for much larger sites with lots of pages or large resources like images or PDF files. In those cases it can become very time consuming to re-upload everything in the site over and over again.

The normal way to deal with this problem is to attempt to compare files on your local system with those on the FTP server. There are four possible states:
- A file exists locally that doesn't exist remotely.
- A file exists remotely that doesn't exist locally.
- A file exists in both places and is different.
- A file exists in both places and is the same.

Those last two conditions are the tricky ones. It's easy enough to tell if a file exists locally or on the remote server, but what does "different" and "the same" mean and how can we tell? The trick is figuring out how to determine if a file is "the same" without actually having to download that file from the remote server. There are a number of different ways to deal with this when using FTP but they all have either problems when considering static sites or when dealing with the Azure Web Apps FTP server.

- One common approach is to compare creation timestamps. The idea is that if a file was created at the same time and is the same size then it's probably the same file. This works great for dynamics sites where the files you're uploading constitute source code and don't regularly change. However, in a static site you're probably re-generating the files on every build. That creates new local files each time with new time stamps. Thus the time stamps are essentially useless for static sites.
- We could try asking the FTP server for hash codes. Some FTP servers support this either through a [proposed IETF standard for the HASH command](https://tools.ietf.org/html/draft-ietf-ftpext2-hash-03) (which expired without ever being officially adopted) or via non-standard hashing commands like XMD5. Unfortunately the Azure Web Apps FTP server doesn't support HASH or any of the other non-standard hashing commands.
- Some FTP servers also support CRC checks using he XCRC command which would probably be good enough for our purposes. That's also unsupported on the Azure Web Apps FTP server.

This leaves us with a bit of a conundrum. How can we determine if a file on the remote server is different than the one on our local system without downloading it first? The answer is to hash the file on your local system and then store the result of that hash operation on the remote server. Then when we need to compare files we just have to download this set of pre-computed hashes that match the files on the remote server and use them instead.

# Implementation

Here is an implementation of this synchronization process. It uses the [FluentFTP](https://github.com/hgupta9/FluentFTP) library, though any FTP library should work with the overall approach. This creates a `hashes.xml` file on the remote server to store the remote file hashes.

```
string host = "something.ftp.azurewebsites.windows.net";
string username = "azure-ftp-username";
string password = "azure-ftp-password";
string localPath = @"C:\StaticSite\output";
string remotePath = "/site/wwwroot";

using (FtpClient client = new FtpClient())
{            
    // Note sure why we need to change these, but get timeout exceptions from FluentFTP if we don't
    client.SocketPollInterval = 1000;
    client.ConnectTimeout = 2000;
    client.ReadTimeout = 2000;
    client.DataConnectionConnectTimeout = 2000;
    client.DataConnectionReadTimeout = 2000;

    // Get the connection
    client.Host = host;
    client.Credentials = new System.Net.NetworkCredential(username, password);
    client.EncryptionMode = FtpEncryptionMode.Implicit;
    client.SslProtocols = System.Security.Authentication.SslProtocols.Default;
    client.Connect();

    // Get the remote hashes
    Dictionary<string, string> remoteHashes = null;
    string remoteHashesFile = $"{remotePath}/hashes.xml";
    if (client.FileExists(remoteHashesFile))
    {
        using (MemoryStream stream = new MemoryStream())
        {
            if (client.DownloadFile(stream, remoteHashesFile))
            {
                XElement items = XElement.Parse(client.Encoding.GetString(stream.GetBuffer()).Replace((char)0x00, ' '));
                remoteHashes = items.Descendants("file")
                    .ToDictionary(x => x.Attribute("path").Value, x => x.Attribute("hash").Value);
            }
        }
    }
    
    // Get all local files
    Dictionary<string, FileMapping> mappings =
        System.IO.Directory.GetFiles(localPath, "*", SearchOption.AllDirectories)
        .Select(x => new
        {
            LocalFile = x,
            RemoteFile = $"{remotePath}/{x.Substring(localPath.Length + 1).Replace("\\", "/")}"
        })
        .ToDictionary(x => x.RemoteFile, x =>
        {
            string remoteHash = null;
            return new FileMapping
            {
                LocalFile = x.LocalFile,
                LocalHash = GetHash(x.LocalFile),
                RemoteHash = (remoteHashes?.TryGetValue(x.RemoteFile, out remoteHash) ?? false) ? remoteHash : null
            };
        });    
    
    // Delete everything on the remote not existing locally
    Stack<string> remoteDirs = new Stack<string>();
    remoteDirs.Push(remotePath);
    while (remoteDirs.Count > 0)
    {            
        // Iterate the child items
        foreach (FtpListItem remoteItem in client.GetListing(remoteDirs.Pop()))
        {                
            if (remoteItem.Type == FtpFileSystemObjectType.Directory)
            {
                // Push this directory on the stack to scan next
                remoteDirs.Push(remoteItem.FullName);
            }
            else if (remoteItem.Type == FtpFileSystemObjectType.File && !mappings.ContainsKey(remoteItem.FullName))
            {
                // Destination file doesn't exist locally
                client.DeleteFile(remoteItem.FullName);
            }
        }
    }
    
    // Now we're left with files locally that either don't exist on remote, have changed, or are the same
    foreach (KeyValuePair<string, FileMapping> mapping in mappings)
    {
        if (mapping.Value.RemoteHash == null || mapping.Value.LocalHash != mapping.Value.RemoteHash)
        {
            // Have to delete first if it exists, see https://github.com/hgupta9/FluentFTP/issues/46
            if (client.FileExists(mapping.Key))
            {
                client.DeleteFile(mapping.Key);
            }
            
            // Either doesn't exist on the remote or is different
            client.UploadFile(mapping.Value.LocalFile, mapping.Key, true);
        }
        else
        {
            // Same, just output a message if you want to
        }
    }

    // Create and upload the hash file
    XElement localHashes = new XElement("files", mappings.Select(x => 
        new XElement("file",
            new XAttribute("path", x.Key),
            new XAttribute("hash", x.Value.LocalHash))));
    client.UploadFile(client.Encoding.GetBytes(localHashes.ToString()), remoteHashesFile, true);
}
```

# Things To Keep In Mind

While this approach has proven to be stable and robust for me, there are a couple of caveats.

- I talk about FTP in this post because that's the underlying protocol. In reality you should almost never use FTP alone. Instead you should use FTPS to communication with the Azure Web App FTP server over SSL to ensure the connection is secure. The code above reflects this, but it's worth reiterating.
- The static generation needs to be deterministic. The hashing concept only works if the static output files are the same. If small bits of content change in the output files from one generation to the next, you'll end up with different hashes and those files will be uploaded again.
- Try to make sure the entire upload finishes since the hash file is deleted as the first step and isn't uploaded again until completion. This is on purpose as a fail safe. If the process fails the site will be left without a hash file and everything will get uploaded fresh (replacing files that may not have changed).
- Make sure to always update the hash file. If remote files change and the hash file isn't updated, the deployment process may not come to the correct conclusion about the file's sameness and may either replace it unnecessarily, or worse, not replace it when it should be replaced.
- I didn't include any output statements in the code above, but in practice I usually write out to the console or a log with the progress and what is happening with each file.