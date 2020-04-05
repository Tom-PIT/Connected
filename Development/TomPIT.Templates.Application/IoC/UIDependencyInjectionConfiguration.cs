using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.IoC
{
	public class UIDependencyInjectionConfiguration : ComponentConfiguration, IUIDependencyInjectionConfiguration
	{
		private ListItems<IUIDependency> _injections = null;

		[Items(DesignUtils.UIDependencyInjectionsItems)]
		public ListItems<IUIDependency> Injections
		{
			get
			{
				if (_injections == null)
					_injections = new ListItems<IUIDependency> { Parent = this };

				return _injections;
			}
		}
	}
}