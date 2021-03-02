using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Security;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class AlienController : SysController
	{
		[HttpGet]
		public ImmutableList<IAlien> Query()
		{
			return DataModel.Aliens.Query();
		}

		[HttpPost]
		public IAlien Select()
		{
			var body = FromBody();
			var token = body.Optional("token", Guid.Empty);

			if (token != Guid.Empty)
				return DataModel.Aliens.Select(token);

			var email = body.Optional("email", string.Empty);

			if (!string.IsNullOrWhiteSpace(email))
				return DataModel.Aliens.Select(email);

			var mobile = body.Optional("mobile", string.Empty);

			if (!string.IsNullOrWhiteSpace(mobile))
				return DataModel.Aliens.Select(mobile);

			var phone = body.Required<string>("phone");

			return DataModel.Aliens.Select(phone);
		}

		[HttpPost]
		public Guid Insert()
		{
			var body = FromBody();
			var firstName = body.Optional("firstName", string.Empty);
			var lastName = body.Optional("lastName", string.Empty);
			var email = body.Optional("email", string.Empty);
			var language = body.Optional("language", Guid.Empty);
			var mobile = body.Optional("mobile", string.Empty);
			var phone = body.Optional("phone", string.Empty);
			var timezone = body.Optional("timezone", string.Empty);

			return DataModel.Aliens.Insert(firstName, lastName, email, mobile, phone, language, timezone);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");
			var firstName = body.Optional("firstName", string.Empty);
			var lastName = body.Optional("lastName", string.Empty);
			var email = body.Optional("email", string.Empty);
			var language = body.Optional("language", Guid.Empty);
			var mobile = body.Optional("mobile", string.Empty);
			var phone = body.Optional("phone", string.Empty);
			var timezone = body.Optional("timezone", string.Empty);

			DataModel.Aliens.Update(token, firstName, lastName, email, mobile, phone, language, timezone);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			DataModel.Aliens.Delete(token);
		}
	}
}
