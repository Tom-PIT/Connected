using System;
using TomPIT.ComponentModel;

namespace TomPIT.Design
{
	public interface IMicroServiceDesign
	{
		void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStages supportedStages, string version, string commit);
		void Delete(Guid token);
		void Update(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStages supportedStages, string version, string commit);
		void IncrementVersion(Guid token);
	}
}
