using System;
using TomPIT.ComponentModel;

namespace TomPIT.Design.Services
{
	public interface IAnalysisArgs
	{
		Type ArgumentsType { get; }
		string Text { get; }
		IComponent Component { get; }
	}
}
