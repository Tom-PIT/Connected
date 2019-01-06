using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class QaHandler : IQaHandler
	{
		private IApiTestHandler _api = null;

		public IApiTestHandler Api
		{
			get
			{
				if (_api == null)
					_api = new ApiTestHandler();

				return _api;
			}
		}
	}
}
