using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Storage;

namespace TomPIT.Security
{
	internal class UserService : ClientRepository<IUser, Guid>, IUserService, IUserNotification
	{
		public event UserChangedHandler UserChanged;
		public UserService(ITenant tenant) : base(tenant, "user")
		{

		}

		public List<IUser> Query()
		{
			return Instance.SysProxy.Users.Query().ToList();
		}

		public IUser Select(int id)
		{
			var result = Get(f => f.Id == id);

			if (result is not null)
				return result;

			result = Instance.SysProxy.Users.Select(id.ToString());

			if (result is not null)
				Set(result.Token, result);

			return result;
		}

		public IUser Select(string qualifier)
		{
			if (string.IsNullOrWhiteSpace(qualifier))
				return null;

			IUser r = null;

			if (Guid.TryParse(qualifier, out var g))
				r = Get(g);
			else if (qualifier.Contains("@"))
				r = Get(f => string.Equals(f.Email, qualifier, StringComparison.OrdinalIgnoreCase));
			else
				r = Get(f => string.Equals(f.LoginName, qualifier, StringComparison.OrdinalIgnoreCase));

			if (r is not null)
				return r;

			r = Instance.SysProxy.Users.Select(qualifier);

			if (r is not null)
				Set(r.Token, r);

			return r;
		}

		public IUser SelectByAuthenticationToken(Guid token)
		{
			var r = Get(f => f.AuthenticationToken == token);

			if (r is not null)
				return r;

			r = Instance.SysProxy.Users.SelectByAuthenticationToken(token);

			if (r is not null)
				Set(r.Token, r);

			return r;
		}

		public IUser SelectBySecurityCode(string securityCode)
		{
			if (string.IsNullOrWhiteSpace(securityCode))
				return null;

			var r = Get(f => string.Equals(f.SecurityCode, securityCode, StringComparison.OrdinalIgnoreCase));

			if (r is not null)
				return r;

			r = Instance.SysProxy.Users.SelectBySecurityCode(securityCode);

			if (r is not null)
				Set(r.Token, r);

			return r;
		}

		public void ChangePassword(Guid user, string existingPassword, string password)
		{
			Instance.SysProxy.Management.Users.ChangePassword(user, existingPassword, password);
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

			if (usr is null)
				throw new TomPITException(SR.ErrUserNotFound);

			if (contentBytes is null)
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
					Size = contentBytes is null ? 0 : contentBytes.Length,
					Type = BlobTypes.Avatar
				};

				avatarId = Tenant.GetService<IStorageService>().Upload(b, contentBytes, StoragePolicy.Singleton);

				if (avatarId == usr.Avatar)
					return;
			}

			Instance.SysProxy.Management.Users.ChangeAvatar(user, avatarId);

			NotifyChanged(this, new UserEventArgs(user));
		}

		public Guid Insert(string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language, string timezone, bool notificationsEnabled, string mobile, string phone, string password, string securityCode)
		{
			return Instance.SysProxy.Management.Users.Insert(loginName, email, status, firstName, lastName, description, pin, language, timezone, notificationsEnabled, mobile, phone, DateTime.MinValue, securityCode);
		}

		public void Update(Guid token, string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language, string timezone, bool notificationsEnabled, string mobile, string phone, string securityCode)
		{
			Instance.SysProxy.Management.Users.Update(token, loginName, email, status, firstName, lastName, description, pin, language, timezone, notificationsEnabled, mobile, phone, DateTime.MinValue, securityCode);
		}
	}
}