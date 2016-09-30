Title: Capturing Standard Input in C#
Lead: My experience resolving a surprisingly tricky problem
Published: 5/17/2016
Tags:
  - stdin
  - stream
  - console
  - cli
---
I recently ran across a problem that was surprisingly challenging. I have a console application and I wanted to be able to send it information over standard input. That includes piping data to it such as `echo "test" | myapp.exe` and receiving command redirection such as `myapp.exe < file.txt` ([see this link](https://www.microsoft.com/resources/documentation/windows/xp/all/proddocs/en-us/redirection.mspx) for more information about Windows command redirection). This sort of input all comes into your application by way of a stream called "standard input" or "stdin" for short. It turns out there's a lot of edge cases to consider when trying to capture standard input, and accounting for them all can be difficult.

The approach I used for a while is:

```
string stdin = null;
if (Console.IsInputRedirected)
{
    using (StreamReader reader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding))
    {
        stdin = reader.ReadToEnd();
    }
}
```

This code relies on the `Console.IsInputRedirected` flag to indicate whether the standard input stream for our process has been redirected, thus indicating that someone is using it to send us something. Checking this is important. The standard input is a `Stream` and as such will block the calling thread when you try to execute code like `Stream.Read()`. If the input hasn't been redirected and you write code like the following, you'll just sit there waiting and waiting for someone to send you something:

```
// This will block waiting for input that never comes
using (StreamReader reader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding))
{
    stdin = reader.ReadToEnd();
}
```

Of course, this would be a short blog post if that first approach worked well. Unfortunately, I discovered that there are situations where it doesn't. If your application could get called from hosts that open standard input as part of their normal operation, but never actually send it anything (or leave it open after sending something), you'll still end up blocking because your application will never reach the "end" of the stream. Good defensive programming means we need to account for this whether we envision it happening or not. One such host is the Visual Studio Code task runner. It apparently opens and leaves open the standard input stream, so that code in my first example would never get to the end of the stream and the application hangs.

This is where it gets a little hacky. There just isn't a good way to read from an arbitrary stream, but stop reading if there isn't any data available. *Some* streams support setting timeouts, cancelling read operations, and the like, but not all. Most of the time our standard input stream is going to fall into this stubborn category.

Your first inclination might be to make use of the `Stream` methods that use a `CancellationToken` like `Stream.ReadAsync()`. For some reason, that doesn't appear to be reliable. Perhaps it was just my setup, but I couldn't get the stream to reliably cancel read operations by triggering the `CancellationToken` in an alternate thread.

That leads us to this little hack:

```
string stdin = null;
if (Console.IsInputRedirected)
{
    using (Stream stream = Console.OpenStandardInput())
    {
        byte[] buffer = new byte[1000];  // Use whatever size you want
        StringBuilder builder = new StringBuilder();
        int read = -1;
        while (true)
        {
            AutoResetEvent gotInput = new AutoResetEvent(false);
            Thread inputThread = new Thread(() =>
            {
                try
                {
                    read = stream.Read(buffer, 0, buffer.Length);
                    gotInput.Set();
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
            })
            {
                IsBackground = true
            };

            inputThread.Start();

            // Timeout expired?
            if (!gotInput.WaitOne(100))
            {
                inputThread.Abort();
                break;
            }

            // End of stream?
            if (read == 0)
            {
                stdin = builder.ToString();
                break;
            }

            // Got data
            builder.Append(Console.InputEncoding.GetString(buffer, 0, read));
        }
    }
}
```

This code launches independent threads one at a time to fill a buffer and then will abort the whole operation if a short timeout is exceeded (including the currently running fill thread). It's certainly not pretty, but in my testing it was the only approach that worked in every case I tried. I'd love it if someone had an easier answer to this, so if you do please let me know.
