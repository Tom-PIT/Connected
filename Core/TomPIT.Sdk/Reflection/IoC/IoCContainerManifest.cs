using System.Collections.Generic;

namespace TomPIT.Reflection.IoC
{
	public class IoCContainerManifest : ComponentManifest
	{
		private List<IoCOperationManifest> _operations = null;

		public IoCContainerManifest(IComponentManifestProvider provider) : base(provider)
		{

		}
		public List<IoCOperationManifest> Operations
		{
			get
			{
				if (_operations == null)
					_operations = new List<IoCOperationManifest>();

				return _operations;
			}
		}
	}
}
