using System;
using TomPIT.ComponentModel;

namespace TomPIT.Design.Services
{
	public class CodeStateArgs : EventArgs, IAnalysisArgs
	{
		public CodeStateArgs(IComponent component, Type argumentsType, string text, int position, string triggerCharacter, string triggerKind)
		{
			Text = text;
			Position = position;
			TriggerCharacter = triggerCharacter;
			TriggerKind = triggerKind;
			ArgumentsType = argumentsType;
			Component = component;
		}

		public IComponent Component { get; }
		public int Position { get; }
		public string Text { get; }
		public string TriggerCharacter { get; }
		public string TriggerKind { get; }
		public Type ArgumentsType { get; }
	}
}
