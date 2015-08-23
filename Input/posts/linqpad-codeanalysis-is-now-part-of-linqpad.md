Title: LINQPad.CodeAnalysis Is Now Part Of LINQPad
Published: 8/23/2015
Tags:
  - LINQPad
  - open source
  - Roslyn
  - .NET Compiler Platform
---
Just a quick note that most of the functionality in [LINQPad.CodeAnalysis](https://github.com/daveaglick/LINQPad.CodeAnalysis) can now be found in [LINQPad 5 (as of 5.02 beta)](https://www.linqpad.net/Download.aspx#beta5). When using LINQPad 5, the syntax tree and syntax visualizer are both found under the "Tree" tab after executing a query. This tab is available for any query and you can also dump a `SyntaxTree` or `SyntaxNode` explicitly by calling `.DumpSyntaxTree()` or `.DumpSyntaxNode()`. The integration in LINQPad 5 is also tighter than a plugin allows and lets you highlight the original query as you highlight nodes in the syntax tree as well as other UI improvements. I'd like to thank Joseph Albahari for making this integration possible and for tweaking things to provide the best possible experience:

<img src="/Content/posts/linqpad5.png" alt="LINQPad 5" class="img-responsive" style="margin-top: 6px; margin-bottom: 6px;">

Unless you're using an older version of LINQPad (such as one of the version 4 betas 4.56.04 or higher) or want access to semantic information from the syntax tree (which isn't included in the new integration), I recommend relying on the integrated native to LINQPad.