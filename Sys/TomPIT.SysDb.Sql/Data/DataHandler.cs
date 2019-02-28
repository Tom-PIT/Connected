using TomPIT.SysDb.Data;

namespace TomPIT.SysDb.Sql.Data
{
	internal class DataHandler : IDataHandler
	{
		private IAuditHandler _audit = null;
		private IUserDataHandler _userData = null;

		public IAuditHandler Audit
		{
			get
			{
				if (_audit == null)
					_audit = new AuditHandler();

				return _audit;
			}
		}

		public IUserDataHandler UserData
		{
			get
			{
				if (_userData == null)
					_userData = new UserDataHandler();

				return _userData;
			}
		}
	}
}
