using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Development;
using TomPIT.Security;

namespace TomPIT.SysDb.Development
{
	public interface IComponentHandler
	{
		List<IComponent> Query();
		void Insert(IMicroService microService, DateTime modified, IFolder folder, string category, string name, Guid token, string type, Guid runtimeConfiguration);
		void Update(IComponent component, DateTime modified, string name, IFolder folder, Guid runtimeConfiguration);
		void Update(IComponent component, IUser user, LockStatus status, LockVerb verb, DateTime date);
		void UpdateState(IComponent component, Guid element, IndexState indexState, DateTime indexTimestamp, AnalyzerState analyzerState, DateTime analyzerTimestamp);
		void Delete(IComponent component);

		IComponent Select(Guid component);
		IComponent Select(IMicroService microService, string category, string name);
		List<IComponent> Query(string category, string name);

		List<IComponentDevelopmentState> QueryStates();
		List<IComponentDevelopmentState> QueryStates(IMicroService microService);
		List<IComponentDevelopmentState> QueryStates(IComponent component);
		List<IComponentDevelopmentState> QueryStates(IComponent component, Guid element);
	}
}
