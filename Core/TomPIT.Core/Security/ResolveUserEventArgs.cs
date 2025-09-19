using System;

namespace TomPIT.Security;
public class ResolveUserEventArgs : EventArgs
{
	public ResolveUserEventArgs()
	{

	}
	public ResolveUserEventArgs(string qualifier)
	{
		Qualifier = qualifier;
	}

	public string? Qualifier { get; set; }
	public IUser? User { get; set; }
}
