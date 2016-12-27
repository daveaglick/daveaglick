Title: WrappingStringBuilder
Lead: A small utility class for wrapping strings at predefined points based on line length.
Published: 10/6/2015
Tags:
  - strings
  - algorithms
---
I've been working on a Roslyn-powered documentation generator built on top of [Wyam](https://wyam.io) for a little while, but recently ran into an area that's taken a lot longer than I thought it would. I've been trying to generate syntax strings that look like the ones on the MSDN site.

<img src="/posts/images/syntax.png" alt="Syntax" class="img-responsive" style="margin-top: 6px; margin-bottom: 6px;">

It turns out this is really hard. Roslyn doesn't quite do this in the box (though it gets close). This means you have to rely on parsing out the different segments of the definition yourself (this part of the process is for a future blog post). Getting the formatting to look good with wrapping and indents once you bring it all together is a challenge.

To addess that last problem, I created a little helper class that can append string segments with an optional indication of whether they should be allowed to wrap to the next line. You can also specify (and change) a string prefix to use for new lines (for indenting text, for example). I haven't seen anything else like this out there, and it took me a while to work through all the edge cases (it's covered by unit tests in my actual project), so I figured I'd share in case it helps anyone.

```
internal class WrappingStringBuilder
{
    private readonly StringBuilder _masterBuilder 
        = new StringBuilder();
    private readonly List<Tuple<string, bool, bool>> _segments 
        = new List<Tuple<string, bool, bool>>();
    private readonly int _maxLineLength;
    public string NewLinePrefix { get; set; }

    public WrappingStringBuilder(int maxLineLength, string newLinePrefix = null)
    {
        if (maxLineLength < 1)
        {
            throw new ArgumentException(nameof(maxLineLength) 
                + " must be greater than 0.", nameof(maxLineLength));
        }
        _maxLineLength = maxLineLength;
        NewLinePrefix = newLinePrefix;
    }

    public int Length => _masterBuilder.Length 
        + (_segments.Count == 1 && _segments[0].Item3 ? 0 : _segments.Sum(x => x.Item1.Length));

    public override string ToString()
    {
        // Exclude the prefix content for the next line if there's nothing after it
        return _masterBuilder + (_segments.Count == 1 && _segments[0].Item3 
            ? string.Empty : string.Join("", _segments.Select(x => x.Item1)));
    }

    public WrappingStringBuilder Append(string value, bool wrapBefore = false)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value == string.Empty)
        {
            return this;
        }

        // Check if we need to wrap
        if (_segments.Count > 0 && _segments.Sum(x => x.Item1.Length) + value.Length > _maxLineLength)
        {
            // If this isn't a breakpoint, we need to wrap at the previous breakpoint
            // (if there is one) otherwise, we can just wrap the entire previous line
            int wrapAt = wrapBefore ? _segments.Count : _segments.FindLastIndex(x => x.Item2);
            if (wrapAt > 0)
            {
                // Found one, wrap it around
                _masterBuilder.AppendLine(
                    string.Join("", _segments.Take(wrapAt).Select(x => x.Item1)));
                _segments.RemoveRange(0, wrapAt);
                if (!string.IsNullOrEmpty(NewLinePrefix))
                {
                    _segments.Insert(0, Tuple.Create(NewLinePrefix, false, true));
                }
            }
        }

        // Append the new segment
        _segments.Add(Tuple.Create(value, wrapBefore, false));

        return this;
    }

    public WrappingStringBuilder AppendLine(string value, bool wrapBefore = false)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        // Append this string (and wrap if needed) then wrap again if anything is left
        Append(value, wrapBefore);
        if (_segments.Count > 0)
        {
            _masterBuilder.AppendLine(
                string.Join("", _segments.Select(x => x.Item1)));
            _segments.Clear();
            if (!string.IsNullOrEmpty(NewLinePrefix))
            {
                _segments.Add(Tuple.Create(NewLinePrefix, false, true));
            }
        }

        return this;
    }

    public WrappingStringBuilder AppendLine()
    {
        return AppendLine(string.Empty);
    }
}
```
