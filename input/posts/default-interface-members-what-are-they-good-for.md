Title: Default Interface Members, What Are They Good For?
Published: 9/12/2019
Image: /images/thumbs-up.jpg
Tags:
  - csharp
---
[In my last post](/posts/default-interface-members-and-inheritance) I promised to look at some of the use cases where I think it's worthwhile to consider using default interface members. They're certainly not going to replace many existing conventions, but I have found some situations where targetted use can lead to cleaner, more maintainable code (at least in my own opinion).

# Update Interfaces Without Breaking

[The docs](https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/default-interface-members-versions) say:

> The most common scenario is to safely add members to an interface already released and used by innumerable clients.

The problem this solves is that if add a new member to an interface, every type that implements that interface will need to provide an implementation for that member. This may not be such a big deal if the interface is in your own code but as with any breaking change, in a library released to the public or other teams it can create a lot of headaches.

Consider the example from my previous post:

Consider the following code:

```csharp
interface ICar
{
    string Make { get; }
}

public class Avalon : ICar
{
    public string Make => "Toyota";
}
```

If I wanted to add a new `GetTopSpeed()` method to the interface, I'd need to then implement it in the `Avalon` class:

```csharp
interface ICar
{
    string Make { get; }
    int GetTopSpeed();
}

public class Avalon : ICar
{
    public string Make => "Toyota";
    public int GetTopSpeed() => 130;
}
```

However, if I create a default implementation of the new `GetTopSpeed()` method in `ICar` I don't need to add it to every implementing class:

```csharp
interface ICar
{
    string Make { get; }
    public int GetTopSpeed() => 150;
}

public class Avalon : ICar
{
    public string Make => "Toyota";
}
```

In addition, I can still provide override implementations for classes where the default isn't appropriate:

```csharp
interface ICar
{
    string Make { get; }
    public int GetTopSpeed() => 150;
}

public class Avalon : ICar
{
    public string Make => "Toyota";
    public int GetTopSpeed() => 130;
}
```

One important note though is that as I mentioned in my previous post, the default `GetTopSpeed()` method will only be available on variables of type `ICar` and not `Avalon` if you don't also provide an override implementation in the class. That means this technique is primarily useful only if you pass around interface types and not implementing types (otherwise you'll end up with a bunch of casts to the interface type in order to get access to the default member implementations).

# Mixins and Traits (Sort Of)

[Mixins](https://en.wikipedia.org/wiki/Mixin) and the similar language concept of [traits](https://en.wikipedia.org/wiki/Trait_(computer_programming)) both describe ways of extending the behavior of an object through composition without resorting to multiple inheritance.

[The Wikipedia article on mixins](https://en.wikipedia.org/wiki/Mixin) says:

> A mixin can also be viewed as an interface with implemented methods.

Sound familiar?

Interfaces in C# that contain default implementations aren't exactly mixins because they can also contain unimplemented members, support interface inheritance, can be specialized, etc. However, if we make an interface that just contains default members we have a mostly traditional mixin.

Consider the following code that adds functionality for "moving" an object and tracking it's location (for example, in a game environment):

```csharp
public interface IMovable
{
    public (int, int) Location { get; set; }
    public int Angle { get; set; }
    public int Speed { get; set; }

    // A method that changes location
    // using angle and speed
    public void Move() => Location = ...;
}

public class Car : IMovable
{
    public string Make => "Toyota";
}
```

Whops! There's a problem with this code that I hadn't considered until I wrote it for the post and tried to compile it. Interfaces (even ones with default implementations) can't contain state. Therefore auto-implemented properties aren't supported by default interface members. From the [design document for default interface members](https://github.com/dotnet/csharplang/blob/master/proposals/csharp-8.0/default-interface-methods.md#detailed-design):

> Interfaces may not contain instance state. While static fields are now permitted instance fields are not permitted in interfaces. Instance auto-properties are not supported in interfaces, as they would implicitly declare a hidden field.

This is where default interface members and the concept of mixins start to diverge a bit (mixins can conceptually contain state as far as I understand them), but we can still accomplish the original goal:

```csharp
public interface IMovable
{
    public (int, int) Location { get; set; }
    public int Angle { get; set; }
    public int Speed { get; set; }

    // A method that changes location
    // using angle and speed
    public void Move() => Location = ...;
}

public class Car : IMovable
{
    public string Make => "Toyota";

    // IMovable
    public (int, int) Location { get; set; }
    public int Angle { get; set; }
    public int Speed { get; set; }
}
```

This accomplishes the original goal by making the common `Move()` method and it's implementation available to all classes that apply the interface. Sure, the class still needs to provide implementations for the properties, but the way that they're at least declared in the `IMovable` interface means the default members in that interface can operate on the those properties and guarantees any class applying the interface will expose the correct state.

As a more complete and practical example, consider a logging mixin:

```csharp
public interface ILogger
{
    public void LogInfo(string message) =>
        LoggerFactory
            .GetLogger(this.GetType().Name)
            .LogInfo(message);
}

public static class LoggerFactory
{
    public static ILogger GetLogger(string name) =>
        new ConsoleLogger(name);
}

public class ConsoleLogger : ILogger
{
    private readonly string _name;

    public ConsoleLogger(string name)
    {
        _name = name
        ?? throw new ArgumentNullException(nameof(name));
    }

    public void LogInfo(string message) =>
        Console.WriteLine($"[INFO] {_name}: {message}");
}
```

I could then apply the `ILogger` interface to any class:

```csharp
public class Foo : ILogger
{
    public void DoSomething()
    {
        ((ILogger)this).LogInfo("Woot!");
    }
}
```

And code like:

```csharp
Foo foo = new Foo();
foo.DoSomething();
```

Would produce:

```
[INFO] Foo: Woot!
```

# Replacing Extension Methods

The biggest area of utility I've found so far is replacing large sets of extension methods. Let's go back to a simple logging example:

```csharp
public interface ILogger
{
    void Log(string level, string message);
}
```

Before default interface members I would typically implement a bunch of extensions to this logging interface so that the logger implementation would only have to implement a single method but users could call a variety of overloads:

```csharp
public static class ILoggerExtensions
{
    public static void LogInfo(this ILogger logger, string message) =>
        logger.Log("INFO", message);

    public static void LogInfo(this ILogger logger, int id, string message) =>
        logger.Log("INFO", $"[{id}] message");

    public static void LogError(this ILogger logger, string message) =>
        logger.Log("ERROR", message);

    public static void LogError(this ILogger logger, int id, string message) =>
        logger.Log("ERROR", $"[{id}] {message}");

    public static void LogError(this ILogger logger, Exception ex) =>
        logger.Log("ERROR", ex.Message);

    public static void LogError(this ILogger logger, int id, Exception ex) =>
        logger.Log("ERROR", $"[{id}] {ex.Message}");
}
```

That's fine, and works. But it has a few shortfalls. For one, the namespaces of the static extension class and the interface may not necessarily match. It also creates some noise by requiring the `this ILogger logger` parameter and referring to a `logger` instance.

What I've started doing with big sets of extensions is making them default interface members instead:

```csharp
public interface ILogger
{
    void Log(string level, string message);

    public void LogInfo(string message) =>
        Log("INFO", message);

    public void LogInfo(int id, string message) =>
        Log("INFO", $"[{id}] message");

    public void LogError(string message) =>
        Log("ERROR", message);

    public void LogError(int id, string message) =>
        Log("ERROR", $"[{id}] {message}");

    public void LogError(Exception ex) =>
        Log("ERROR", ex.Message);

    public void LogError(int id, Exception ex) =>
        Log("ERROR", $"[{id}] {ex.Message}");
}
```

I find those implementation much cleaner and easier to read (and thus maintain). Using default interface members also presents some other benefits where extensions might otherwise have been used:

- They're in the scope of the instance and `this` can be used.
- Other types of members like indexers can be provided.
- They can be overridden by implementing classes to specialize the behavior.

Something that bugs me about the code above though is that it's not totally clear what the required, unimplemented contract of the interface is and what's implemented by default. A comment separating the two sections might help but I do like how extension classes are explicit in this regard.

To address that, I've starting making any interface that contains default members partial (other than one or two trivial ones). Then I put the default members in other files with the naming convention "ILogger.LogInfoDefaults.cs" and "ILogger.LogErrorDefaults.cs", etc. If I only have a small set of default members that don't suggest any sort of grouping, I name the file "ILogger.Defaults.cs".

This separates the default member implementations from the unimplemented contract that any implementing class will have to provide implementations for. It also helps break up what could become a very long file. There's even a neat trick to enable ASP.NET-style Visual Studio file nesting in any project format. Add this to your project file or `Directory.Build.props`:

```xml
<ItemGroup>
  <ProjectCapability Include="DynamicDependentFile"/>
  <ProjectCapability Include="DynamicFileNesting"/>
</ItemGroup>
```

Then you can select "File Nesting" in the Solution Explorer and all your `.Defaults.cs` files will appear as children of the main interface file.

Finally, there are still some situations where extension methods are preferred:

- If you typically pass around class types instead of the interface type (because you'd have to cast to the interface to access the default member implementations).
- If you often use the pattern `public static T SomeExt<T>(this T foo)` to return the exact type the extension was called for (for example, in a fluent API).
