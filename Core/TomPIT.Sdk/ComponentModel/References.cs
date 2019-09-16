using TomPIT.Annotations.Design;
using TomPIT.Collections;

namespace TomPIT.ComponentModel
{
	public class References : ComponentConfiguration, IServiceReferencesConfiguration
	{
		private ListItems<IServiceReference> _references = null;

		[Items("TomPIT.Design.Items.ReferencesCollection, TomPIT.Design")]
		public ListItems<IServiceReference> MicroServices
		{
			get
			{
				if (_references == null)
					_references = new ListItems<IServiceReference> { Parent = this };

				return _references;
			}
		}
	}
}
