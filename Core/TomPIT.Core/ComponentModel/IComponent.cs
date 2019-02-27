using System;
using TomPIT.Development;

namespace TomPIT.ComponentModel
{
	public interface IComponent
	{
		string Name { get; }
		Guid MicroService { get; }
		Guid Token { get; }
		string Type { get; }
		string Category { get; }
		Guid RuntimeConfiguration { get; }
		DateTime Modified { get; }
		Guid Folder { get; }
		LockStatus LockStatus { get; }
		Guid LockUser { get; }
		DateTime LockDate { get; }
		LockVerb LockVerb { get; }
	}
}
