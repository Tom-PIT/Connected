using System;

namespace TomPIT.Deployment
{
	public interface IPackageComponent
	{
		string Name { get; }
		Guid Token { get; }
		string Type { get; }
		Guid Feature { get; }
		string Category { get; }
		Guid RuntimeConfiguration { get; }
	}
}
