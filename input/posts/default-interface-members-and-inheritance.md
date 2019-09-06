Title: Default Interface Members and Inheritance
Published: 9/6/2019
Image: /images/thinking.jpg
Tags:
  - csharp
---
Default interface members (or "DIM" as I've seen the feature called) is a new language feature available in C# 8 that lets you define implementations directly in an interface. I started out with the intent of writing about use cases for the feature, but ended up writing so much that I decided to split the post in two. This part deals with how default interface members need to be invoked and the differences in semantics between class inheritance and default interface member implementation.

# Must Invoke From The Interface

Consider the following code:

```csharp
interface ICar
{
	// Seems like a reasonable default
	public int GetTopSpeed() => 150;
}

public class Elantra : ICar
{	
}
```

This defines an interface `ICar` with a method `GetTopSpeed()` and that method has a default implementation. You might think you could then write:

```csharp
Elantra e = new Elantra();
e.GetTopSpeed();
```

But that won't compile. You have to invoke default interface members from an instance of the interface (unless they've been redefined, more on that in a minute):

```csharp
Elantra e = new Elantra();
((ICar)e).GetTopSpeed();
```

At this point you might be thinking "well that seems silly," but there's a good reason why default interface members behave this way. Consider the following:

```csharp
interface ICar
{
	// Seems like a reasonable default
	public int GetTopSpeed() => 150;
}

interface IMovable
{
	// Nothing moves faster than the speed of light
	public int GetTopSpeed() => 671000000;
}

public class Elantra : ICar, IMovable
{	
}
```

If you called `GetTopSpeed()` on an instance of `Elantra` what would the result be? Are you actually calling `ICar.GetTopSpeed()` or `IMovable.GetTopSpeed()`? This problem (often referred to as "diamond inheritance") is one of the reasons true multiple inheritance is so difficult to do well in a language like C++. To avoid it, the C# language team explicitly elected _not_ to make default interface members a mechanism to achieve multiple inheritance. Instead you have to be explicit about which implementation you're calling to remove all ambiguity.

# Default Implementations vs. Inheritance

[Something that initially confused me](https://twitter.com/daveaglick/status/1169777331608707075) was the relationship between default interface members and the way members are inherited in a traditional class hierarchy. Consider this code:

```csharp
interface ICar
{
	public string Make { get; }
	public int Cylinders => 4;
}

public abstract class Toyota : ICar
{
	public string Make => "Toyota";
}

public class Avalon : Toyota
{
	public int Cylinders => 6;
}
```

What would you expect this code to output:

```csharp
ICar car = new Avalon();
Console.WriteLine(car.Cylinders);
```

My initial reaction was that this should output `6`, but it actually outputs `4`.

<?# giphy g01ZnwAUvutuK8GIQn /?>

The reason is because `Avalon.Cylinders` isn't actually implementing `ICar.Cylinders` given that the interface is implicit via the base `Toyota` class. They're two totally different properties.

[Ben Adams was the first](https://twitter.com/ben_a_adams/status/1169790052425240581) of many to point out that this behavior isn't actually different from the way interfaces currently work. The code above is essentially equivalent to writing the following, which will also output `4` instead of `6`:

```csharp
interface ICar
{
	public string Make { get; }
	public int Cylinders { get; }
}

public abstract class Toyota : ICar
{
	public string Make => "Toyota";
	int ICar.Cylinders => 4;
}

public class Avalon : Toyota
{
	public int Cylinders => 6;
}
```

I envision this being something I'll have to keep reminding myself about. I think the reason is that the semantics are different from what we're used to after a decade of working with `virtual` and `override` in class hierarchies.

More specifically, up until default interface members we _had_ to provide an implementation within an implementing class because the interface simply couldn't contain one. That means in the code above for the abstract `Toyota` base class I would've had to write one of these:
- `public int Cylinders => 4` to implement the interface property and provide a default value, forcing the property into the inheritance chain of `Toyota`.
- `public abstract int Cylinders { get; }` to define the interface property as abstract and force derived classes to provide an implementation.
- `int ICar.Cylinders => 4` to implement the interface property and provide a default value, but not place the property into the inheritance chain of `Toyota`.

I've come to think of that last syntax as "opting-out" of class inheritance. I have to have _something_ that implements the interface property (because it's not implemented in the interface) and I have to use a special syntax that makes it very clear I'm implementing the property at the interface and not the class level if that's my intent. **If you don't want the property to be a part of the class inheritance hierarchy you have to opt-out**.

Contrast that with the semantics of a default interface member. The equivalent `int ICar.Cylinders => 4` definition never has to show up in the implementing `Toyota` class since the default property implementation was provided directly in the interface. In this case `Cylinders` has an implementation from the interface so you're not forced to put _anything_ in the `Toyota` class about it. That property does not belong to the class in this case. **If you want the property to be a part of the class inheritance hierarchy you have to opt-in**:

```csharp
interface ICar
{
	public string Make { get; }
	public int Cylinders => 4;
}

public abstract class Toyota : ICar
{
	public string Make => "Toyota";
	public virtual int Cylinders =>
		((ICar)this).Cylinders;
}

public class Avalon : Toyota
{
	public override int Cylinders => 6;
}
```

This code will output the expected `6` because we "opted-in" to implementing the `Cylinders` property in the `Toyota` class instead of leaving the implementation in the interface. The `Toyota` class only invokes the implementation from the interface, but by doing so we've placed the property implementation into the class inheritance hierarchy and can now rely on the `virtual` and `override` behavior we know.

One final note: the `((ICar)this).Cylinders` syntax in the class implementation that calls the default interface implementation is awkward. [There's an open issue](https://github.com/dotnet/csharplang/issues/406) to add support for `base(ICar).Cylinders` syntax, but it requires changes to the CLR so [it got pushed to a later language version](https://github.com/dotnet/csharplang/blob/master/meetings/2019/LDM-2019-04-29.md#conclusion).