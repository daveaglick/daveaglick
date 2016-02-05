Title: Computed Properties and Entity Framework (Revisited)
Lead: Another way to use your computed properties in predicates and projections.
Published: 2/5/2016
Tags:
  - Entity Framework
  - LINQ
  - database
  - LINQ to Entities
---
Since publishing [my first post on the topic](/posts/computed-properties-and-entity-framework) over a year ago, I've continued to look for easy ways to tackle this problem. In the last post, I ended up recommending the excellent [DelegateDecompiler](https://github.com/hazzik/DelegateDecompiler) library to help convert plain unmapped properties to expression trees that LINQ to Entities can use. I still like this approach, but I've also been searching for a way to make this process a little more transparent and use a little less magic.

First, a little detour. I know Entity Framework specifically, and other "heavy" ORMs in general, have a bad reputation from some developers. That's fine, everyone should use what makes them the most productive. However, this position does bother me a bit because I think it's partly unfair. Entity Framework and other ORMs are tools. Like any complex tool they can be used without much depth or you can learn how they really work and use them well. An analogy might be a racing bike. I'm not a biker (bicyclist?) and probably wouldn't even know how to properly pedal one, let alone change gears or swap the chain. But I can hop on and probably get from one place to another. However, I might complain that the bike is slow, hard to push up hills, makes my muscles hurt, and is generally less productive than just walking. I see a lot of complains of Entity Framework that seem focused on the fact that it's slow, generates queries that are too big, uses too much memory, etc. In many cases (though not all), these complaints can be mitigated by a better understanding of the tool. For example, you may need to eagerly load navigation properties or delay materialization. I've personally found that I can generally write Entity Framework queries that generate SQL similar to what I would have hand generated. And let's not forget that it's also *really easy* to write bad SQL too. Anyway, rant over.

Getting back to the topic at hand, I've attempted to find a way to avoid all the expression tree magic and dynamic compilation involved in a lot of the more advances approaches I discussed in my previous post. It's not that there's anything wrong with them, they all work quite well actually, it's just that I tend to strive for the simplest most direct way to do something because I find it's much more maintainable in the long run.

I had three requirements:
- The approach had to let me do the same kinds of evaluations I would be able to in an inline LINQ to Entities query (but didn't need to do anything exotic like convert unsupported methods to expressions).
- It had to let me use *the same code* to do the calculation in the context of a single property of an entity.
- It had to be relatively simple to use without too many hoops or duplicated setup code.

## Implementation

Here's what I've come up with and I figured I'd share in case anyone is looking to do the same thing.

It starts with a couple projection classes:

```
public class ComputedInput<TSource, TItem>
{
    public TSource Source { get; set; }
    public TItem Item { get; set; }
}

public class ComputedOutput<TSource, TValue>
{
    public TSource Source { get; set; }
    public TValue Value { get; set; }
}
```

The idea here is that when used in a query, I'll be starting with an arbitrary source object that may or may not be the object I need to perform the calculation on. It's important to carry this source object through the entire computation and provide a result projection that includes it, otherwise we'll just be returning the result of the computation by itself which would prevent additional chained computations, predicates, or mapping.

There's also an abstract class from which we'll create our shared computations:

```
public abstract class Computation<TItem, TValue>
{
    public abstract Expression<Func<ComputedInput<TSource, TItem>, ComputedOutput<TSource, TValue>>> GetComputation<TSource>();

    public TValue GetValue(TItem item)
    {
        return GetComputation<object>()
            .Compile()
            .Invoke(new ComputedInput<object, TItem>
            {
                Item = item
            })
            .Value;
    }
}
```

The `GetComputation<TSource>()` method is what we'll have to override for each computation class and where the computation logic will go. `GetValue()` allows us to use the computation from a single instance rather than an entire query (more on this in a bit).

Finally, we've got a few extension methods:

```
public static class ComputationExtensions
{
    public static IQueryable<TResult> Compute<TSource, TItem, TValue, TResult>(
        this IQueryable<TSource> source,
        Computation<TItem, TValue> computation,
        Expression<Func<TSource, ComputedInput<TSource, TItem>>> itemSelector,
        Expression<Func<ComputedOutput<TSource, TValue>, TResult>> resultSelector)
    {
        Expression<Func<ComputedInput<TSource, TItem>, ComputedOutput<TSource, TValue>>> computationExpression
            = computation.GetComputation<TSource>();
        return source
            .Select(itemSelector)
            .Select(computationExpression)
            .Select(resultSelector);
    }

    public static IQueryable<TResult> Compute<TItem, TValue, TResult>(
        this IQueryable<TItem> source,
        Computation<TItem, TValue> computation,
        Expression<Func<ComputedOutput<TItem, TValue>, TResult>> resultSelector)
    {
        return source.Compute(computation,
            x => new ComputedInput<TItem, TItem>
            {
                Source = x,
                Item = x
            },
            resultSelector);
    }

    public static IQueryable<ComputedOutput<TItem, TValue>> Compute<TItem, TValue>(
        this IQueryable<TItem> source,
        Computation<TItem, TValue> computation)
    {
        return source.Compute(computation, x => x);
    }
}
```

Don't worry about those too much - they look complicated, but they're really not too bad. Mostly they're just about applying the projections for the input, output, and computation to the query.

## Example

Let's look at an example entity:

```
public class Car
{
    public int Axles { get; set; }
    
    [NotMapped]
    public int Wheels
    {
        get { return Axles * 2; }
    }
}
```

If you wanted to use `Wheels` in a query, you'd have to materialize the query first:

```
// This would throw an exception because LINQ to Entites doesn't know about Car.Wheels
var result = context.Cars.Where(car => car.Wheels > 4).ToList();

// This would work, but you'd be getting all your cars first and then applying the predicate
var result2 = context.Cars.ToList().Where(car => car.Wheels > 4);

// This would also work, but now I have to write the wheels logic in two places
// and if it changes in one, I have to remember to change it in the other
var result3 = context.Cars
    .Select(car => new
    {
        Car = car,
        Wheels = car.Axles * 2
    })
    .Where(proj => proj.Wheels > 4)
    .Select(proj => proj.Car)
    .ToList();
```

Using the new `Computation<TItem, TValue>` class, I can write my computation and put it into a static class for easy access:

```
public static class CarComputations
{
    private class WheelsCalculation : Computation<Car, int>
    {
        public override Expression<Func<ComputedInput<TSource, Car>, ComputedOutput<TSource, int>>> GetComputation<TSource>()
        {
            return input => new ComputedOutput<TSource, int>
            {
                Source = input.Source,  // This just passes through our source object to the output projection
                Value = input.Item.Axles * 2 
            };
        }
    }
    
    public static Computation<Car, int> Wheels = new WheelsCalculation();
}
```

Let's look at that query again. Now I can write:

```
var result4 = context.Cars
    .Compute(CarComputations.Wheels)
    .Where(proj => proj.Value > 4)
    .Select(proj => proj.Source);
```

And finally, let's change the `Car.Wheels` property to also use the same calculation:

```
public class Car
{
    public int Axles { get; set; }
    
    [NotMapped]
    public int Wheels
    {
        get { return CarComputations.Wheels.GetValue(this); }
    }
}
```

Tada! If you've made it this far, you've probably had a similar problem and were hopefully able to figure out what was going on. I'm interested if anyone has feedback or improvements - this technique has been working pretty well for me, but I'm always open to suggestions.