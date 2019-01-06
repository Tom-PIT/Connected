using TomPIT.Annotations;

namespace TomPIT.ComponentModel
{
	public class References : ConfigurationBase, IServiceReferences
	{
		private ListItems<IServiceReference> _references = null;

		[Items("TomPIT.Items.ReferencesCollection, TomPIT.Development")]
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
