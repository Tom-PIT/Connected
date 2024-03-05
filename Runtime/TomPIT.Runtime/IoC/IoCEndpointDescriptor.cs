using System;

using TomPIT.Compilation;
using TomPIT.ComponentModel.IoC;
using TomPIT.Connectivity;

namespace TomPIT.IoC
{
	internal class IoCEndpointDescriptor
	{
		private Type _type = null;
		private bool _typeInitialized = false;
		private object _sync = new object();
		public IoCEndpointDescriptor(ITenant tenant)
		{
			Tenant = tenant;
		}

		private ITenant Tenant { get; }

		public Guid Component { get; set; }
		public Guid Element => Endpoint.Id;
		public Guid MicroService { get; set; }
		public IIoCEndpoint Endpoint { get; set; }

		public Type Type => _type;
		//{
		//	get
		//	{
		//		if (_type == null && !_typeInitialized)
		//		{
		//			lock (_sync)
		//				if (_type == null && !_typeInitialized)
		//				{
		//					_typeInitialized = true;
		//					_type = Tenant.GetService<ICompilerService>().ResolveType(MicroService, Endpoint, Endpoint.Name, false);
		//				}
		//		}

		//		return _type;
		//	}
		//}

		public bool Initialized => _typeInitialized;
		public void Initialize()
		{
			if (_typeInitialized)
				return;

			lock (_sync)
			{
				if (_typeInitialized)
					return;

				_type = Tenant.GetService<ICompilerService>().ResolveType(MicroService, Endpoint, Endpoint.Name, false);
				_typeInitialized = true;
			}
		}
	}
}
