Title: Fun With Fizz Buzz
Published: 10/3/2014
Tags:
  - programming
---

<p>On Twitter last night I noticed someone mention that they were "having fun with #FizzBuzz". I had never heard of Fizz Buzz before, so I decided to <a href="http://en.wikipedia.org/wiki/Fizz_buzz">look it up</a>. In short, Fizz Buzz is a simple programming task that any competent programmer should be able to accomplish. The task is to print numbers from 1 to 100, delimited by a comma and space. For each number divisible by 3 you print "Fizz" instead of the number, for each number divisible by 5 you print out "Buzz", and for each number divisible by both 3 and 5 you print out "FizzBuzz". The apparent simplicity and hidden complexity of this problem also makes it popular in programming interviews. I like a good brain teaser, so down the rabbit hole I went. My only ground rule was no help from the Internet.</p>


<p>My first instinct was to keep it simple:</p>

<pre class="prettyprint">Console.Write("1");
for(int i = 2 ; i < 101 ; i++)
{
	Console.Write(", ");
	if(i % 3 == 0 && i % 5 == 0)
	{
		Console.Write("FizzBuzz");
	}
	else if(i % 3 == 0)
	{
		Console.Write("Fizz");
	}
	else if(i % 5 == 0)
	{
		Console.Write("Buzz");
	}
	else
	{
		Console.Write(i);
	}
}</pre>

<p>It gets the job done, but man that is not elegant at all. And it's long. And it has redundancy in the combined "FizzBuzz". Not a good solution at all. Let's see if I can't simplify this a little bit:</p>

<pre class="prettyprint">Console.Write("1");
for(int i = 2 ; i < 101 ; i++)
{
	string output = ", ";
	if(i % 3 == 0)
	{
		output += "Fizz";
	}
	if(i % 5 == 0)
	{
		output += "Buzz";
	}
	if(output.Length == 2)
	{
		output += i;
	}
	Console.Write(output);
}</pre>

<p>That's a little bit better. It's not much shorter, but it does refactor out the redundancy which would be good maintenance-wise if the text ever had to change. It's also using the string length as a flag to indicate if the number should be output or not. For my next try, I wondered if caching the content would help:</p>

<pre class="prettyprint">string[] output = new string[100];
output = output.Select((o, i) => (i + 1) % 3 == 0 ? "Fizz" : string.Empty).ToArray();
output = output.Select((o, i) => (i + 1) % 5 == 0 ? o + "Buzz" : o).ToArray();
output = output.Select((o, i) => o.Length == 0 ? (i + 1).ToString() : o).ToArray();
Console.WriteLine(string.Join(", ", output));
</pre>

<p>Now that's looking a little more concise! This attempt is basically the same as the one before except the conditions are collapsed into repeated iterations over a cache using a ternary operator (which can make for really unreadable code if you're not careful). I think it's debatable if it actually enhances readability, but at this point I'm going for innovation over maintainability. I do like how using <code>string.Join()</code> eliminates worrying about proper delimiter placement. To that end, I figured I'd try getting the whole thing down to a single statement:</p>

<pre class="prettyprint">Console.Write(string.Join(", ", 
    Enumerable.Range(1, 100).Select(i => 
        i % 15 == 0 ? "FizzBuzz" : 
            (i % 3 == 0 ? "Fizz" : 
                (i % 5 == 0 ? "Buzz" : i.ToString())
            )
    )));</pre>

<p>And there you have it, a single-statement Fizz Buzz thanks to excessive use of the ternary operator! The redundancy has also crept back in, but hey, that's the least of the maintenance problems with this code.</p>

<p>Have I missed any other cool strategies?</p>
