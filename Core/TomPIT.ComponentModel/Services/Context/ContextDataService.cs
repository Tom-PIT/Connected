namespace TomPIT.Services.Context
{
	internal class ContextDataService : ContextClient, IContextDataService
	{
		private IContextDataAudit _audit = null;
		private IContextUserDataService _userData = null;

		public ContextDataService(IExecutionContext context) : base(context)
		{
		}

		public IContextDataAudit Audit
		{
			get
			{
				if (_audit == null)
					_audit = new ContextDataAudit(Context);

				return _audit;
			}
		}

		public IContextUserDataService User
		{
			get
			{
				if (_userData == null)
					_userData = new ContextUserDataService(Context);

				return _userData;
			}
		}
	}
}
