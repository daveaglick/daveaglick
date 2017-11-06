Title: Using ASP.NET ModelState With Vue.js
Image: /images/stamp.jpg
Published: 11/6/2017
Tags:
  - ASP.NET
  - ASP.NET MVC
  - Vue.js
---
Recently I've been using more and more [Vue.js](https://vuejs.org/) in the client code for my ASP.NET MVC websites. It provides a great balance between modern interactive client functionality and server logic. However, one area that's been troubling me is how to integrate the powerful built-in `ModelState` [server-side validation framework](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation#model-state) with the client, particularly in WebAPI or other service-based scenarios. Ideally, I'd like to display server-side validation errors on the client when data fails to validate due to validation attributes like `[Required]`. This article shows one way to do that.

# Server

The first thing we need to do is get the `ModelState` to the client in the first place. The whole `ModelState` object is too complex for our needs so I've written a helper method that can take a `ModelStateDictionary` and convert it to a `ContentResult` suitable for returning from an action:

```
public static ErrorContentResult ErrorsToJsonResult(this ModelStateDictionary modelState)
{
    IEnumerable<KeyValuePair<string, string[]>> errors = modelState.IsValid
        ? null
        : modelState
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray())
            .Where(m => m.Value.Any());
    return new ErrorContentResult
    {
        Content = JsonConvert.SerializeObject(errors, settings),
        ContentType = "application/json"
    };
}
```

You can use this helper from any action:

```
// Do some processing...
if(!ModelState.IsValid)
{
    ModelState.ErrorsToJsonResult();
}
// Finish up and return a success response
```

After using this, if the model state is invalid it'll return an error response and JSON payload that looks like this:

```
[  
  {  
    "Key":"Foo",
    "Value":[  
      "The Foofield is required."
    ]
  },
  {  
    "Key":"Bar",
    "Value":[  
      "The Barfield is required."
    ]
  }
]
```

# Client

Now that the server is returning our model state errors as JSON, let's take a look at how we can process that in Vue.js. The trick is to put the errors somewhere where arbitrary Vue components can react to them. This method will concatenate the model state JSON we're sending back for each key and then set a top-level data property called `modelState` in the Vue model `v`:

```
processModelState: function(response, vm) {
    if(response) {
        $.each(response, function(i, state) {
            var message = "";
            var stateErrors = response[i].Value;
            if(stateErrors)
            {
                $.each(stateErrors, function (j, stateError) {
                    if (j > 0) {
                        message += "; ";
                    }
                    message += stateError;
                });
                vm.$set(vm.modelState, state.Key, message);
            }
        });
    }
}
```

Notice that we have to use `$set` to create the `modelState` properties. That's because [Vue can't react to new properties](https://vuejs.org/v2/guide/reactivity.html#Change-Detection-Caveats). The `Vue.set` method (and it's alias `$set`) adds a pre-wrapped reactive property to an object.

This can then be used within an event handler like this:

```
submit: function(event) {
    // Clear the modelState before submission
    for (var error in this.modelState) {
        delete this.modelState[error];
    }
    // Need to capture this
    var self = this;
    $.ajax({
        method: "POST",
        processData: false,
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(self.model),
        error: function(error) {
            // Here's where the model state errors get set
            self.processModelState(error.responseJSON, self);
        },
        success: function(data) {
            // ...
        }
    });
}
```

Now we need to rig up the UI to react to items in the `modelState` Vue model property. For example, if you're using horizontal forms in [Bootstrap](https://getbootstrap.com/docs/3.3/), this might look something like:

```
<div class="form-group" :class="{'has-error': modelState['Foo'] !== undefined}">
    <label class="col-sm-4 control-label">Document</label>
    <div class="col-sm-8 form-control-static">
        <input v-model="model.Foo">
        <div class="help-block">Please enter some data.</div>
        <div class="help-block" v-if="modelState['Foo'] !== undefined"><strong><span class="k-icon k-i-warning"></span> {{ modelState['Foo'] }}</strong></div>
    </div>
</div>
```

That will set the `has-error` CSS class on the outer `form-group` if there are model state errors for the given key and will display a bold `help-block` with the error message(s). We can do better though. Here's a component that does something similar:

```
<script type="x-template" id="form-group-template">
    <div class="form-group" :class="{'has-error': error !== undefined}">
        <label v-if="label !== undefined" class="col-sm-4 control-label">{{ label }}</label>
        <div class="form-control-static col-sm-8" :class="{'col-sm-offset-4': label === undefined}">
            <slot></slot>
            <div class="help-block" v-if="help">{{ help }}</div>
            <div class="help-block" v-if="error !== undefined"><strong><span class="k-icon k-i-warning"></span> {{ error }}</strong></div>
        </div>
    </div>
</script>

<script>
    Vue.component('form-group', {
        template: '#form-group-template',
        props: ['label', 'help', 'error']
    });
</script>
```

Which lets us write this:

```
<form-group label="Document" help="Please enter some data." :error="modelState['Foo']">
    <input v-model="model.Foo">
</form-group>
```

You would need to tweak the component and CSS usage depending on what CSS framework you're using and how you want everything to look, but hopefully you get the idea. I'm curious if anyone else is doing something similar or has found a better way to report server-side validation failures on the client.