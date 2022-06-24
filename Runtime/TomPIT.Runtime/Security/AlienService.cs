using System;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Security
{
	internal class AlienService : ClientRepository<IAlien, Guid>, IAlienService, IAlienNotification
	{
		public AlienService(ITenant tenant) : base(tenant, "alien")
		{
		}

		public void Delete(Guid token)
		{
			var u = Tenant.CreateUrl("Alien", "Delete");
			var e = new JObject
			{
				{"token", token }
			};

			Tenant.Post(u, e);
			Remove(token);
		}

		public Guid Insert(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone, string resourceType = null, string resourcePrimaryKey = null)
		{
			var u = Tenant.CreateUrl("Alien", "Insert");
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

			return Tenant.Post<Guid>(u, e);
		}

		public void NotifyChanged(object sender, AlienEventArgs e)
		{
			Remove(e.Alien);
		}

		public IAlien Select(Guid token)
		{
			return Get(token,
				(f) =>
				{
					var u = Tenant.CreateUrl("Alien", "Select");
					var e = new JObject
					{
						{"token", token }
					};

					return Tenant.Post<Alien>(u, e);
				});
		}

		public IAlien Select(string firstName = null, string lastName = null, string email = null, string mobile = null, string phone = null, string resourceType = null, string resourcePrimaryKey = null)
		{
			if (Get(f => string.Compare(f.Email, email, true) == 0
				 && string.Compare(f.FirstName, firstName, true) == 0
				 && string.Compare(f.LastName, lastName, true) == 0
				 && string.Compare(f.Mobile, mobile, true) == 0
				 && string.Compare(f.Phone, phone, true) == 0
				 && string.Compare(f.ResourceType, resourceType, true) == 0
				 && string.Compare(f.ResourcePrimaryKey, resourcePrimaryKey, true) == 0) is IAlien r)
				return r;

			r = Tenant.Post<Alien>(Tenant.CreateUrl("Alien", "Select"), new
			{
				email,
				firstName,
				lastName,
				mobile,
				phone,
				resourceType,
				resourcePrimaryKey
			});

			if (r is not null)
				Set(r.Token, r);

			return r;
		}
		public IAlien Select(string email)
		{
			var r = Get(f => string.Compare(f.Email, email, true) == 0);

			if (r != null)
				return r;

			var u = Tenant.CreateUrl("Alien", "Select");
			var e = new JObject
			{
				{"email", email }
			};

			r = Tenant.Post<Alien>(u, e);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IAlien Select(string resourceType, string resourcePrimaryKey)
		{
			if (Get(f => string.Compare(f.ResourceType, resourceType, true) == 0 && string.Compare(f.ResourcePrimaryKey, resourcePrimaryKey, true) == 0) is IAlien r)
				return r;

			r = Tenant.Post<Alien>(Tenant.CreateUrl("Alien", "Select"), new
			{
				resourceType,
				resourcePrimaryKey
			});

			if (r is not null)
				Set(r.Token, r);

			return r;
		}

		public IAlien SelectByMobile(string mobile)
		{
			var r = Get(f => string.Compare(f.Mobile, mobile, true) == 0);

			if (r != null)
				return r;

			var u = Tenant.CreateUrl("Alien", "Select");
			var e = new JObject
			{
				{"mobile", mobile }
			};

			r = Tenant.Post<Alien>(u, e);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IAlien SelectByPhone(string phone)
		{
			var r = Get(f => string.Compare(f.Phone, phone, true) == 0);

			if (r != null)
				return r;

			var u = Tenant.CreateUrl("Alien", "Select");
			var e = new JObject
			{
				{"phone", phone }
			};

			r = Tenant.Post<Alien>(u, e);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public void Update(Guid token, string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone, string resourceType = null, string resourcePrimaryKey = null)
		{
			var u = Tenant.CreateUrl("Alien", "Update");
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

			Tenant.Post(u, e);
			Remove(token);
		}
	}
}
