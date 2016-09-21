Title: Supercharge Your Enums
Lead: It's easy with this one little trick!
Published: 9/20/2016
Tags:
  - enum
---
I was recently talking with someone on Twitter about looking back at my old Java code. I don't miss writing Java (there's a good reason I decided to specialize in the .NET stack a decade ago), but there are a couple aspects of the language that I do remember fondly. The way enum types in Java can be more than just atomic values has always struck me as kind of cool. I've found use cases over the years where I wish C# has something similar. I've solved this need in a variety of ways, and I'll show you one of my favorites here for it's simplicity.

The idea is that you may want your enum type to contain some additional logic. In other words, you want a predefined set of "things" and to be able to refer to them by name, but you also want those things to be objects. To make this work, I use the following base class. The base class is responsible for reflecting on all the `public static` fields in it's derived class(es) and providing all of the enum values in the `Values` property. This uses a little trick in the generic type constraints where you derive from a class that has a generic type of the derived class.

```
public abstract class ClassEnum<T> where T : ClassEnum<T>
{
    static ClassEnum()
    {
        Values = typeof(T)
            .GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(x => x.FieldType == typeof(T))
            .ToDictionary(x => x.Name, x => (T) x.GetValue(null));
    }

    public static IReadOnlyDictionary<string, T> Values { get; }
}
```

Then you can use any class as your enum type. Note the private constructor below which ensures the only instances are the static fields exposed by the class.

```
public class Color : ClassEnum<Color>
{
    // Enum Values
    public static readonly Color Red = new Color("Red", 0);
    public static readonly Color Blue = new Color("Blue", 200);

    // Instance
    public string Name { get; }
    public int Hue { get; }

    private Color(string name, int hue)
    {
        Name = name;
        Hue = hue;
    }
}
```

Now you can use your new enum-like types like this:

```
Color color = Color.Red;
int[] colorHues = Color.Values
    .Select(x => x.Value.Hue)
    .ToArray();
```