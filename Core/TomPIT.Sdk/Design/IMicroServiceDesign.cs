using System;
using TomPIT.ComponentModel;

namespace TomPIT.Design
{
	public interface IMicroServiceDesign
	{
		void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStages supportedStages);
		void Delete(Guid token);
		void Update(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStages supportedStages);
	}
}
