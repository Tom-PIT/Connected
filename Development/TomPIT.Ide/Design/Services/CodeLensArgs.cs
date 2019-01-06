using System;
using TomPIT.ComponentModel;

namespace TomPIT.Design.Services
{
	public class CodeLensArgs : EventArgs, IAnalysisArgs
	{
		public CodeLensArgs(IComponent component, Type argumentsType, string text)
		{
			Text = text;
			ArgumentsType = argumentsType;
			Component = component;
		}

		public IComponent Component { get; }
		public string Text { get; }
		public Type ArgumentsType { get; }
	}
}
