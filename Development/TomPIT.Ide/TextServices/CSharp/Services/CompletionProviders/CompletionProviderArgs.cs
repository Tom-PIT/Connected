using System;
using Microsoft.CodeAnalysis;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders
{
	public class CompletionProviderArgs : EventArgs
	{
		public CompletionProviderArgs(ITextEditor editor, ICompletionContext context, SemanticModel model, IPosition position)
		{
			Editor = editor;
			Context = context;
			Model = model;
			Position = position;
		}

		public ITextEditor Editor { get; }
		public ICompletionContext Context { get; }
		public SemanticModel Model { get; }
		public IPosition Position { get; }
	}
}
