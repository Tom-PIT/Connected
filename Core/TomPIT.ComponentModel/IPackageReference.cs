namespace TomPIT.ComponentModel;

public interface IPackageReference : IElement
{
	string PackageName { get; }
	string Version { get; }
}
