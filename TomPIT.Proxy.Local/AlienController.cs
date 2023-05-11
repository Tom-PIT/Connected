using System;
using System.Collections.Immutable;
using TomPIT.Security;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class AlienController : IAlienController
	{
		public void Delete(Guid token)
		{
			DataModel.Aliens.Delete(token);
		}

		public Guid Insert(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone, string resourceType, string resourcePrimaryKey)
		{
			return DataModel.Aliens.Insert(firstName, lastName, email, mobile, phone, language, timezone, resourceType, resourcePrimaryKey);
		}

		public ImmutableList<IAlien> Query()
		{
			return DataModel.Aliens.Query();
		}

		public IAlien Select(Guid token, string resourceType = null, string resourcePrimaryKey = null, string email = null, string mobile = null, string phone = null)
		{
			if (token != Guid.Empty)
				return DataModel.Aliens.Select(token);

			if (!string.IsNullOrWhiteSpace(resourceType))
				return DataModel.Aliens.Select(resourceType, resourcePrimaryKey);

			if (!string.IsNullOrWhiteSpace(email))
				return DataModel.Aliens.Select(email);

			if (!string.IsNullOrWhiteSpace(mobile))
				return DataModel.Aliens.Select(mobile);

			if (!string.IsNullOrWhiteSpace(phone))
				return DataModel.Aliens.Select(phone);

			return null;
		}

		public void Update(Guid token, string firstName, string lastName, string email, Guid language, string mobile, string phone, string timezone, string resourceType, string resourcePrimaryKey)
		{
			DataModel.Aliens.Update(token, firstName, lastName, email, mobile, phone, language, timezone, resourceType, resourcePrimaryKey);
		}
	}
}
