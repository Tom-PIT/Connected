using System;

namespace TomPIT.Services.Context
{
	public interface IContextDiagnosticService
	{
		void Error(string category, string source, string message);
		void Warning(string category, string source, string message);
		void Info(string category, string source, string message);

		void Console(string message);

		Guid EnterMetric(Guid component);
		Guid EnterMetric(Guid component, Guid element);
		void ExitMetric();
		void ExitMetric(Guid id);
	}
}
