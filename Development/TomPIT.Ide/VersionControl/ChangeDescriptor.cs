using System.Collections.Generic;

namespace TomPIT.Ide.VersionControl
{
	public class ChangeDescriptor : IChangeDescriptor
	{
		private List<IChangeMicroService> _microServices = null;

		public List<IChangeMicroService> MicroServices
		{
			get
			{
				if (_microServices == null)
					_microServices = new List<IChangeMicroService>();

				return _microServices;
			}
		}
	}
}
