using System;
using System.Collections.Generic;
using TomPIT.Analysis;

namespace TomPIT.Ide.Analysis
{
	[Obsolete]
	public interface IToolsService
	{
		void Register(IToolMiddleware middleware);
		void Run(string name);
		void Activate(string name);
		void Complete(string name);
		ITool Select(string name);
		List<ITool> Query();

		IToolMiddleware GetTool(string name);
	}
}
