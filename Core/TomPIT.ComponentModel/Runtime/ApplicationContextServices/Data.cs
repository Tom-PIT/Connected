namespace TomPIT.Runtime.ApplicationContextServices
{
	internal class Data : ApplicationContextClient, IDataService
	{
		private IDataAudit _audit = null;

		public Data(IApplicationContext context) : base(context)
		{
		}

		public IDataAudit Audit
		{
			get
			{
				if (_audit == null)
					_audit = new DataAudit(Context);

				return _audit;
			}
		}
	}
}
