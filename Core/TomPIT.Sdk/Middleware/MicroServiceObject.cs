using System;
using Newtonsoft.Json;
using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.Middleware
{
	public class MicroServiceObject : IMicroServiceObject
	{
		private IMicroServiceContext _context = null;
		private IMicroService _microService = null;

		protected MicroServiceObject(Guid microService)
		{
			if (microService != Guid.Empty)
				_microService = MiddlewareDescriptor.Current.Tenant?.GetService<IMicroServiceService>().Select(microService);
		}

		protected MicroServiceObject(IMicroServiceContext context)
		{
			_context = context;
		}

		[JsonIgnore]
		[SkipValidation]
		public IMicroServiceContext Context
		{
			get
			{
				if (_context == null)
					_context = new MicroServiceContext(_microService, MiddlewareDescriptor.Current.Tenant?.Url);

				return _context;
			}
		}
	}
}
