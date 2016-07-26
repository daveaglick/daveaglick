Title: Round Robin Row Selection From SQL Server
Published: 5/17/2013
Tags:
  - SQL
  - SQL Server
  - round robin
---

<p>I've been trying to answer at least one question a day on Stack Overflow recently, and <a href="http://stackoverflow.com/questions/16595598/how-to-maintain-a-round-robin-approach">one came up yesterday</a> that I thought was a pretty good little SQL problem: how can you efficiently select one row from a database in a "round robin" fashion? That is, how can you make sure the selections are evenly distributed? Turns out this can be accomplished with a single SQL query on SQL 2005 and newer using the <code><a href="http://msdn.microsoft.com/en-us/library/ms177564(v=sql.90).aspx">OUTPUT</a></code> clause. Assuming the table has an "Id" primary key and a "LastSelected" DateTime column, the following SQL query will select the record that hasn't been selected in the longest time (or pick an arbitrary one if there is a tie), update the last time that record was selected, and then return all columns for the record.</p>

<pre class="prettyprint">UPDATE MyTable
SET LastSelected = GetDate()
OUTPUT INSERTED.*
WHERE Id = (SELECT TOP (1) Id FROM MyTable ORDER BY LastSelected)</pre>
