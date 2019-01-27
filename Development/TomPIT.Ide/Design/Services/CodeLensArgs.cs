using System;
using TomPIT.ComponentModel;

namespace TomPIT.Design.Services
{
	public class CodeLensArgs : EventArgs, IAnalysisArgs
	{
		public CodeLensArgs(IComponent component, IText configuration, Type argumentsType, string text)
		{
			Text = text;
			ArgumentsType = argumentsType;
			Component = component;
			Configuration = configuration;
		}

		public IComponent Component { get; }
		public string Text { get; }
		public Type ArgumentsType { get; }
		public IText Configuration { get; }
	}
}
