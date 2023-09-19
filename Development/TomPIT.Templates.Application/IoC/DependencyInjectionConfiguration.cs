using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.IoC
{
	[ComponentCreatedHandler("TomPIT.MicroServices.Design.CreateHandlers.EventBinding, TomPIT.MicroServices.Design")]
	public class DependencyInjectionConfiguration : ComponentConfiguration, IDependencyInjectionConfiguration
	{
		private ListItems<IDependency> _injections = null;

		[Items(DesignUtils.DependencyInjectionsItems)]
		public ListItems<IDependency> Injections
		{
			get
			{
				if (_injections == null)
					_injections = new ListItems<IDependency> { Parent = this };

				return _injections;
			}
		}

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Namespace { get; set; }
	}
}
