using Newtonsoft.Json.Linq;
using System;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	internal class AlienService : ClientRepository<IAlien, Guid>, IAlienService, IAlienNotification
	{
		public AlienService(ISysConnection connection) : base(connection, "alien")
		{
		}

		public void Delete(Guid token)
		{
			var u = Connection.CreateUrl("Alien", "Delete");
			var e = new JObject
			{
				{"token", token }
			};

			Connection.Post(u, e);
			Remove(token);
		}

		public Guid Insert(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone)
		{
			var u = Connection.CreateUrl("Alien", "Insert");
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

			return Connection.Post<Guid>(u, e);
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
					var u = Connection.CreateUrl("Alien", "Select");
					var e = new JObject
					{
						{"token", token }
					};

					return Connection.Post<Alien>(u, e);
				});
		}

		public IAlien Select(string email)
		{
			var r = Get(f => string.Compare(f.Email, email, true) == 0);

			if (r != null)
				return r;

			var u = Connection.CreateUrl("Alien", "Select");
			var e = new JObject
			{
				{"email", email }
			};

			r = Connection.Post<Alien>(u, e);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IAlien SelectByMobile(string mobile)
		{
			var r = Get(f => string.Compare(f.Mobile, mobile, true) == 0);

			if (r != null)
				return r;

			var u = Connection.CreateUrl("Alien", "Select");
			var e = new JObject
			{
				{"mobile", mobile }
			};

			r = Connection.Post<Alien>(u, e);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IAlien SelectByPhone(string phone)
		{
			var r = Get(f => string.Compare(f.Phone, phone, true) == 0);

			if (r != null)
				return r;

			var u = Connection.CreateUrl("Alien", "Select");
			var e = new JObject
			{
				{"phone", phone }
			};

			r = Connection.Post<Alien>(u, e);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public void Update(Guid token, string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone)
		{
			var u = Connection.CreateUrl("Alien", "Update");
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

			Connection.Post(u, e);
			Remove(token);
		}
	}
}
