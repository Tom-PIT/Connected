using System;
using Microsoft.CodeAnalysis;

namespace TomPIT.Ide.TextServices.CSharp.Services.CodeLensProviders
{
	internal class CodeLensProviderArgs : EventArgs
	{
		public CodeLensProviderArgs(ITextEditor editor, SemanticModel model)
		{
			Editor = editor;
			Model = model;
		}

		public ITextEditor Editor { get; }
		public SemanticModel Model { get; }
	}
}
