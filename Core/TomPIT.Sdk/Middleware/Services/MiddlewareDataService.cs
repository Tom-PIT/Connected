namespace TomPIT.Middleware.Services
{
	internal class MiddlewareDataService : MiddlewareObject, IMiddlewareDataService
	{
		private IMiddlewareDataAudit _audit = null;
		private IMiddlewareUserDataService _userData = null;
		private IMiddlewareLockingService _locking = null;

		public MiddlewareDataService(IMiddlewareContext context) : base(context)
		{
		}

		public IMiddlewareLockingService Locking
		{
			get
			{
				if (_locking == null)
					_locking = new MiddlewareLockingService(Context);

				return _locking;
			}
		}

		public IMiddlewareDataAudit Audit
		{
			get
			{
				if (_audit == null)
					_audit = new MiddlewareDataAudit(Context);

				return _audit;
			}
		}

		public IMiddlewareUserDataService User
		{
			get
			{
				if (_userData == null)
					_userData = new MiddlewareUserDataService(Context);

				return _userData;
			}
		}
	}
}
