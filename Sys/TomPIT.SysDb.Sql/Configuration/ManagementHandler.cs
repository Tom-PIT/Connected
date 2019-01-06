using TomPIT.SysDb.Management;

namespace TomPIT.SysDb.Sql.Configuration
{
	internal class ManagementHandler : IManagementHandler
	{
		private ISettingHandler _settings = null;

		public ISettingHandler Settings
		{
			get
			{
				if (_settings == null)
					_settings = new SettingHandler();

				return _settings;
			}
		}
	}
}
