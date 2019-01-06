using System;

namespace TomPIT.Environment
{
	public interface IResourceGroup
	{
		string Name { get; }
		Guid Token { get; }
	}
}
