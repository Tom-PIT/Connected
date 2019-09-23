using System;
using Microsoft.CodeAnalysis;
using TomPIT.Ide.TextEditor.Languages;

namespace TomPIT.Ide.TextEditor.CSharp.Services.CompletionProviders
{
	internal class CompletionProviderArgs : EventArgs
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
