Title: Custom Entity Type Configurations in Entity Framework Code First (Part 1)
Published: 4/17/2013
Tags:
  - Entity Framework
  - Entity Framework Code First
---
<p>One of the things I really like about Entity Framework Code First is the way you can mix declarative configuration (I.e., by using <a href="http://msdn.microsoft.com/en-us/library/dd901590(v=vs.95).aspx">Data Annotation</a> attributes) with programmatic configuration for more complicated cases (I.e., by using the <a href="http://msdn.microsoft.com/en-US/data/jj591617">fluent API</a>). The one aspect of this that really bothers me though is that in normal usage the fluent API commands end up being placed inside your <code><a href="http://msdn.microsoft.com/en-us/library/system.data.entity.dbcontext(v=vs.103).aspx">DbContext</a></code> class removed from your actual entity. If you change some aspect of an entity that uses the fluent API for configuration, you have to remember to go check the <code><a href="http://msdn.microsoft.com/en-us/library/system.data.entity.dbcontext.onmodelcreating(v=vs.103).aspx">OnModelCreating()</a></code> method to ensure you don't need to modify the code-based configuration. It would be much better (in my opinion) if all configuration, declarative and programmatic, were located close to the entity and/or encapsulated within it. This article explains one way of accomplishing this.</p>

<p>The first thing you'll need to understand is the way that the fluent API actually configures entities. Inside of the <code><a href="http://msdn.microsoft.com/en-us/library/system.data.entity.dbcontext(v=vs.103).aspx">DbContext</a></code> class (which you've presumably subclassed) there is an overridable method called <code><a href="http://msdn.microsoft.com/en-us/library/system.data.entity.dbcontext.onmodelcreating(v=vs.103).aspx">OnModelCreating()</a></code>. This method has a single parameter of type <code><a href="http://msdn.microsoft.com/en-us/library/system.data.entity.dbmodelbuilder(v=vs.103).aspx">DbModelBuilder</a></code>. During normal fluent API usage you write code that looks like this inside the <code>OnModelCreating()</code> method:</p>

<pre class="prettyprint">modelBuilder.Entity&lt;Department&gt;().Property(t =&gt; t.Name).IsRequired();</pre>

<p>When you call <code><a href="http://msdn.microsoft.com/en-us/library/gg696542(v=vs.103).aspx">DbModelBuilder.Entity&lt;TEntityType&gt;()</a></code>, you get back an <code><a href="http://msdn.microsoft.com/en-us/library/gg696117(v=vs.103).aspx">EntityTypeConfiguration&lt;TEntityType&gt;</a></code> class that is used for configuring the entity. However, this isn't the only way to get an <code>EntityTypeConfiguration</code> class. You can actually create them yourself:</p>

<pre class="prettyprint">public class DepartmentTypeConfiguration : EntityTypeConfiguration&lt;Department&gt;
{
  public DepartmentTypeConfiguration() { }
}</pre>

<p>Once you've instantiated one, you can use it just like you would have used the one you obtained from the <code><a href="http://msdn.microsoft.com/en-us/library/gg696542(v=vs.103).aspx">DbModelBuilder.Entity&lt;TEntityType&gt;()</a></code> call:</p>

<pre class="prettyprint">DepartmentTypeConfiguration departmentConfig = new DepartmentTypeConfiguration();
departmentConfig.Property(t =&gt; t.Name).IsRequired();</pre>

<p>The previous example was just to show that the custom <code>EntityTypeConfiguration</code> class works the same way as the ones you obtain by calling <code>DbModelBuilder.Entity&lt;TEntityType&gt;()</code>. Alternatively you can specify configuration code in the constructor, which is more useful because it means the configuration code will get called whenever a new instance of your <code>EntityTypeConfiguration</code> class is created (I.e., through reflection).</p>

<pre class="prettyprint">public class DepartmentTypeConfiguration : EntityTypeConfiguration&lt;Department&gt;
{
  public DepartmentTypeConfiguration()
  {
    Property(t =&gt; t.Name).IsRequired();
  }
}</pre>

<p>The fluent API calls (such as <code>Property()</code>) change the internal state of the <code>EntityTypeConfiguration</code> class. When all of the configuration is complete, Entity Framework reads the state of all <code>EntityTypeConfiguration</code> classes that have been registered and uses them to build the model. But back up a step, notice I said "all <code>EntityTypeConfiguration</code> classes <em>that have been registered</em>". There is one more step before a custom <code>EntityTypeConfiguration</code> class can be used for configuration - it has to be registered with the <code><a href="http://msdn.microsoft.com/en-us/library/system.data.entity.modelconfiguration.configuration.configurationregistrar(v=vs.103).aspx">ConfigurationRegistrar</a></code>. To do so, you just use the <code>DbModelBuilder.Configurations</code> property:</p>

<pre class="prettyprint">modelBuilder.Configurations.Add(departmentConfig);</pre>

<p>This adds the custom <code>EntityTypeConfiguration</code> instance to the list of configurations that will be used to build the final model. At this point, we could just reflect over the assembly looking for <code>EntityTypeConfiguration</code> classes, instantiating them, and adding them to the <code>ConfigurationRegistrar</code> (<a href="http://areaofinterest.wordpress.com/2010/12/08/dynamically-load-entity-configurations-in-ef-codefirst-ctp5/">as described by Jonas Cannehag</a>):</p>

<pre class="prettyprint">var typesToRegister = Assembly.GetAssembly(typeof(YourDbContext)).GetTypes()
  .Where(type =&gt; type.Namespace != null
    &amp;&amp; type.Namespace.Equals(typeof(YourDbContext).Namespace))
  .Where(type =&gt; type.BaseType.IsGenericType
    &amp;&amp; type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration&lt;&gt;));

foreach (var type in typesToRegister)
{
  dynamic configurationInstance = Activator.CreateInstance(type);
  modelBuilder.Configurations.Add(configurationInstance);
}</pre>

<p>This will allow you to create as many custom EntityTypeConfiguration classes as you need for each entity in your model. However, there are some limitations:</p>

* The <code><a href="http://msdn.microsoft.com/en-us/library/gg696203(v=vs.103).aspx">ConfigurationRegistrar.Add()</a></code> method only allows one <code>EntityTypeConfiguration</code> class per entity type. This may be a problem in complex models if you have some configurations for a given entity spread out in multiple places (for example, you want to place the responsibility of configuring relationships for a given entity near the entities on the other side of the relationships).
* I personally find the idea of placing configuration code inside the constructor of a dedicated class a little awkward. I would prefer to have my custom configurations specified through an interface that I could implement right on the entity, or perhaps use more than once to specify configuration for multiple entities in a single configuration class. That would give more flexibility.

<p>In <a href="/posts/custom-entity-type-configurations-in-entity-framework-code-first-part-2">my next post</a> I'll discuss an alternate method of specifying custom entity type configurations that builds on this technique and addresses these two points.</p>
