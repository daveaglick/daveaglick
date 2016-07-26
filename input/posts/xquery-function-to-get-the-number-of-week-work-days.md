Title: XQuery Function To Get The Number Of Week/Work Days
Published: 6/17/2010
Tags:
  - XML
  - XQuery
  - networkdays
  - weekdays
---

<p>Today's post is an XQuery function designed to get a count of the number of week (or work) days between two dates. It's designed to mimic the Excel <code>NETWORKDAYS</code> function. I got the algorithm from Bernal Schooley in this <a href="http://www.eggheadcafe.com/community/aspnet/2/44982/how-to-calculate-num-of-w.aspx">thread</a> and then adapted it to XQuery. It also makes use of the <a href="http://www.xqueryfunctions.com/">FunctX</a> <code><a href="http://www.xqueryfunctions.com/xq/functx_day-of-week.html">day-of-week</a></code> function, so if you have FunctX functions already referenced you can take that part out.</p>

<pre class="prettyprint">declare namespace functx = "http://www.functx.com";

declare function functx:day-of-week
 ($date as xs:anyAtomicType?) as xs:integer? {
 if (empty($date))
 then ()
 else
  xs:integer((xs:date($date) - xs:date('1901-01-06')) div xs:dayTimeDuration('P1D')) mod 7
};

declare function local:weekdays
 ($start as xs:anyAtomicType?, $end as xs:anyAtomicType?) as xs:integer? {
 if(empty($start) or empty($end))
 then()
 else
  if($start > $end)
  then -local:weekdays($end, $start)
  else
   let $dayOfWeekStart := functx:day-of-week($start)
   let $dayOfWeekEnd := functx:day-of-week($end)
   let $adjDayOfWeekStart := if($dayOfWeekStart = 0) then 7 else $dayOfWeekStart
   let $adjDayOfWeekEnd := if($dayOfWeekEnd = 0) then 7 else $dayOfWeekEnd
   return
    if($adjDayOfWeekStart <= $adjDayOfWeekEnd)
    then xs:integer((xs:integer(days-from-duration(xs:date($end) - xs:date($start)) div 7) * 5)
     + max(((min((($adjDayOfWeekEnd + 1), 6)) - $adjDayOfWeekStart), 0)))
    else xs:integer((xs:integer(days-from-duration(xs:date($end) - xs:date($start)) div 7) * 5)
     + min((($adjDayOfWeekEnd + 6) - min(($adjDayOfWeekStart, 6)), 5)))
};</pre>
<p>Usage: <code>local:weekdays('2009-06-01', '2010-06-30')</code></p>
