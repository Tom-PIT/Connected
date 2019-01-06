using TomPIT.Annotations;

namespace TomPIT.ComponentModel
{
	public class References : ComponentConfiguration, IServiceReferences
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
