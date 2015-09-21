Title: Streaming LINQ-Based Text Search and Replace
Published: 8/4/2015
Tags:
  - LINQ
---
I was recently thinking about streaming operations and got to wondering about string operations on streaming data. Specifically, how would you perform a text search and replace on a string if you could only stream the string one character at a time? This could have real-world application in asynchronous systems where you get a string a character at a time, or in memory-intensive scenarios where you can't load the source string into memory at once. For the most part though, this was just a thought exercise. I structured the code as a LINQ-like extension method, though obviously the same approach could be used with streams.

Here's what I came up with:

```
public static IEnumerable<char> Replace(this IEnumerable<char> source, string search, string replace)
{
	// Iterate the source characters
	int match = 0;
	foreach(char c in source)
	{
		if(match == search.Length)
		{
			// Found a match, replace it with replace
			foreach(char r in replace)
			{
				yield return r;
			}
			match = 0;
		}
		
		if(c == search[match])
		{
			// Potentially a match
			match++;
		}
		else
		{
			// Not a match, output candidate match up to this point
			if(match > 0)
			{
				for(int m = 0 ; m < match ; m++)
				{
					yield return search[m];
				}
				match = 0;
			}
			
			// And output non-matched char
			yield return c;
		}
	}
	
	// Output any matches or partial matches at the end
	if(match == search.Length)
	{
		foreach(char r in replace)
		{
			yield return r;
		}
	}
	else if(match > 0)
	{
		for(int m = 0 ; m < match ; m++)
		{
			yield return search[m];
		}			
	}
}
```

Usage:

```
string source = @@"Lorem ipsum dolor sit amet, consectetur adipiscing
	elit, sed do eiusmod tempor incididunt ut labore et dolore magna 
	aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco 
	laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure 
	dolor in reprehenderit in voluptate velit esse cillum dolore eu 
	fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, 
	sunt in culpa qui officia deserunt mollit anim id est laborum.";
string search = @@"dolor";
string replace = @@"foobar";
string replaced = new string(source.ToCharArray().Replace(search, replace).ToArray());
```

Performance isn't terrible, but it's not too great either. In very rough benchmarking it's about an order of magnitude (and a little more) slower than a standard `string.Replace(...)`. Of course, there are much faster string search algorithms that take advantage of being able to seek. You could also speed this up by buffering and then doing a `string.Replace(...)` on each chunk (though you'd have to account for matches at the break points of the chunks). You could also easly genericize the algorithm to search and replace any sort of object in an `IEnumerable<T>`, not just `char`.

Please let me know in the comments if you have any other optimization ideas.