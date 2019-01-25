using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Environment;
using TomPIT.Globalization;

namespace TomPIT.SysDb.Development
{
	public interface IMicroServiceHandler
	{
		List<IMicroService> Query();

		IMicroService SelectByUrl(string url);
		IMicroService Select(Guid token);
		IMicroService Select(string name);

		void Insert(Guid token, string name, string url, MicroServiceStatus status, IResourceGroup resourceGroup, Guid template, string meta);
		void Update(IMicroService microService, string name, string url, MicroServiceStatus status, Guid template, IResourceGroup resourceGroup, Guid package);

		void Delete(IMicroService microService);
		void UpdateMeta(IMicroService microService, byte[] meta);

		IMicroServiceString SelectString(IMicroService microService, ILanguage language, Guid element, string property);
		List<IMicroServiceString> QueryStrings();
		void UpdateString(IMicroService microService, ILanguage language, Guid element, string property, string value);
		void DeleteString(IMicroService microService, Guid element, string property);

		string SelectMeta(IMicroService microService);
	}
}
