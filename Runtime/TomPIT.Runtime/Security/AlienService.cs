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

		public Guid Insert(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone)
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

		public void Update(Guid token, string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone)
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

			Tenant.Post(u, e);
			Remove(token);
		}
	}
}
