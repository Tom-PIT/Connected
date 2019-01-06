using TomPIT.SysDb.Data;

namespace TomPIT.SysDb.Sql.Data
{
	internal class DataHandler : IDataHandler
	{
		private IAuditHandler _audit = null;

		public IAuditHandler Audit
		{
			get
			{
				if (_audit == null)
					_audit = new AuditHandler();

				return _audit;
			}
		}
	}
}
