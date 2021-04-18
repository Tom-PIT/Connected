using System.Collections.Generic;
using TomPIT.Design;

namespace TomPIT.MicroServices.Design.Designers
{
	internal class ModelAmbientProvider : IAmbientProvider
	{
		private List<IAmbientToolbarAction> _actions = null;
		public List<IAmbientToolbarAction> ToolbarActions
		{
			get
			{
				if (_actions == null)
				{
					_actions = new List<IAmbientToolbarAction>
					{
						new SynchronizeEntityToolbarAction()
					};
				}

				return _actions;
			}
		}
	}
}