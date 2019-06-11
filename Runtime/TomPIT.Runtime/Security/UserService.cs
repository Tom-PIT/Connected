using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Storage;

namespace TomPIT.Security
{
	internal class UserService : ClientRepository<IUser, Guid>, IUserService, IUserNotification
	{
		public UserService(ISysConnection connection) : base(connection, "user")
		{

		}

		public event UserChangedHandler UserChanged;

		public List<IUser> Query()
		{
			var u = Connection.CreateUrl("User", "Query");

			return Connection.Get<List<User>>(u).ToList<IUser>();
		}

		public IUser Select(string qualifier)
		{
			IUser r = null;

			if (Guid.TryParse(qualifier, out Guid g))
				r = Get(g);
			else if (qualifier.Contains("@"))
				r = Get(f => string.Compare(f.Email, qualifier, true) == 0);
			else
				r = Get(f => string.Compare(f.LoginName, qualifier, true) == 0);

			if (r != null)
				return r;

			var u = Connection.CreateUrl("User", "Select")
				.AddParameter("qualifier", qualifier);

			r = Connection.Get<User>(u);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IUser SelectByAuthenticationToken(Guid token)
		{
			var r = Get(f => f.AuthenticationToken == token);

			if (r != null)
				return r;

			var u = Connection.CreateUrl("User", "SelectByAuthenticationToken")
				.AddParameter("token", token);

			r = Connection.Get<User>(u);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public void ChangePassword(Guid user, string existingPassword, string password)
		{
			var u = Connection.CreateUrl("UserManagement", "ChangePassword");
			var e = new JObject
			{
				{"user",user },
				{"existingPassword",existingPassword },
				{"newPassword",password }
			};

			Connection.Post(u, e);
		}

		public void Logout(int user)
		{
			throw new NotImplementedException();
		}

		public void NotifyChanged(object sender, UserEventArgs e)
		{
			Remove(e.User);

			UserChanged?.Invoke(Connection, e);
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

				Connection.GetService<IStorageService>().Delete(usr.Avatar);
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

				avatarId = Connection.GetService<IStorageService>().Upload(b, contentBytes, StoragePolicy.Singleton);

				if (avatarId == usr.Avatar)
					return;
			}

			var u = Connection.CreateUrl("UserManagement", "ChangeAvatar");
			var e = new JObject
			{
				{"user", user},
				{"avatar", avatarId }
			};

			Connection.Post(u, e);

			NotifyChanged(this, new UserEventArgs(user));
		}
	}
}