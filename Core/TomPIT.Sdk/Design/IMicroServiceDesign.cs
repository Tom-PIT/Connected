using System;
using TomPIT.ComponentModel;

namespace TomPIT.Design
{
	public interface IMicroServiceDesign
	{
		void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status);
		void Update(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, UpdateStatus updateStatus, CommitStatus commitStatus);
	}
}
