using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TomPIT.Services;

namespace TomPIT.Ide.CodeAnalysis
{
	public class SnippetArgs : EventArgs
	{
		public SnippetArgs (IExecutionContext context, SemanticModel model, TextSpan span, int position)
		{
			Context = context;
			Model = model;
			Span = span;
			Position = position;
		}
		public IExecutionContext Context { get; }
		public SemanticModel Model { get; }
		public TextSpan Span { get; }
		public int Position { get; }
	}
}
