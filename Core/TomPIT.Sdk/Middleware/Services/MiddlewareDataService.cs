namespace TomPIT.Middleware.Services
{
	internal class MiddlewareDataService : MiddlewareObject, IMiddlewareDataService
	{
		private IMiddlewareDataAudit _audit = null;
		private IMiddlewareUserDataService _userData = null;

		public MiddlewareDataService(IMiddlewareContext context) : base(context)
		{
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
