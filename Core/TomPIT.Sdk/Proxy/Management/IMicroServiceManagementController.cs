using System;
using TomPIT.ComponentModel;

namespace TomPIT.Proxy.Management
{
	public interface IMicroServiceManagementController
	{
		void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, string meta);
		void Delete(Guid token);
		void Update(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, UpdateStatus updateStatus, CommitStatus commitStatus, Guid package, Guid plan);
	}
}
