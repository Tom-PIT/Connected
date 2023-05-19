using System.Reflection;

namespace TomPIT.Startup;
public sealed class ApplicationPartsArgs : EventArgs
{
	public ApplicationPartsArgs()
	{
		Assemblies = new();
		Parts = new();
	}

	public List<Assembly> Assemblies { get; }
	public List<string> Parts { get; }
}
