using System;
using Microsoft.CodeAnalysis;

namespace TomPIT.Ide.TextServices.CSharp.Services.DecorationsProvider
{
	internal class DeltaDecorationProviderArgs : EventArgs
	{
		public DeltaDecorationProviderArgs(ITextEditor editor, SemanticModel model)
		{
			Editor = editor;
			Model = model;
		}

		public ITextEditor Editor { get; }
		public SemanticModel Model { get; }
	}
}
