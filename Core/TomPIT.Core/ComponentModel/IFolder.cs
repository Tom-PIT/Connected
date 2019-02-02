using System;

namespace TomPIT.ComponentModel
{
	public interface IFolder
	{
		string Name { get; }
		Guid Token { get; }
		Guid MicroService { get; }
		Guid Parent { get; }
	}
}
