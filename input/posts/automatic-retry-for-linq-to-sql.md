Title: Automatic Retry for LINQ to SQL
Lead: A new library that makes retrying transient failures in LINQ to SQL easier.
Published: 12/23/2014
Tags:
  - LINQ
  - LINQ to SQL
---
<p><strong><a href="https://github.com/daveaglick/LinqToSqlRetry">GitHub</a></strong></p>
<p><strong><a href="http://www.nuget.org/packages/LinqToSqlRetry/">NuGet</a></strong></p>

<p><a href="https://github.com/daveaglick/LinqToSqlRetry">LinqToSqlRetry</a> is a simple library to help manage retries in LINQ to SQL. This is particularly important in cloud-based infrastructures like Azure where transient failures are not uncommon. And despite the popularity of Entity Framework, Dapper, and other ORM or data access libraries, there is still a place for simple LINQ to SQL code.</p>

<p>Retry logic is provided via extension methods, so you will need to bring the <code>LinqToSqlRetry</code> namespace into scope in every file you need retry logic:</p>

<pre class="prettyprint">using LinqtoSqlRetry;</pre>
 
<h2>Retry On Submit Changes</h2>

<p>Instead of using <code>DataContext.SubmitChanges()</code> just use <code>DataContext.SubmitChangesRetry()</code>:</p>

<pre class="prettyprint">using(var context = new MyDbContext())
{
  context.Items.InsertOnSubmit(new Item { Name = "ABC" });
  context.SubmitChangesRetry();
}</pre>

<h2>Retry on Queries</h2>

<p>Add the <code>Retry()</code> extension method to the end of your queries. It should generally be the last extension you use before materializing the query (which happens when you use extensions like <code>ToList()</code> or <code>Count()</code>):</p>

<pre class="prettyprint">using(var context = new MyDbContext())
{
  int count = context.Items.Where(x => x.Name == "ABC").Retry().Count();
}</pre>


<h2>Retry Policy</h2>

<p>The retry logic is controlled by a policy that indicates when a retry should take place and how long to wait before retrying the operation. Two policies are supplied:</p>

* <code>LinearRetry</code> retries a specific number of times (3 by default) and waits a specified amount each time (10 seconds by default).
* <code>ExponentialRetry</code> retries a specific number of times (3 by default) and waits an increasing multiple of time (5 seconds by default) after an initial wait on the first retry (10 seconds by default).

<p>By default the <code>LinearRetry</code> policy is used. The policy that you use and it's settings can be changed by passing it in to any of the extension methods:</p>

<pre class="prettyprint">using(var context = new MyDbContext())
{
  // Start with a 4 second wait, increasing by a factor of 2, for 5 attempts
  var retryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(2), 5);
  context.Items.InsertOnSubmit(new Item { Name = "ABC" });
  context.SubmitChangesRetry(retryPolicy);
  int count = context.Items.Where(x => x.Name == "ABC").Retry(retryPolicy).Count();
}</pre>

<p>You can specify your own custom policies to do things like log retry attempts, use more complex logic, retry on different types of errors, etc. by implementing <code>IRetryPolicy</code>.</p>

<h2>Retry for Arbitrary Operations</h2>

<p>You can also retry any arbitrary operation with the Retry() extension methods on any IRetryPolicy object. There are two of them, one takes an <code>Action</code> and the other takes a <code>Func&lt;t&gt;</code> and returns the result. For example:</p>

<pre class="prettyprint">var retryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(2), 5);
retryPolicy.Retry(() => AMethodThatMightFail());</pre>

<h2>Under the Hood</h2>

<p>The retry logic under the hood is fairly simple:</p>

<pre class="prettyprint">int retryCount = 0;
while (true)
{
    try
    {
        return func();
    }
    catch (Exception ex)
    {
        TimeSpan? interval = retryPolicy.ShouldRetry(retryCount, ex);
        if (!interval.HasValue)
        {
            throw;
        }
        Thread.Sleep(interval.Value);
    }
    retryCount++;
}</pre>

<p>In the code above, the function in the call to <code>func()</code> and the <code>retryPolicy</code> object are provided based on usage. This just gives you an idea what's going on during the retry loop. Just look in <a href="https://github.com/daveaglick/LinqToSqlRetry">the GitHub repository</a> for more information.</p>