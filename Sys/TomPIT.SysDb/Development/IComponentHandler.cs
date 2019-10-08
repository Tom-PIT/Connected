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
		void Insert(IMicroService microService, DateTime modified, IFolder folder, string category, string nameSpace, string name, Guid token, string type, Guid runtimeConfiguration);
		void Update(IComponent component, DateTime modified, string name, IFolder folder, Guid runtimeConfiguration);
		void Update(IComponent component, IUser user, LockStatus status, LockVerb verb, DateTime date);
		void UpdateStates(List<IComponentAnalyzerState> states);
		void UpdateStates(List<IComponentIndexState> states);
		void Delete(IComponent component);

		IComponent Select(Guid component);
		IComponent Select(IMicroService microService, string category, string name);
		List<IComponent> Query(string category, string name);

		List<IComponentDevelopmentState> QueryStates();
		List<IComponentDevelopmentState> QueryActiveAnalyzerStates(DateTime timeStamp);
		List<IComponentDevelopmentState> QueryActiveIndexStates(DateTime timeStamp);
		List<IComponentDevelopmentState> QueryStates(IMicroService microService);
		List<IComponentDevelopmentState> QueryStates(IComponent component);
		List<IComponentDevelopmentState> QueryStates(IComponent component, Guid element);
	}
}
