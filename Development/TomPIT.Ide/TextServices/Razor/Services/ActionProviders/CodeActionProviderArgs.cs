﻿using System;
using Microsoft.CodeAnalysis;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.Razor.Services.ActionProviders
{
	internal class CodeActionProviderArgs : EventArgs
	{
		public CodeActionProviderArgs(ITextEditor editor, ICodeActionContext context, SemanticModel model, SyntaxNode node)
		{
			Editor = editor;
			Context = context;
			Model = model;
			Node = node;
		}

		public ITextEditor Editor { get; }
		public ICodeActionContext Context { get; }
		public SemanticModel Model { get; }
		public SyntaxNode Node { get; }
	}
}
