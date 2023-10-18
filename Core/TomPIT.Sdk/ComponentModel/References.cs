using TomPIT.Annotations.Design;
using TomPIT.Collections;

namespace TomPIT.ComponentModel
{
	public class References : ComponentConfiguration, IServiceReferencesConfiguration
	{
		private ListItems<IServiceReference> _references = null;
		private ListItems<IAssemblyReference> _assemblies = null;

		[Items("TomPIT.Design.Items.ReferencesCollection, TomPIT.Design")]
		public ListItems<IServiceReference> MicroServices
		{
			get
			{
				if (_references is null)
					_references = new ListItems<IServiceReference> { Parent = this };

				return _references;
			}
		}

		public ListItems<IAssemblyReference> Assemblies
		{
			get
			{
				if (_assemblies is null)
					_assemblies = new ListItems<IAssemblyReference> { Parent = this };

				return _assemblies;
			}
		}
	}
}
