using System;
using System.Collections.Generic;

namespace TomPIT.Navigation
{
	internal class NavigationHandlerDescriptor
	{
		private List<string> _keys = null;
		public NavigationHandlerDescriptor(Guid microService, Guid component, Type handler)
		{
			Component = component;
			Handler = handler;
			MicroService = microService;
		}

		public Guid MicroService { get; }
		public Type Handler { get; }
		public Guid Component { get; }

		public List<string> RouteKeys
		{
			get
			{
				if (_keys == null)
					_keys = new List<string>();

				return _keys;
			}
		}
	}
}
