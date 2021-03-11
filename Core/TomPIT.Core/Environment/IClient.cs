using System;

namespace TomPIT.Environment
{
	public enum ClientStatus
	{
		NotSet = 0,
		Enabled = 1,
		Disabled = 2
	}
	public interface IClient
	{
		string Token { get; }
		string Name { get; }
		DateTime Created { get; }
		ClientStatus Status { get; }
		string Type { get; }
	}
}
