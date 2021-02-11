using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Globalization;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers.Management
{
	public class GlobalizationManagementController : SysController
	{
		[HttpPost]
		public Guid InsertLanguage()
		{
			var body = FromBody();

			var name = body.Required<string>("name");
			var lcid = body.Required<int>("lcid");
			var status = body.Required<LanguageStatus>("status");
			var mappings = body.Optional("mappings", string.Empty);

			return DataModel.Languages.Insert(name, lcid, status, mappings);
		}

		[HttpPost]
		public void UpdateLanguage()
		{
			var body = FromBody();

			var token = body.Required<Guid>("token");
			var name = body.Required<string>("name");
			var lcid = body.Required<int>("lcid");
			var status = body.Required<LanguageStatus>("status");
			var mappings = body.Optional("mappings", string.Empty);

			DataModel.Languages.Update(token, name, lcid, status, mappings);
		}

		[HttpPost]
		public void DeleteLanguage()
		{
			var body = FromBody();

			var token = body.Required<Guid>("token");

			DataModel.Languages.Delete(token);
		}
	}
}
