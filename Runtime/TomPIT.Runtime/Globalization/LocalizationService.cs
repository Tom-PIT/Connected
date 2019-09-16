using TomPIT.Connectivity;

namespace TomPIT.Globalization
{
	internal class LocalizationService : TenantObject, ILocalizationService
	{
		public LocalizationService(ITenant tenant) : base(tenant)
		{
			Cache = new StringsCache(Tenant);
		}

		public string GetString(string microService, string stringTable, string key, int lcid, bool throwException)
		{
			return Cache.GetString(microService, stringTable, key, lcid, throwException);
		}

		private StringsCache Cache { get; }
	}
}
