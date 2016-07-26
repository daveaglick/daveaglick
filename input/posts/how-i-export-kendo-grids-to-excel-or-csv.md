Title: How I Export Kendo Grids to Excel (or CSV)
Published: 3/6/2014
Tags:
  - ASP.NET
  - ASP.NET MVC
  - KendoUI
  - KendoUI MVC
  - grid
  - export
  - CSV
  - Excel
---

<p>Exporting grid data is a common need for line-of-business applications. Once you’ve got all the great data presented, filtered, etc. how to you allow the user to download it and continue playing with it? There are many approaches discussed online to solve this (see <a href="http://blogs.telerik.com/kendoui/posts/13-03-12/exporting_the_kendo_ui_grid_data_to_excel">here</a> and <a href="http://stackoverflow.com/questions/14472802/by-using-kendo-how-to-export-the-grid-data-to-any-one-of-the-following-files-cs">here</a>). Unfortunately, I didn’t find any of them really had the polish I wanted and that my users demand. There are really two categories of solutions to this problem:</p>

<h1>Server-Side</h1>

<p>This approach relies on intercepting or otherwise taking the output of the method that generates the data for the grid and formatting it for download by the user. There are a couple of problems with this technique. The first is that the data you return to the grid may not be suitable for the user. It may have aggregate data, piecemeal data, etc. You also loose any heading information you’ve added to the grid. Sure, you could use something like .NET reflection to get the name of properties, but that’s not the same thing as headers. Especially since the Kendo grid (when used through the Kendo MVC library) automatically splits and/or uses data annotations for the header text. You also loose any filtering, sorting, etc. unless you also capture the state of the grid options which can be tricky to get right. In general, I think it comes down to the server-side data that the grid uses isn’t the same thing as what’s presented to the user. And if you’re going to return a CSV or Excel file that doesn’t match what they’re looking at on the screen then you’re going to cause confusion.</p>

<h1>Client-Side</h1>

<p>The other kind of approach is to get the data on the client. I like this approach better because it has the potential to exactly match what the user sees on their screen. I found the following <a href="https://github.com/uber-rob/kendo-grid-csv-download">source on GitHub</a>, which itself is based on the work in this <a href="http://www.telerik.com/forums/export-to-csv">forum thread</a>. The goal of this code is to trigger downloading and formatting of the data by the grid itself after the user has manipulated it and then package that data into a file for download. In other words, use the grid as a proxy so that the data you use for the file matches exactly what the user sees. While I think the approach is sound, I found many problems with the code itself. For example, it didn’t wait for the grid data source to complete fetching data, thus often returning stale data to the user. It also lacked support for things like stripping out HTML (which I use in grids a lot for embedded links). My modified version looks like this:</p>

<pre class="prettyprint">// Modified from https://github.com/uber-rob/kendo-grid-csv-download
function kendoGridToCSV(grid) {

    // Get access to basic grid data
    var datasource = grid.dataSource;
    var originalPageSize = datasource.pageSize();
    var originalPage = datasource.page();

    // Remove the success function since we don't care about what happens after we get the data
    // (and the prototype success function causes problems when we copy the datasource over from the grid)
    datasource.success = function (data) {
        var csv = '';

        // Add the header row
        for (var i = 0; i &lt; grid.columns.length; i++) {
            var title = grid.columns[i].title,
                field = grid.columns[i].field;
            if (typeof (field) === "undefined") { continue; /* no data! */ }
            if (typeof (title) === "undefined") { title = field }

            title = title.replace(/"/g, '""');
            csv += '"' + title + '"';
            if (i &lt; grid.columns.length - 1) {
                csv += ",";
            }
        }
        csv += "\n";

        // Add each row of data
        $.each(data.Data, function (index, row) {
            // Do a first pass to parse any dates (may eventually need to parse other types of received values here)
            for (var i = 0; i &lt; grid.columns.length; i++) {
                var fieldName = grid.columns[i].field;
                if (typeof (fieldName) === "undefined") { continue; }
                if (typeof row[fieldName] == "string" &amp;&amp; row[fieldName].lastIndexOf("/Date(", 0) === 0) {
                    row[fieldName] = kendo.parseDate(row[fieldName]);
                }
            }

            // Now generate the actual values
            for (var i = 0; i &lt; grid.columns.length; i++) {
                var fieldName = grid.columns[i].field;
                if (typeof (fieldName) === "undefined") { continue; }

                // Get the template and use it to get the display value
                var tmpl = grid._cellTmpl(grid.columns[i], {});
                var kt = kendo.template(tmpl);
                value = kt(row);

                // Strip any HTML (needs to be inclosed in an outer tag to work)
                // Also strip any elements with the 'no-export' class
                // Also remove any label elements since they get used often in links
                var html = $('&lt;div&gt;' + value + '&lt;/div&gt;');
                html.find('.label').remove();
                html.find('.no-export').remove();
                value = html.text().trim();

                // Format for CSV (escape quotes and add the comma)
                value = value.replace(/"/g, '""');
                csv += '"' + value + '"';
                if (i &lt; grid.columns.length - 1) {
                    csv += ",";
                }
            }
            csv += "\n";
        });

        // Send the CSV content back to the server to generate a download link
        postToURL("/CsvToExcel", { data: csv });

        // Reset back to original values and reset the datasource
        datasource.pageSize(originalPageSize);
        datasource.page(originalPage);
        delete datasource.success;   

        // Reset the datasource now that we're done
        datasource._dequeueRequest();
        datasource.view();
        kendo.ui.progress(grid.element, false);
    }

    // Increase page size to cover all the data and then trigger fetching and processing of all the data
    datasource.pageSize(datasource.total());
    datasource.view();
}</pre>

<p>The one problem with this code is that it creates the data on the client. “But that’s great!” you say, “The client won’t have to download anything extra!” you say. Unfortunately there’s actually no good cross-browser way to get a blob of data into a “file” that the client presents for download to the user. As suggested by the original authors, there is a library called <a href="https://github.com/dcneiner/Downloadify">Downloadify</a> that can solve this using JavaScript and Flash. If that works for you, great! However, I’m not a fan of Flash and can’t rely on it being available in my user’s environment so I needed a workaround.</p>

<h2>Downloading The File</h2>

<p>The first challenge is getting the data to the user. I found this JavaScript method that can be used to post arbitrary content to a server (we can’t use the jQuery AJAX post method because we need this to be an actual post request so the prompt to save the resulting file is presented):</p>

<pre class="prettyprint">// This posts specific data to a given URL
// From http://stackoverflow.com/questions/133925/javascript-post-request-like-a-form-submit
function postToURL(url, values) {
    var form = $('&lt;form id="postToURL"&gt;&lt;/form&gt;');

    form.attr("method", "post");
    form.attr("action", url);

    $.each(values, function (key, value) {
        var field = $('&lt;input&gt;&lt;/input&gt;');

        field.attr("type", "hidden");
        field.attr("name", key);
        field.attr("value", value);

        form.append(field);
    });

    // The form needs to be a part of the document in
    // order for us to be able to submit it.
    $(document.body).append(form);
    form.submit();
    $("#postToURL").remove();
}</pre>

<p>You’ll notice it’s called from inside the function that converts the grid to CSV data. That solves the client-side part of the equation. Yes, I know what you’re thinking. Isn’t it inefficient to have the client get the data from the server, format it, and then send it right back to the server, only to have it sent BACK to the client as a file? Well, yes, yes it is. In my situation though, the performance hit was acceptable in order to achieve the best user interaction. That’s aided by the fact that the user clicked a button to make this happen. If a user takes an action that they know might be long-running, their a lot more likely to forgive a little wait time.</p>

<p>On the server I have an action called <code>CsvToExcel</code>. However, before I show you that, here’s an alternate action that would have just packaged up the CSV and returned it as a file:</p>

<pre class="prettyprint">[POST("MakeFile")]
[ValidateInput(false)]
public virtual ActionResult MakeFile(string fileName, string contentType, string data)
{
    if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(contentType) || string.IsNullOrWhiteSpace(data))
        return HttpNotFound();
    return File(Encoding.UTF8.GetBytes(data), contentType, fileName);
}</pre>

<p>Now my actual action uses <a href="http://www.aspose.com/.net/excel-component.aspx">Aspose Cells</a> to create an Excel file with headings, frozen panes, etc. but you could make do with other free alternatives mentioned in the linked articles at the beginning of this post. Here is my real action:</p>

<pre class="prettyprint">[POST("CsvToExcel")]
[ValidateInput(false)]
public virtual ActionResult CsvToExcel(string data)
{
    if (string.IsNullOrWhiteSpace(data))
        return HttpNotFound();

    // Create a workbook from the CSV data
    Workbook workbook;
    using(MemoryStream inputStream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
    {
        workbook = new Workbook(inputStream, new LoadOptions(LoadFormat.CSV));
    }
    Worksheet worksheet = workbook.Worksheets[0];

    // Make the heading row bold
    Style boldStyle = workbook.Styles[workbook.Styles.Add()];
    boldStyle.Font.IsBold = true;
    worksheet.Cells.Rows[0].ApplyStyle(boldStyle, new StyleFlag() { FontBold = true });

    // Freeze, autofit, and activate autofilter for the heading row
    worksheet.FreezePanes(1, 0, 1, 0);
    worksheet.AutoFitColumns();
    int letterIndex = worksheet.Cells.MaxDataColumn + 65;
    char letter = letterIndex &gt; 90 ? 'Z' : (char)letterIndex;
    worksheet.AutoFilter.Range = "A1:" + letter + "1";

    // Return the file
    byte[] output;
    using (MemoryStream outputStream = new MemoryStream())
    {
        workbook.Save(outputStream, SaveFormat.Xlsx);
        outputStream.Seek(0, SeekOrigin.Begin);
        output = outputStream.ToArray();
    }
    return File(output, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "grid.xlsx");
}</pre>

<h2>Rigging It Up</h2>

<p>The last part is rigging this all up to an actual grid. I wanted it to be as easy as possible to add this capability to any arbitrary grid. To that end, I create an extension method for the Kendo MVC wrappers that lets me add an “Export” button the exact same way you add a “Create” button to the grid toolbar. Here’s the extension:</p>

<pre class="prettyprint">public static GridToolBarCustomCommandBuilder&lt;TModel&gt; Export&lt;TModel&gt;(this GridToolBarCommandFactory&lt;TModel&gt; factory)
    where TModel : class, new()
{
    return factory.Custom().Text("Export").HtmlAttributes(new { @class = "export-grid" });
}</pre>

<p>And here’s the small bit a JavaScript that supports it (essentially rigging up a jQuery click handler for the new button to the <code>kendoGridToCSV</code> function mentioned earlier:</p>

<pre class="prettyprint">// This rigs up the export button on the grid
$(".export-grid").click(function (e) {
    e.preventDefault();
    var grid = $(e.target).parents('.k-grid').data("kendoGrid");
    kendoGridToCSV(grid);
});</pre>

<p>Finally, here’s how to use it on your grid:</p>

<pre class="prettyprint">@(Html.Kendo().Grid(...).Name("...")
    ...
    .ToolBar(x =&gt; x.Export())
    ...
)</pre>

<p>Easy, right? I hope this was helpful – it took me a while to work through all the bits and hopefully it will save you some time.</p>