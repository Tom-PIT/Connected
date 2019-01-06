using System;

namespace TomPIT.ComponentModel.Features
{
	public interface IFeature
	{
		string Name { get; }
		Guid Token { get; }
		Guid MicroService { get; }
	}
}
