using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.IoC
{
	[Manifest(DesignUtils.IoCManifest)]
	public class IoCContainerConfiguration : ComponentConfiguration, IIoCContainerConfiguration
	{
		private ListItems<IIoCOperation> _ops = null;

		[Items(DesignUtils.IoCOperationItems)]
		public ListItems<IIoCOperation> Operations
		{
			get
			{
				if (_ops == null)
					_ops = new ListItems<IIoCOperation> { Parent = this };

				return _ops;
			}
		}
	}
}
