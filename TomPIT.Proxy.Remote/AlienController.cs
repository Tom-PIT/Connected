using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Collections;
using TomPIT.Security;

namespace TomPIT.Proxy.Remote
{
	internal class AlienController : IAlienController
	{
		private const string Controller = "Alien";

		public void Delete(Guid token)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Delete"), new
			{
				token
			});
		}

		public Guid Insert(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone, string resourceType, string resourcePrimaryKey)
		{
			var u = Connection.CreateUrl(Controller, "Insert");
			var e = new JObject();

			if (!string.IsNullOrWhiteSpace(firstName))
				e.Add("firstName", firstName);

			if (!string.IsNullOrWhiteSpace(lastName))
				e.Add("lastName", lastName);

			if (!string.IsNullOrWhiteSpace(email))
				e.Add("email", email);

			if (!string.IsNullOrWhiteSpace(mobile))
				e.Add("mobile", mobile);

			if (!string.IsNullOrWhiteSpace(phone))
				e.Add("phone", phone);

			if (language != Guid.Empty)
				e.Add("language", language);

			if (!string.IsNullOrWhiteSpace(timezone))
				e.Add("timezone", timezone);

			if (!string.IsNullOrWhiteSpace(resourceType))
				e.Add("resourceType", resourceType);

			if (!string.IsNullOrWhiteSpace(resourcePrimaryKey))
				e.Add("resourcePrimaryKey", resourcePrimaryKey);

			return Connection.Post<Guid>(u, e);
		}

		public ImmutableList<IAlien> Query()
		{
			return Connection.Get<List<Alien>>(Connection.CreateUrl(Controller, "Query")).ToImmutableList<IAlien>();
		}

		public IAlien Select(Guid token, string resourceType = null, string resourcePrimaryKey = null, string email = null, string mobile = null, string phone = null)
		{
			var u = Connection.CreateUrl(Controller, "Select");
			var e = new JObject
					{
						{"token", token }
					};

			return Connection.Post<Alien>(u, e);
		}

		public void Update(Guid token, string firstName, string lastName, string email, Guid language, string mobile, string phone, string timezone, string resourceType, string resourcePrimaryKey)
		{
			var u = Connection.CreateUrl(Controller, "Update");
			var e = new JObject
			{
				{"token", token }
			};

			if (!string.IsNullOrWhiteSpace(firstName))
				e.Add("firstName", firstName);

			if (!string.IsNullOrWhiteSpace(lastName))
				e.Add("lastName", lastName);

			if (!string.IsNullOrWhiteSpace(email))
				e.Add("email", email);

			if (!string.IsNullOrWhiteSpace(mobile))
				e.Add("mobile", mobile);

			if (!string.IsNullOrWhiteSpace(phone))
				e.Add("phone", phone);

			if (language != Guid.Empty)
				e.Add("language", language);

			if (!string.IsNullOrWhiteSpace(timezone))
				e.Add("timezone", timezone);

			if (!string.IsNullOrWhiteSpace(resourceType))
				e.Add("resourceType", resourceType);

			if (!string.IsNullOrWhiteSpace(resourcePrimaryKey))
				e.Add("resourcePrimaryKey", resourcePrimaryKey);

			Connection.Post(u, e);
		}
	}
}
