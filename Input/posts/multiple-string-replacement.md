Title: Multiple String Replacement
Lead: Strings are hard.
Published: 8/24/2015
Tags:
  - strings
  - algorithms
---
I recently had a need to make multiple string replacements in a target string at the same time. This turned out to be much more difficult that I anticipated, so I'm going to share my results in the hopes of saving someone else from going down this rabbit hole.

Let's assume that the string I need to make replacements in is:

```
the toboggan rushed to the uneaten foobaz ham and cheese
```

Let's also assume that these are my replacements:

<table class="table">
  <tr><th>Search For</th><th>Replace With</th></tr>
  <tr><td>toboggan</td><td>toward</td></tr>
  <tr><td>to</td><td>toil</td></tr>
  <tr><td>eat</td><td>froz</td></tr>
  <tr><td>uneated</td><td>bar</td></tr>
  <tr><td>foo</td><td>afoob</td></tr>
  <tr><td>foobar</td><td>afoobarb</td></tr>
  <tr><td>cheese</td><td>beer</td></tr>
</table>

My final string should be:

```
the toward rushed toil the unfrozen afoobbaz ham and beer
```

It's all nonsense, but helps to illustrate several pitfalls of multiple string replacement.

## Attempt 1: Multiple Replace Calls

A first attempt was made to sort the search strings in descending order and then perform multiple `string.Replace(...)` calls:

```
public static string Replace(string s, IDictionary<string, string> map)
{
	foreach(KeyValuePair<string, string> item in map.OrderByDescending(x => x.Key.Length))
	{
		s = s.Replace(item.Key, item.Value);
	}
	return s;
}
```

This produced the output:

```
the toilward rushed toil the unfrozen afoobbaz ham and beer
```

You can see that this isn't quite right. The main problem is that if any mapping key is a substring of a longer previously mapped value, the replaced value will get replaced again by the shorter match.

## Attempt 2: Substitution Tokens

So if the problem in my first attempt was that I was replacing the replacement values on subsiquent `string.Replace(...)` calls, then one solution would be to iterate twice. The first time through, replace the search strings with unique tokens. Then the second time through, replace the tokens with the actual replacement value.

```
public static string Replace(string s, IDictionary<string, string> map)
{
	// It doesn't matter what this is as long as it's unique
	string token = "aca13231-307a-4fd0-b859-d603c6d45360";
	
	// Do the first set of replacements
	Dictionary<string, string> tokens = new Dictionary<string, string>();
	int c = 0;
	foreach(KeyValuePair<string, string> item in map)
	{
		string tokensKey = token + "-" + c;
		s = s.Replace(item.Key, tokensKey);
		tokens[tokensKey] = item.Value;
		c++;
	}
	
	// Now replace the tokens
	foreach(KeyValuePair<string, string> item in tokens)
	{
		s = s.Replace(item.Key, item.Value);
	}	
	
	return s;
}
```

This works and returns the correct result, but yech. We can do better.

## Attempt 3: Replacement Search Tree

I hopped on Twitter and asked if anyone had seen or worked on this problem before and got a really helpful response from [D Nemec](https://twitter.com/djnemec). He provided a link to a gist he wrote that contained an implementation of a tree that could be used to search for replacement strings by descending one character at a time. It worked great for simple sets of replacements, but didn't account for cases where you start going down one branch of the tree only to discover that it doesn't match when a shorter branch would have. It also had some problems at the end of the search string. I hacked on it a little bit and came up with this working implementation:

```
public static string Replace(string s, IDictionary<string, string> map)
{
    Trie<char> lookup = new Trie<char>(map.Keys);
    StringBuilder builder = new StringBuilder();
    int lastIdx = -1;
    Trie<char>.Node lastNode = lookup.Root;
    int matchIdx = -1;
    HashSet<Trie<char>.Node> badMatches = new HashSet<Trie<char>.Node>();
    for (int i = 0; i < s.Length + 1; i++)
    {
        if (i < s.Length)
        {
            char chr = s[i];
            if (lastNode.HasNext(chr))
            {
                // Partial match
                Trie<char>.Node matchNode = lastNode.Children[chr];
                if (!badMatches.Contains(matchNode))
                {
                    lastNode = matchNode;
                    if (matchIdx == -1)
                    {
                        matchIdx = i;
                    }
                    continue;
                }
            }
        }

        if (lastNode.IsRoot)
        {
            // Complete match
            string key = new string(lastNode.Cumulative.ToArray());
            builder.Append(map[key]);
            lastIdx = i - 1;
        }
        else
        {
            // No match
            if (matchIdx != -1)
            {
                // Backtrack to the last match start and don't consider this match
                i = matchIdx - 1;
                matchIdx = -1;
                badMatches.Add(lastNode);
                lastNode = lookup.Root;
                continue;
            }
            builder.Append(i < s.Length ? s.Substring(lastIdx + 1, i - lastIdx) : s.Substring(lastIdx + 1));
            lastIdx = i;
        }
        badMatches.Clear();
        matchIdx = -1;
        lastNode = lookup.Root;
    }
    return builder.ToString();
}

private class Trie<T> where T : IComparable<T>
{
    public Node Root { get; }

    public Trie(IEnumerable<IEnumerable<T>> elems)
    {
        Root = new Node(new T[0]);
        foreach (var elem in elems)
        {
            LoadSingle(elem);
        }
    }

    private void LoadSingle(IEnumerable<T> word)
    {
        Node lastNode = Root;
        foreach (var chr in word)
        {
            Node node;
            if (!lastNode.Children.TryGetValue(chr, out node))
            {
                node = new Node(lastNode.Cumulative.Concat(new[] { chr }));
                lastNode.Children[chr] = node;
            }
            lastNode = node;
        }
        lastNode.IsRoot = true;
    }

    public class Node
    {
        public IEnumerable<T> Cumulative { get; }
        public bool IsRoot { get; set; }
        public IDictionary<T, Node> Children { get; }

        public bool HasNext(T elem)
        {
            return Children.Keys.Any(k => k.Equals(elem));
        }

        public Node(IEnumerable<T> cumulative)
        {
            Cumulative = cumulative;
            Children = new Dictionary<T, Node>();
        }
    }
}
```

This works as intended. I also discovered some other edge cases while working on it:

```
the toboggan rushed to the uneaten foobaz ham and cheesey
```

Should become:

```
the toward rushed toil the unfrozen afoobbaz ham and beery
```

And:

```
the toboggan rushed to the uneaten fooba
```

Should become:

```
the toward rushed toil the unfrozen afoobba
```

## Performance

One final note is about the performance of the two different working algorithms. One might think that the tree-based approach is faster given that it doesn't do multiple iteration and relies on a `StringBuilder` to build the result (instead of multiple string assignments). However, you should never assume a specific performance profile and instead rely on benchmarking. When evaulating against the short samples given above, the token-based replacement algotihm is actually about 3 to 4 times faster than the tree-based one. My guess is that this is due to compiler optimizations on string assignments and the fact that the search string was very short. Figuring out exactly what's going on is a blog post for another day, but it's a lesson that you should always benchmark performance-critical code.

So, have I missed any multiple replacement string algorithms? Can you think of any improvements to the ones I've presented?