using TomPIT.SysDb.Analytics;

namespace TomPIT.SysDb.Sql.Analytics
{
	internal class AnalyticsHandler : IAnalyticsHandler
	{
		private IMruHandler _mru = null;
		public IMruHandler Mru
		{
			get
			{
				if (_mru == null)
					_mru = new MruHandler();

				return _mru;
			}
		}
	}
}
