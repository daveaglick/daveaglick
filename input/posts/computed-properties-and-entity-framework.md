Title: Computed Properties and Entity Framework
Lead: How to use your computed properties in predicates and projections.
Published: 12/11/2014
Tags:
  - Entity Framework
  - LINQ
  - database
  - LINQ to Entities
---
<p>If you're using an ORM, it's not uncommon to have computed properties in addition to the ones that are stored directly in the database. Unfortunatly, these computed properties don't work with Entity Framework out of the box. In this post I'm going to discuss the problem and suggest various ways of mitigating it.</p>
    
<p>For example, consider the following entities that I will use as an example throughout this post.</p>

```
public class Customer
{
  public string FirstName { get; set; }
  public string LastName { get; set; }
  public virtual ICollection<Holding> Holdings { get; private set; }

  [NotMapped]
  public decimal AccountValue
  {
    get { return Holdings.Sum(x => x.Value); }
  }
 
  [NotMapped]
  public string FullName
  {
    get { return FirstName + " " + LastName; }
  }
}

public class Holding
{
  public virtual Stock Stock { get; set; }
  public int Quantity { get; set; }

  [NotMapped]
  public decimal Value
  {
    get { return Quantity * Stock.Price; }
  }
}

public class Stock
{
  public string Symbol { get; set; }
  public decimal Price { get; set; }
}
```
 
<p>As you can see, there are several computed properties including one that aggregates data from a collection and another that uses data from a reference. These properties work fine once you've obtained the entities and want to work with them. The problems start when you want to use them within a query or as part of a projection. For example, the following query will throw an exception:</p>

```
var customers = ctx.Customers.Where(c => c.AccountValue > 10);
```

<p>Likewise, this projection will also fail:</p>

```
var result = ctx.Customers
  .Select(c => new
  {
    FullName = c.FullName,
    AccountValue = c.AccountValue
  });
```
      
<p>In the first case we're trying to use a computed property within a predicate, but Entity Framework doesn't know how to convert <code>AccountValue</code> to SQL code. In the projection, Entity Framework doesn't know how to convert <code>FullName</code> to SQL code either. In both cases, it will fail to formulate a SQL query and throw an exception because it just doesn't know what to do with the properties that aren't directly stored in the database. Let's take a look at some of the ways we can get around this problem.</p>

<h1>Don't Use Computed Properties</h1>

<p>Part of the problem here is that Entity Framework and LINQ to Entities just doesn't know how to look at the actual code for each property. It tries to map the property name to the database and when it doesn't find a match it gives up. However, Entity Framework does actually know how to translate a large number of primitive operations and LINQ methods into SQL code. They just can't be hidden behind properties. The queries above could be rewritten as:</p>

```
var customers = ctx.Customers.Where(c => c.Holdings.Sum(h => h.Quantity * h.Stock.Price) > 10);
```

```
var result = ctx.Customers
  .Select(c => new
  {
    FullName = c.FirstName + " " + c.LastName,
    AccountValue = c.Holdings.Sum(h => h.Quantity * h.Stock.Price)
  });
```

<p>Notice how the code for each computed property is used directly in the query. Also notice that in the case of <code>AccountValue</code> we had to not only expand it's code, but also the code of <code>Holding.Value</code>. There can be no computed properties anywhere in the query code.</p>

<p>By now, it should be obvious why this doesn't work for anything but the simplest cases. It can become very complex when you have nested computed properties. It's also not DRY because you end up repeating the computation code in every query. And when the underlying logic changes you have to go find every query and change that too.</p>

<h1>Materialize The Entities</h1>

<p>This is the most common suggestion for getting around this problem. Entity Framework only has an issue if you try and execute the computed properties <em>on the database server</em>. It works just fine when you run the computations against fully materialized entities. To make this work you just have to materialize the entities (I.e., fetch data from the database server) <em>before</em> using any computed properties. With this approach the queries would look like:</p>

```
var customers = ctx.Customers.ToList().Where(c => c.AccountValue > 10);
```

```
var result = ctx.Customers
  .ToList()
  .Select(c => new
  {
    FullName = c.FullName,
    AccountValue = c.AccountValue
  });
```

<p>See that extra call to <code>.ToList()</code> in there? That forces Entity Framework to go to the database, execute whatever portion of the query it's seen thus far, and return a <code>List&lt;T&gt;</code> that can continue to be operated on. By the time the above queries get to the <code>.Where()</code> and <code>.Select()</code> calls, we're operating on a <code>List&lt;Customer&gt;</code> collection instead of on the database.</p>

<p>Let's think about what this means though. If we're fetching data from the database <em>before</em> executing the <code>.Where()</code> condition, then we're pulling down <em>all the rows!</em> Likewise, by going to the database before the <code>.Select()</code> projection, we're fetching <em>all the columns</em> and filtering after we've gotten them. This works okay for small tables and simple data. However, with any meaningful database it falls apart and your performance will quickly suffer.</p>

<h1>Encapsulate</h1>

<p>This approach gets a little more complicated. In this case we'll put the computed logic inside an extension method that we can use as if it were an actual LINQ operation. It will take the unmaterialized query and apply whatever additional computation we need. It's essentially like the first example of just not using computed properties except it puts the logic somewhere where it can be reused from other queries. Given the following extension method:</p>

```
public static IQueryable<Customer> WhereAccountValueIsHigh(this IQueryable<Customer> customers)
{
  return customers.Where(c => c.Holdings.Sum(h => h.Quantity * h.Stock.Price) > 10);
}
```

<p>We can now write:</p>

```
var customers = ctx.Customers.WhereAccountValueIsHigh();
```

<p>That extension can be reused wherever we want customers with high account values. Likewise, for the projection we could create the following extension:</p>

```
public class CustomerData
{
  string FullName { get; set; }
  decimal AccountValue { get; set; }
}

public static IQueryable<CustomerData> SelectCustomerData(this IQueryable<Customer> customers)
{
  return customers.Select(c => new CustomerData
  {
    FullName = c.FirstName + " " + c.LastName,
    AccountValue = c.Holdings.Sum(h => h.Quantity * h.Stock.Price)
  });
}
```

<p>Notice that I also need an actual projection type for the extension in this case, otherwise it wouldn't know what to use for the generic parameter of the return <code>IQueryable</code>. This extension can be called like:</p>

```
var customers = ctx.Customers.SelectCustomerData();
```

<p>Both extensions let you move the computation somewhere that is at least a little bit reusable. For example, I could now also write the following to chain them together:</p>

```
var customers = ctx.Customers.WhereAccountValueIsHigh().SelectCustomerData();
```

<p>We still have a number of problems with this approach. The first is that we still have to duplicate the computation within the property and then again within the extension. This may be fine if you have a computation that you really only need to use in queries (in which case you can remove it as a property), but if not you're still stuck maintaining it in two places. You also still have to expand any nested called to computed properties. For example, if the <code>.WhereAccountValueIsHigh()</code> extension had used the <code>Holding.Value</code> property directly, you still would have gotten an exception. This approach does not work around the limitations of the LINQ to Entity query provider not recognizing computed properties, it just puts the actual computations somewhere a little more reusable. Finally, this approach is not very composable. If you had another view that needed an additional property (like the customers initials) you would either need to create an additional extension for the new view or add the initials calculation to <code>.SelectCustomerData()</code> and then do an additional projection to remove it from the first view that doesn't need it.</p>

<h1>Expression Trees and LINQ to Entities</h1>

<p>All of the approaches so far have attempted to work around the problem by ensuring that the computed properties never reach LINQ to entities and therefore never get translated to SQL. Before looking at a different type of approach, let's back up a moment and consider how LINQ to entities actually does translate all those LINQ methods and primitive operations to SQL. Under the hood, LINQ to entities reads <a href="http://msdn.microsoft.com/en-us/library/bb397951.aspx">Expression Trees</a>, which is a fundamental concept of the entire LINQ system. Essentially, expression trees represent code in a high-level abstraction. LINQ to entities knows how to understand an expression tree and convert most (but not all) of the corresponding commands into SQL statements. In addition, all of the LINQ extensions like <code>.Where()</code> and <code>.Select()</code> essentially build up the expression tree that LINQ to entities eventually processes.</p>

<p>The question then becomes, how can we pass our own expression tree to LINQ to entities? And how can be construct such an expression tree that represents the logic in our computed properties? Thankfully, there are a number of folks who've talked this very question.</p>

<h2>Write Your Own Expressions</h2>

<p>One approach would be to just write your own `Expression`. For example:</p>

```
public readonly Expression<Func<Customer, bool>> AccountValueIsHigh = c => c.Holdings.Sum(h => h.Quantity * h.Stock.Price) > 10;
```

<p>This can then be used like:</p>

```
var customers = ctx.Customers.Where(AccountValueIsHigh);
```

<p>One problem with this approach is that we can't use an <code>Expression</code> directly in a <code>.Select()</code> call because the compiler has no idea how to convert the <code>Expression</code> into something that can be assigned.</p>

<h2>LINQ Expression Projection</h2>

<p>To address the problem I just mentioned and provide a way to use your own <code>Expression</code> object directly in <code>.Select()</code> projection calls, Asher Barak has created a project called <a href="http://linqexprprojection.codeplex.com/">LINQ Expression Projection</a>. It was originally explained in <a href="http://www.codeproject.com/Articles/402594/Black-Art-LINQ-expressions-reuse">this CodeProject article</a> and allows you to essentially wrap your expression in something that can be used as an assignment. For example, given the following <code>Expression</code>:</p>

```
public readonly Expression<Func<Customer, decimal>> AccountValueExpression = c => c.Holdings.Sum(h => h.Quantity * h.Stock.Price);
```

<p>You would write:</p>

```
var customers = ctx.Customers
  .AsExpressionProjectable()
  .Select(c => new
  {
    AccountValue = AccountValueExpression.Project<decimal>()
  });
```

<p>Notice the extra call to <code>.AsExpressionProjectable()</code>, which prepares the query for projection and sets up a scan for all instances of the <code>.Project&lt;T&gt;()</code> extension method. Once found, the library swaps the appropriate <code>Expression</code> in for the assignment at runtime.</p>

<h2>Linq.Translations</h2>

<p><a href="http://damieng.com/">Damien Guard</a>, <a href="http://davidfowl.com/">David Fowler</a>, and Colin Meek has done something similar in a much more generic way. Their <a href="https://github.com/damieng/Linq.Translations">Linq.Translations library</a> was first described in <a href="http://damieng.com/blog/2009/06/24/client-side-properties-and-any-remote-linq-provider">a blog post on Damien's blog</a>. The idea is that you set up an <code>Expression</code>-like variable that can be used within the computed property. Then when you write a query, you can just use the computed property without worrying about the <code>Expression</code> at all. The only extra thing you need is to make a final call to <code>.WithTranslations()</code> in the LINQ query to convert all the computed properties to their <code>Expression</code> equivalents. In usage, it would look something like this:</p>

```
public class Customer
{
  public string FirstName { get; set; }
  public string LastName { get; set; }

  private static readonly CompiledExpression<Customer, string> fullNameExpression
     = DefaultTranslationOf<Customer>.Property(e => e.FullName).Is(e => e.FirstName + " " + e.LastName);

  [NotMapped]
  public string FullName
  {
    get { return fullNameExpression.Evaluate(this); }
  }
}
```

```
var customers = ctx.Customers
  .Select(c => new
  {
    FullName = c.FullName
  })
  .WithTranslations();
```

<p>While this is somewhat similar to the <a href="http://linqexprprojection.codeplex.com/">LINQ Expression Projection</a> project, it trades complexity during definition for ease of use in queries.</p>

<h2>DelegateDecompiler</h2>

<p>Hopefully you're still with me because this last project blew my mind. I first learned about <a href="https://github.com/hazzik/DelegateDecompiler">DelegateDecompiler</a> from <a href="http://lostechies.com/jimmybogard/2014/05/07/projecting-computed-properties-with-linq-and-automapper/">this blog post</a> on <a href="http://lostechies.com/jimmybogard/">Jimmy Bogard's blog</a>. While <a href="https://github.com/hazzik/DelegateDecompiler">DelegateDecompiler</a> still attempts to pass an <code>Expression</code> to the query provider, it takes the burden of creating the <code>Expression</code> out of your hands. It does this by compiling any computed properties into IL then decompiling the IL back into an expression tree. This way it can transparently handle just about any computation you give it including nested computed properties, custom methods, etc. The one caveat is that even though it may seem like it, it's not magic and can't make Entity Framework and LINQ to Entities recognize methods, classes, etc. that they wouldn't have if you had used them directly.</p>

<p>Using DelegateDecompiler is stupidly simple. You just decorate your computed properties with <code>ComputedAttribute</code> and then call <code>.Decompile()</code> in your query. For example:</p>

```
public class Customer
{
  public string FirstName { get; set; }
  public string LastName { get; set; }

  [NotMapped]
  [Computed]
  public string FullName
  {
    get { return FirstName + " " + LastName; }
  }
}
```

```
var customers = ctx.Customers
  .Select(c => new
  {
    FullName = c.FullName
  })
  .Decompile();
```

<p>That's all there is to it. There's even a <a href="https://www.nuget.org/packages/DelegateDecompiler.EntityFramework/">DelegateDecompiler.EntityFramework</a> library that you can use if you want to support more advanced Entity Framework functionality such as async queries (though it's not necessary for simple Entity Framework usage). In fact, you can make it even easier to use by automatically processing <code>NotMappedAttribute</code> to indicate computed properties. To do this, just create a new <code>Configuration</code> class:</p>

```
public class DelegateDecompilerConfiguration : DefaultConfiguration
{
    public override bool ShouldDecompile(MemberInfo memberInfo)
    {
        // Automatically decompile all NotMapped members
        return base.ShouldDecompile(memberInfo) || memberInfo.GetCustomAttributes(typeof(NotMappedAttribute), true).Length > 0;
    }
}
```

<p>And then register it like this:</p>

```
DelegateDecompiler.Configuration.Configure(new DelegateDecompilerConfiguration());
```

<h1>Conclusions</h1>

<p>As you can see, there are a lot of different ways of working around the lack of support for computed properties in Entity Framework and LINQ to Entities. Which approach you choose depends largely on the factors of your project such as database complexity, query complexity, reuse requirements, etc. Hopefully one of these methods will help you tame all that computation and help release your data from your database.</p>