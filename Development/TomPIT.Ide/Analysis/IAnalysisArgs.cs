using System;
using TomPIT.ComponentModel;

namespace TomPIT.Ide.Analysis
{
	public interface IAnalysisArgs
	{
		Type ArgumentsType { get; }
		string Text { get; }
		IComponent Component { get; }
		IText Configuration { get; }
	}
}
