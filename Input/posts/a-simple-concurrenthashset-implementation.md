Title: A Simple ConcurrentHashSet Implementation
Published: 3/7/2016
Tags:
  - collections
  - concurrent
  - HashSet
---
I really like the classes in the `System.Collections.Concurrent` namespace. They have clean APIs, they implement the appropriate interfaces, and they make working with collections from multiple threads much, much easier. That said, there's one collection I find myself reaching for repeatedly that isn't available in the set of concurrent collections: a hash set. I have no idea why this was omitted, but I decided to go ahead and create a simple implementation myself.

First off, I decided not to create a class for this from scratch. I've already opened [an issue on the CoreFx GitHub repository](https://github.com/dotnet/corefx/issues/3563) requesting this be added, and I've got it in the back of my mind that I may take a stab if I find myself with enough free time. The underlying code for `ConcurrentDictionary<TKey, TValue>` is probably the closest example, and it's pretty complicated.

In fact, for a simpler implementation I just decided to wrap `ConcurrentDictionary<TKey, TValue>` and basically ignore the values. You could alternatively wrap `HashSet<T>` with locking code, but I figured the existing concurrent class probably has better optimizations for locking than a simple global lock around a delegated `HashSet<T>`.

Here's what I came up with:

```
public class ConcurrentHashSet<T> : ICollection<T>, IReadOnlyCollection<T>
{
    // This class wraps our keys and serves only to provide support for the special case
    // where the item is null which is supported in a HashSet<T> but not as a key in a dictionary
    private class Item
    {
        public Item(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }

    // We also have to wrap the comparer since the generic types of the 
    // item and underlying dictionary are different
    private class ItemComparer : IEqualityComparer<Item>
    {
        private readonly IEqualityComparer<T> _comparer;

        public ItemComparer(IEqualityComparer<T> comparer)
        {
            _comparer = comparer;
        }

        public bool Equals(Item x, Item y)
        {
            return _comparer.Equals(x.Value, y.Value);
        }

        public int GetHashCode(Item obj)
        {
            return _comparer.GetHashCode(obj.Value);
        }
    }

    private readonly ConcurrentDictionary<Item, byte> _dictionary;

    public ConcurrentHashSet()
    {
        _dictionary = new ConcurrentDictionary<Item, byte>(new ItemComparer(EqualityComparer<T>.Default));
    }

    public ConcurrentHashSet(IEnumerable<T> collection)
    {
        _dictionary = new ConcurrentDictionary<Item, byte>(
            collection.Select(x => new KeyValuePair<Item, byte>(new Item(x), Byte.MinValue)),
            new ItemComparer(EqualityComparer<T>.Default));
    }

    public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
    {
        _dictionary = new ConcurrentDictionary<Item, byte>(
            collection.Select(x => new KeyValuePair<Item, byte>(new Item(x), Byte.MinValue)),
            new ItemComparer(comparer));
    }

    public ConcurrentHashSet(IEqualityComparer<T> comparer)
    {
        _dictionary = new ConcurrentDictionary<Item, byte>(new ItemComparer(comparer));
    }

    public bool Add(T item) => _dictionary.TryAdd(new Item(item), Byte.MinValue);

    // IEnumerable, IEnumerable<T>

    public IEnumerator<T> GetEnumerator() => _dictionary.Keys.Select(x => x.Value).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // IReadOnlyCollection<T>

    public int Count => _dictionary.Count;

    // ICollection<T>

    void ICollection<T>.Add(T item) => ((IDictionary<Item, byte>) _dictionary).Add(new Item(item), Byte.MinValue);

    public void Clear() => _dictionary.Clear();

    public bool Contains(T item) => _dictionary.ContainsKey(new Item(item));

    public void CopyTo(T[] array, int arrayIndex) => 
        _dictionary.Keys.Select(x => x.Value).ToArray().CopyTo(array, arrayIndex);

    public bool Remove(T item)
    {
        byte value;
        return _dictionary.TryRemove(new Item(item), out value);
    }

    public bool IsReadOnly => false;
}
```