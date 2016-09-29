Title: Creating A Custom Roslyn Script Host
Lead: How to create a wrapping host class and lift method and class declarations.
Published: 9/21/2016
Image: /images/rollercoaster.jpg
Tags:
- Roslyn
- scripting
---
I love [Roslyn's scripting support](https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples). It lets you quickly spin up a script host and evaluate arbitrary expressions without having to think too much about things like default references and host classes. However, sometimes you need to customize the behavior of the script or the host and the only way to accomplish this is to create your own scripting functionality using the foundational Roslyn compilation libraries. This post will show you how to create a custom script host that includes a wrapping context class with support for global properties, lifting of global class and method declarations, and output to a dynamic assembly for invocation and evaluation.


# Wrapping Context Class

The Roslyn scripting libraries actually aren't all that complicated (at least relative to the main Roslyn compilation libraries). They essentially do what we're going to explore in this blog post, but in a more general way.

The code in a script typically consists of statements that need to be evaluated. Note that any code in C# needs to be placed within a class and method (there's no such thing as "global code"). For example, consider the following script:

```
int x = 1 + 2;
Console.WriteLine(x);
```

This isn't a valid C# program by itself and it would produce and error if you tried to compile it as-is. One of the primary jobs of the script host is to place the script code inside an invisible class and method that wraps the script code and turns it into a valid compilation.

For example, the script code above could be wrapped in the following class and method to turn it into a valid compilation:

```
public class Script
{
  public void Run()
  {
    int x = 1 + 2;
    Console.WriteLine(x);
  }
}
```

Now if you compiled it, either in Visual Studio or by the Roslyn compilation libraries, it would produce a library assembly that you could use like any other.

Creating the wrapping class and method in our script host doesn't have to be complicated. In fact, string interpolation makes it extremely easy. This functionality will be the first step in our custom script host:

```

```

## Adding Globals

# Lifting Global Declarations

# Output To Dynamic Assembly

# Invocation