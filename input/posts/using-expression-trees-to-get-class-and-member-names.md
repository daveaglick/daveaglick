Title: Using Expression Trees To Get Class And Member Names
Image: /images/tree.jpg
Published: 6/22/2017
---
This is a short post about a technique I've been using recently to get class and member names without resorting to strings. It's not particularly innovative, but works well and might be of interest if you hate magic strings as much as I do.

First, let's assume that we have the following class:

```
public class Foo
{
	public int Bar { get; set; }
	public int Baz(int red, string green) => 0;
}
```

Let's also assume that we have the following method that requires a string-based class name and member name:

```
public void UseNames(string className, string memberName)
{
    // ...
}
```

We need to be able to call the `UseNames` method with the class and member name for the `Foo.Bar` property and the `Foo.Baz` method.

## Magic Strings

The most direct way of doing this would be to just use strings:

```
UseNames("Foo", "Bar");
UseNames("Foo", "Baz");
```

Those are called *magic strings*. They're magic because they refer to elements in the code and are specified as string literals. Magic strings have a lot of problems, not the least of which is that if you refactor the code (for example, to rename the class) you'd have to remember to change the string or do a string-based find-and-replace, both of which are error prone.

We can do better.

## nameof

[The `nameof` operator](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/nameof) was introduced in C# 6 and it's fantastic for eliminating magic strings by getting the name for a code element. With the `nameof` operator we can rewrite those string literals like this:

```
UseNames(nameof(Foo), nameof(Foo.Bar));
UseNames(nameof(Foo), nameof(Foo.Baz));
```

That's a little better. We no longer have magic strings, but we still have some redundancy because we're using the class twice: once to get the class name, and again to qualify the member name. It also means we can inadvertently change one without changing the other since the class name and member name are totally disconnected.

We can do better.

## Property Name

[Expression Trees](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/) let us examine code at runtime. It's like doing reflection on code instead of an object. We can use an expression tree to capture a property call and get the name of the property:

```
public void UseNames<T>(Expression<Func<T, object>> expression)
{
	MemberExpression member = expression.Body as MemberExpression;
	if (member == null)
	{
		// The property access might be getting converted to object to match the func
		// If so, get the operand and see if that's a member expression
		member = (expression.Body as UnaryExpression)?.Operand as MemberExpression;
	}
	if (member == null)
	{
		throw new ArgumentException("Action must be a member expression.");
	}

    // Pass the names on to the string-based UseNames method
	UseNames(typeof(T).Name, member.Member.Name);
}
```

Now we can get the class and property name like this:

```
UseNames<Foo>(x => x.Bar);
```

## Method Name

Unfortunately, the expression tree code used for property names won't quite work for method names because an expression still has to be valid code and we can't call a method without specifying arguments. However, this is a single exception to this rule: the `nameof` operator I mentioned above. When inside a `nameof` expression, you can refer to any member, including a method, without having to form a complete call. So what we need is a way to combine the use of `nameof` with the expression technique we used for property names:

```
public void UseNames<T>(Expression<Func<T, string>> expression)
{
	ConstantExpression constant = expression.Body as ConstantExpression;
	if (constant == null)
	{
		throw new ArgumentException("Expression must be a constant expression.");
	}
	UseNames(typeof(T).Name, constant.Value.ToString());
}
```

Notice this code is much shorter, and that the `Func<T, string>` is now returning a string instead of an object. That's because all we're really doing here is using the `Func` to allow us to call a `nameof` on the appropriate type as specified in the generic type argument:

```
UseNames<Foo>(x => nameof(x.Bar));
UseNames<Foo>(x => nameof(x.Baz));
```

And there you have it: a short, single method that will get the class and member name for both properties and methods without magic strings and with a minimum amount of redundancy.

## Similar Approaches

This technique is far from unique and folks have been using expression trees for similar purposes for a while. As pointed out by [vcsjones](https://twitter.com/vcsjones) there are even some [helpers built into ASP.NET MVC](https://github.com/aspnet/Mvc/blob/4bddb5ff1b42b353ab66c7bd31356d3353c79b7d/src/Microsoft.AspNetCore.Mvc.ViewFeatures/Internal/ExpressionHelper.cs#L22) that perform similar functionality (though not quite the same). That said, I've never seen an expression tree used this way in combination with a `nameof` inside the expression to get a method name without arguments from a lambda, so there's that.

## Performance

As has been [pointed out on Twitter by filip_woj](https://twitter.com/filip_woj/status/877905232272867328), expression trees are slow. Depending on what you're doing with them, they can add a large performance hit to your application. The code above shouldn't be *too* bad since we're not actually compiling the expressions, just inspecting them, but buyer beware. If you're running performance sensitive code, it's worth doing some benchmarks. It's up to you whether the convenience of using an expression tree to get the name for both the class and member without using two separate `nameof` operators is worth the performance hit.

Thanks to [torn_hoof](https://twitter.com/torn_hoof), we have a benchmark for comparison. Based on [running all three possibilities through BenchmarkDotNet](https://gist.github.com/Tornhoof/2bb73db914a13825ec7c0eb89d8d6b6a) we can observe the following:

```
 |                    Method |          Mean |      Error |    StdDev |  Gen 0 | Allocated |
 |-------------------------- |--------------:|-----------:|----------:|-------:|----------:|
 |           NameOfBenchmark |     0.0002 ns |  0.0010 ns | 0.0009 ns |      - |       0 B |
 |       ExpressionBenchmark | 1,394.7139 ns | 10.5833 ns | 9.8996 ns | 0.2117 |     888 B |
 | ExpressionNameOfBenchmark |   824.3050 ns |  4.2835 ns | 4.0068 ns | 0.1335 |     560 B |
```

In other words:
* `UseNames(nameof(Foo), nameof(Foo.Bar))` takes about 0 ns per call
* `UseNames<Foo>(x => nameof(x.Bar))` takes about 820 ns per call
* `UseNames<Foo>(x => x.Bar)` takes about 1,400 ns per call

After getting some hard numbers, I wouldn't go using this approach over and over in a tight loop in performance critical methods, but I'd be comfortable using it in an average web or desktop application.