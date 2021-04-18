using System;

namespace TomPIT.Design
{
	public interface IChangeFolder
	{
		Guid Id { get; }
		string Name { get; }
		Guid Parent { get; }
	}
}
