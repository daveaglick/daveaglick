Title: The Bleeding Edge Of Razor
Lead: Using the Razor view engine in your own code.
Published: 10/4/2018
Image: /images/blocks.jpg
Tags:
  - ASP.NET
  - ASP.NET MVC
  - Razor
---
Over the years there've been a number of projects designed to make using Razor templates from your own code easier. For a while, these third-party libraries were the only way to easily use Razor outside ASP.NET MVC because using the ASP.NET code directly was too complicated. That started to change with ASP.NET Core and the ASP.NET team has slowly started to address this use case. In this post we'll take a look at the current bleeding edge of Razor and how you can use it today to enable template rendering in your own application.

Before we start looking at code, let's back up a step and consider what Razor is (and what it isn't). At it's core, Razor is a templating language. Templating languages are designed to make producing output content easier by intermixing raw output with instructions on how to generate additional programmatically-based output. In this case, Razor is used to produce HTML documents. An important distinction that I want to make here is that Razor _is not_ the set of HTML helpers and other support functionality that comes along with ASP.NET MVC. For example, helpers like `Html.Partial()` and page directives like `@section` aren't part of the Razor language. Instead they're shipped with ASP.NET MVC as additional support on top of Razor, which your Razor code can use.

This distinction wasn't always clear, but recently the ASP.NET team has been focusing on separating Razor _the language_ from Razor _for ASP.NET MVC_. This is partly out of necessity as Razor has grown to support at least three different dialects (ASP.NET MVC, Razor Pages, and Blazor), but it also makes using Razor for your own purposes easier too.

# Rendering Phases

Turning Razor content from a string, file, or other source into final rendered HTML goes through several phases.