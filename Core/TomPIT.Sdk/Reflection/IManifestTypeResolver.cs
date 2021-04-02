using System;

namespace TomPIT.Reflection
{
	public interface IManifestTypeResolver: IDisposable
	{
		IManifestTypeDescriptor Resolve(string name);
	}
}
