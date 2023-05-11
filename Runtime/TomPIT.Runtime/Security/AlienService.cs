using System;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	internal class AlienService : ClientRepository<IAlien, Guid>, IAlienService, IAlienNotification
	{
		public AlienService(ITenant tenant) : base(tenant, "alien")
		{
		}

		public void Delete(Guid token)
		{
			Instance.SysProxy.Alien.Delete(token);

			Remove(token);
		}

		public Guid Insert(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone, string resourceType = null, string resourcePrimaryKey = null)
		{
			return Instance.SysProxy.Alien.Insert(firstName, lastName, email, mobile, phone, language, timezone, resourceType, resourcePrimaryKey);
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
					return Instance.SysProxy.Alien.Select(token, null, null, null, null, null);
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

			r = Instance.SysProxy.Alien.Select(Guid.Empty, resourceType, resourcePrimaryKey, email, mobile, phone);

			if (r is not null)
				Set(r.Token, r);

			return r;
		}
		public IAlien Select(string email)
		{
			var r = Get(f => string.Compare(f.Email, email, true) == 0);

			if (r != null)
				return r;

			r = Instance.SysProxy.Alien.Select(Guid.Empty, email: email);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IAlien Select(string resourceType, string resourcePrimaryKey)
		{
			if (Get(f => string.Compare(f.ResourceType, resourceType, true) == 0 && string.Compare(f.ResourcePrimaryKey, resourcePrimaryKey, true) == 0) is IAlien r)
				return r;

			r = Instance.SysProxy.Alien.Select(Guid.Empty, resourceType: resourceType, resourcePrimaryKey: resourcePrimaryKey);

			if (r is not null)
				Set(r.Token, r);

			return r;
		}

		public IAlien SelectByMobile(string mobile)
		{
			var r = Get(f => string.Compare(f.Mobile, mobile, true) == 0);

			if (r != null)
				return r;

			r = Instance.SysProxy.Alien.Select(Guid.Empty, mobile: mobile);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IAlien SelectByPhone(string phone)
		{
			var r = Get(f => string.Compare(f.Phone, phone, true) == 0);

			if (r != null)
				return r;

			r = Instance.SysProxy.Alien.Select(Guid.Empty, phone: phone);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public void Update(Guid token, string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone, string resourceType = null, string resourcePrimaryKey = null)
		{
			Instance.SysProxy.Alien.Update(token, firstName, lastName, email, language, mobile, phone, timezone, resourceType, resourcePrimaryKey);

			Remove(token);
		}
	}
}
