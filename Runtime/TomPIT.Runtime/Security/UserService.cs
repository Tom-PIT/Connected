using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Storage;

namespace TomPIT.Security
{
	internal class UserService : ClientRepository<IUser, Guid>, IUserService, IUserNotification
	{
		public UserService(ITenant tenant) : base(tenant, "user")
		{

		}

		public event UserChangedHandler UserChanged;

		public List<IUser> Query()
		{
			var u = Tenant.CreateUrl("User", "Query");

			return Tenant.Get<List<User>>(u).ToList<IUser>();
		}

		public IUser Select(string qualifier)
		{
			if (string.IsNullOrWhiteSpace(qualifier))
				return null;

			IUser r = null;

			if (Guid.TryParse(qualifier, out var g))
				r = Get(g);
			else if (qualifier.Contains("@"))
				r = Get(f => string.Compare(f.Email, qualifier, true) == 0);
			else
				r = Get(f => string.Compare(f.LoginName, qualifier, true) == 0);

			if (r != null)
				return r;

			var u = Tenant.CreateUrl("User", "Select")
				.AddParameter("qualifier", qualifier);

			r = Tenant.Get<User>(u);

			if (r != null)
				Set(r.Token, r);
			else if (Guid.TryParse(qualifier, out var parsedQualifier))
				Set(parsedQualifier, null, TimeSpan.FromMinutes(1));
		
			return r;
		}

		public IUser SelectByAuthenticationToken(Guid token)
		{
			var r = Get(f => f.AuthenticationToken == token);

			if (r != null)
				return r;

			var u = Tenant.CreateUrl("User", "SelectByAuthenticationToken")
				.AddParameter("token", token);

			r = Tenant.Get<User>(u);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IUser SelectBySecurityCode(string securityCode)
		{
			if (string.IsNullOrWhiteSpace(securityCode))
				return null;

			var r = Get(f => string.Compare(f.SecurityCode, securityCode, false) == 0);

			if (r != null)
				return r;

			r = Tenant.Post<User>(Tenant.CreateUrl("User", "SelectBySecurityCode"), new
			{
				securityCode
			});

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public void ChangePassword(Guid user, string existingPassword, string password)
		{
			var u = Tenant.CreateUrl("UserManagement", "ChangePassword");
			var e = new JObject
			{
				{"user",user },
				{"existingPassword",existingPassword },
				{"newPassword",password }
			};

			Tenant.Post(u, e);
		}

		public void Logout(int user)
		{
			throw new NotImplementedException();
		}

		public void NotifyChanged(object sender, UserEventArgs e)
		{
			Remove(e.User);

			UserChanged?.Invoke(Tenant, e);
		}

		public void ChangeAvatar(Guid user, byte[] contentBytes, string contentType, string fileName)
		{
			var usr = Select(user.ToString());
			var avatarId = Guid.Empty;

			if (usr == null)
				throw new TomPITException(SR.ErrUserNotFound);

			if (contentBytes == null)
			{
				if (usr.Avatar == Guid.Empty)
					return;

				Tenant.GetService<IStorageService>().Delete(usr.Avatar);
			}
			else
			{
				var b = new Blob
				{
					ContentType = contentType,
					FileName = fileName,
					PrimaryKey = user.ToString(),
					ResourceGroup = Guid.Empty,
					Size = contentBytes == null ? 0 : contentBytes.Length,
					Type = BlobTypes.Avatar
				};

				avatarId = Tenant.GetService<IStorageService>().Upload(b, contentBytes, StoragePolicy.Singleton);

				if (avatarId == usr.Avatar)
					return;
			}

			var u = Tenant.CreateUrl("UserManagement", "ChangeAvatar");
			var e = new JObject
			{
				{"user", user},
				{"avatar", avatarId }
			};

			Tenant.Post(u, e);

			NotifyChanged(this, new UserEventArgs(user));
		}
	}
}