namespace TomPIT.Services.Context
{
	internal class ContextDataService : ContextClient, IContextDataService
	{
		private IContextDataAudit _audit = null;

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
	}
}
