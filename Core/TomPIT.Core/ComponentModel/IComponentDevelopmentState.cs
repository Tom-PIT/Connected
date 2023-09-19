using System;

namespace TomPIT.ComponentModel
{
	public enum IndexState
	{
		Synchronized = 0,
		Invalidated = 1
	}

	public enum AnalyzerState
	{
		Analyzed = 0,
		Pending = 1,
		Analyzing = 2
	}

	public interface IComponentDevelopmentState : IComponent
	{
		int Id { get; }
		IndexState IndexState { get; }
		AnalyzerState AnalyzerState { get; }
		DateTime AnalyzerTimestamp { get; }
		DateTime IndexTimestamp { get; }
		Guid Element { get; }
	}
}
