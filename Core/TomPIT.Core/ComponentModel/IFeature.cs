using System;

namespace TomPIT.ComponentModel
{
	public interface IFeature
	{
		string Name { get; }
		Guid Token { get; }
		Guid MicroService { get; }
	}
}
