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

		private bool Disposed { get; set; }

		[JsonIgnore]
		[SkipValidation]
		public IMicroServiceContext Context
		{
			get
			{
				if (_context == null && !Disposed)
					_context = new MicroServiceContext(_microService, MiddlewareDescriptor.Current.Tenant?.Url);

				return _context;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!Disposed)
			{
				if (disposing)
				{
					if (_context != null)
					{
						_context.Dispose();
						_context = null;
					}

					OnDisposing();
				}

				Disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			System.GC.SuppressFinalize(this);
		}

		protected virtual void OnDisposing()
		{

		}
	}
}
