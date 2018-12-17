Title: Announcing AzurePipelines.TestLogger
Lead: An Azure Pipelines logger extension for the Visual Studio Test Platform
Published: 12/17/2018
Image: /images/test-tubes.jpg
Tags:
  - open source
  - Azure
  - devops
---
In today's episode of "what crazy niche has Dave gotten sucked into this time?" I announce a new test logger for the Visual Studio Test Platform designed to publish your test results in real-time to Azure Pipelines. This means that you can run `dotnet test` from your build script on Azure Pipelines and feed your test results directly to the test summary for your build without having to rely on post-processing like the `PublishTestResults` Azure Pipelines task.

Before we get to publishing results to Azure Pipelines, let's back up a step and briefly consider what a Visual Studio Test Platform test logger actually is. [According to the official docs](https://github.com/Microsoft/vstest-docs/blob/master/docs/report.md), "A test logger is a test platform extension to control reporting of test results.". That's not particularly helpful. What it really means is that you can write a library to hook into what's happening with your test run and do something with that information. The API for this isn't great or well documented, basically a single interface with a few event handlers, but it's enough to get details about each test run.

The AzurePipelines.TestLogger then registers handlers for these test events, builds a heirarchy from the test and source (I.e., file) names, and publishes that to Azure Pipelines while your tests are running using the [Azure DevOps REST API](https://docs.microsoft.com/en-us/rest/api/azure/devops/?view=azure-devops-rest-5.0). There were some tricky parts such as figuring out which version of the API to specify for which endpoint (each endpoint is versioned a little differently, particularly with preview versions). Getting some of the Azure Pipelines-specific data like nested test results and parent test durations to work was also a challenge. Now that I've worked through everything, I rather like the result:

<img src="/posts/images/test-summary.png" class="img-responsive"></img>

Each test "run" is shown at the root of the result tree (a run is the combination of test assembly and build job/agent). Then each test fixture or class is shown at the second level with it's fully qualified name (minus the root namespace). Nested classes are shown with `+` notation. Then individual tests are displayed at the third level. This three-deep heirarchy keeps very large test runs nice and tidy. On the downside, the Azure Pipelines test summary will only show statistics for top-level tests. That's not ideal for a logger that nests results like this one, but the clarity of grouping tests under their fixture is more valuable than listing a correct total in the test summary in my opinion. Thankfully the pass/fail will still "bubble up" so even though the summary may show fewer tests than actually exist, it'll still correctly indicate if any tests are failing (which would then require a drill-down to figure out which ones are failing). [There's an open feature suggestion here for showing all nested tests in the summary](https://developercommunity.visualstudio.com/content/idea/409015/show-all-tests-in-the-hierarchy-in-test-summary.html).

If you're using .NET and Azure Pipelines and you need this in your life, [head on over to the GitHub repository](https://github.com/daveaglick/AzurePipelines.TestLogger) for installation and usage instructions. Happy testing.