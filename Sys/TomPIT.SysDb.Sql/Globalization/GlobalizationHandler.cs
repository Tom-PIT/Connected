using TomPIT.SysDb.Globalization;

namespace TomPIT.SysDb.Sql.Globalization
{
	internal class GlobalizationHandler : IGlobalizationHandler
	{
		private ILanguageHandler _languages = null;

		public ILanguageHandler Languages
		{
			get
			{
				if (_languages == null)
					_languages = new LanguageHandler();

				return _languages;
			}
		}
	}
}
