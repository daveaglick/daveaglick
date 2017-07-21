Title: Using The Same Object For Server And Client View Models
Image: /images/modelcars.jpg
Published: 7/21/2017
Tags:
  - ASP.NET
  - ASP.NET MVC
  - Vue.js
  - magic strings
---
This short post explores a really simple technique for using the same C# object as a view model on both the client and server.

I've become very fond of using [Vue](https://vuejs.org/) for writing the client-side UI code that I used to use [jQuery](https://jquery.com/) and custom JavaScript for. I'm talking specifically about adding a bit of client interactivity to traditional, server-rendered ASP.NET MVC views ([though I hear Vue is great at Node-based server-side rendering as well](https://vuejs.org/v2/guide/ssr.html)).

# The Traditional ASP.NET MVC Way

In the past my client code usually didn't have it's own model, or at least it wasn't exposed so directly. For example, consider a simple C# server-side view model for a view that lets the user change their contact information:

```
public class ContactInfoViewModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

I may have used that model in my ASP.NET MVC Razor view like this (or in ASP.NET Core I might use a [tag helper](http://www.davepaquette.com/archive/2015/05/11/cleaner-forms-using-tag-helpers-in-mvc6.aspx)):

```
@model ContactInfoViewModel

@using(Html.BeginForm("Edit", "ContactInfo", FormMethod.Post))
{
    <div>@Html.TextBoxFor(m => m.FirstName)</div>
    <div>@Html.TextBoxFor(m => m.LastName)</div>
    <div><input type="submit">Submit</input></div>
}
```

The result is that the form elements in the HTML rendered on the server and sent to the client has the correct name of all the properties in my view model so that when I post it back, the model binder can match everything up and hydrate a new instance of my C# view model class:

```
public class ContactInfoController : Controller
{
    // ...

    [HttpPost]
    public ActionResult Edit(ContactInfoViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Do stuff with model data...
        }
    }
}
```

Notice that I never created a JavaScript object to act as a model on the client. The form elements and their names and values essentially act like the client "model".

# The Vue Client View Model

One thing that's taken a little getting used with Vue (and presumably other similar frameworks like [React](https://facebook.github.io/react/)) is the notion of the client data model. In Vue, you need to create a JavaScript object that Vue uses for it's two-way data binding functionality:

```
var app = new Vue({
    el: '#app',
    data: {
        firstName: null,
        lastName: null
    },
    methods: {
        submit: function() {
            // Do data submission here...
        }
    }
})
```

Then your form might look like:

```
<div id="app">
    <form>
        <div><input v-model="firstName"></div>
        <div><input v-model="lastName"></div>
        <div><button type="button" v-on:click.prevent="submit">Submit</button></div>
    </form>
</div>
```

The problem here is that I'm now duplicating the structure and date of my server-side C# view model inside JavaScript. What happens if I change the name of one of the C# view model properties but forget to change it in the JavaScript code? This is an example of [dreaded magic strings](/posts/eliminating-magic-strings) because I'm expecting the names in my JavaScript (essentially strings) are going to match my strongly-typed C# object when I post it back and bind. There has to be a better way...

# Generating The Client Object

I asked about this on Twitter and got a ton of great feedback:

<blockquote class="twitter-tweet" data-partner="tweetdeck"><p lang="en" dir="ltr">Anyone have tricks for keeping their client-side view model (I.e., Vue) in sync with backend model (I.e., object you&#39;re binding to on post)?</p>&mdash; Dave Glick (@daveaglick) <a href="https://twitter.com/daveaglick/status/888378843123331073">July 21, 2017</a></blockquote>
<script async src="//platform.twitter.com/widgets.js" charset="utf-8"></script>

It was suggested I check out some libraries that would help:

* [typescript-cs-poco](https://github.com/ffMathy/typescript-cs-poco)
* [TypeScriptDefinitionsGenerator](https://github.com/slovely/TypeScriptDefinitionsGenerator)
* [Typewriter](https://frhagn.github.io/Typewriter/index.html)

All of these look great and are variations on the theme of using code generation to create TypeScript files for C# classes. I'm not sure I want to add TypeScript support to my application just for this though, so in cases where I'm not already using TypeScript I wanted to find an easier way.

It turns out there's already a library that knows how to turn basically any C# object structure into JavaScript and we all already have it installed: [JSON.NET](http://www.newtonsoft.com/json). Unfortunately, JSON.NET turns an object into JSON which isn't quite the same thing as an actual JavaScript object. Fortunately, there's `JSON.parse()` which can take that JSON string representation of our view model and turn it into a full JavaScript object.

What I ended up with were two relatively simple HTML helpers, one for when you don't already have an instance of the view model on the server (I.e., you just need to post the view model back) and one when you do (I.e., you're passing existing data to the view to be manipulated on the client and then posted back):

```
public static class HtmlHelpers
{
    // ...

    public static MvcHtmlString GetClientModel<TModel>(this HtmlHelper helper)
        where TModel : new() =>
        GetClientModel(helper, new TModel());

    public static MvcHtmlString GetClientModel<TModel>(this HtmlHelper helper, TModel model)
    {
        string escapedJson = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.None)
            .Replace("\\\"", "\\\\\"")
            .Replace("\"", "\\\"");
        return new MvcHtmlString($"JSON.parse(\"{escapedJson}\")");
    }
}
```

Notice that this helper doesn't actually create a valid JavaScript string of the model. Instead it relies on the client to convert the JSON representation of the view model to an object using `JSON.parse()`. While this may seem strange, it ensures we don't get any strange conversion behavior since we're using JavaScript to perform the conversion directly. That does mean we need to be careful about escaping which is why `\` and `"` are double-escaped using the replace calls above.

Now that we have the helper, we can use our server-side view model to create the JavaScript object for Vue and reference it in the form elements:

```
<div id="app">
    <form>
        <div><input v-model="@(nameof(ContactInfoViewModel.FirstName))"></div>
        <div><input v-model="@(nameof(ContactInfoViewModel.LastName))"></div>
        <div><button type="button" v-on:click.prevent="submit">Submit</button></div>
    </form>
</div>

<script>
    var app = new Vue({
        el: '#app',
        data: @(Html.GetClientModel<ContactInfoViewModel>()),
        methods: {
            submit: function() {
                // Do data submission here...
            }
        }
    })
</script>
```

No more magic strings! When you serialize the Vue data model as JSON and post it back (using jQuery, [Axios](https://github.com/mzabriskie/axios), or whatever else) the ASP.NET model binder will automatically map the JSON object back onto your server-side view model and your postback controller action will get a fully instantiated representation of what Vue was tracking on the client. Here's an example Vue `submit` function:

```
<script>
    var app = new Vue({
        el: '#app',
        data: @(Html.GetClientModel<ContactInfoViewModel>()),
        methods: {
            submit: function() {
                var self = this;
                $.ajax({
                    method: "POST",
                    processData: false,
                    contentType: 'application/json',
                    dataType: 'json',
                    data: JSON.stringify(self.$data)
                });
            }
        }
    })
</script>
```