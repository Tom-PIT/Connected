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
	}
}