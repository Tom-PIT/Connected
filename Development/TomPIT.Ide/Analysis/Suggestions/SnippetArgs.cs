using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TomPIT.Middleware;

namespace TomPIT.Ide.Analysis.Suggestions
{
	public class SnippetArgs : EventArgs
	{
		public SnippetArgs(IMiddlewareContext context, SemanticModel model, TextSpan span, int position)
		{
			Context = context;
			Model = model;
			Span = span;
			Position = position;
		}
		public IMiddlewareContext Context { get; }
		public SemanticModel Model { get; }
		public TextSpan Span { get; }
		public int Position { get; }
	}
}
