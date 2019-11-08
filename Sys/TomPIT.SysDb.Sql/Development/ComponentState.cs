using System;
using TomPIT.ComponentModel;

namespace TomPIT.SysDb.Sql.Development
{
	internal class ComponentState : Component, IComponentDevelopmentState
	{
		public IndexState IndexState { get; set; }

		public AnalyzerState AnalyzerState { get; set; }

		public DateTime AnalyzerTimestamp { get; set; }

		public DateTime IndexTimestamp { get; set; }

		public Guid Element { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			IndexState = GetValue("index_state", IndexState.Synchronized);
			IndexTimestamp = GetDate("index_timestamp");
			AnalyzerState = GetValue("analyzer_state", AnalyzerState.Analyzed);
			AnalyzerTimestamp = GetDate("analyzer_timestamp");
			Element = GetGuid("element");
		}
	}
}
