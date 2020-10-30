using System;

namespace TomPIT.Connectivity
{
	public interface ICurrentCredentials : ICredentials
	{
		Guid Token { get; }
	}
}
