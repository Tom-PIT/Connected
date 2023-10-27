using System;

namespace TomPIT.Design;
public class FileArgs : ComponentArgs
{
	public FileArgs(Guid microService, Guid component, Guid file) : base(microService, component, string.Empty)
	{
		File = file;
	}

	public Guid File { get; }
}
