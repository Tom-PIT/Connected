using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Exceptions;
using TomPIT.Globalization;
using TomPIT.Routing;
using TomPIT.Security;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;
using TomPIT.Sys.Security;

namespace TomPIT.Sys.Data
{
	internal class Users : SynchronizedRepository<IUser, Guid>
	{
		public Users(IMemoryCache container) : base(container, "user")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Security.Users.Query();

			foreach (var i in ds)
			{
				DecryptSecurityCode(i);

				Set(i.Token, i, TimeSpan.Zero);
			}
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Security.Users.Select(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			DecryptSecurityCode(r);

			Set(id, r, TimeSpan.Zero);
		}

		public IUser SelectByAuthenticationToken(Guid token)
		{
			var r = Get(f => f.AuthenticationToken == token);

			if (r != null)
				return r;

			r = Shell.GetService<IDatabaseService>().Proxy.Security.Users.SelectByAuthenticationToken(token);

			if (r != null)
			{
				DecryptSecurityCode(r);

				Set(r.Token, r, TimeSpan.Zero);
			}

			return r;
		}

		public IUser SelectBySecurityCode(string code)
		{
			var r = Get(f => string.Compare(f.SecurityCode, code, false) == 0);

			if (r != null)
				return r;

			r = Shell.GetService<IDatabaseService>().Proxy.Security.Users.SelectBySecurityCode(code);

			if (r != null)
			{
				DecryptSecurityCode(r);
				Set(r.Token, r, TimeSpan.Zero);
			}

			return r;
		}

		public IUser Select(Guid token)
		{
			return Get(token,
				(f) =>
				{
					var r = Shell.GetService<IDatabaseService>().Proxy.Security.Users.Select(token);

					if (r != null)
						DecryptSecurityCode(r);

					return r;
				});
		}

		public IUser SelectByEmail(string email)
		{
			var r = Get(f => string.Compare(f.Email, email, true) == 0);

			if (r != null)
				return r;

			r = Shell.GetService<IDatabaseService>().Proxy.Security.Users.SelectByEmail(email);

			if (r != null)
			{
				DecryptSecurityCode(r);
				Set(r.Token, r, TimeSpan.Zero);
			}

			return r;
		}

		public IUser SelectByLoginName(string loginName)
		{
			var r = Get(f => string.Compare(loginName, f.LoginName, true) == 0);

			if (r != null)
				return r;

			var domainUsers = Where(f => !f.IsLocal());

			r = domainUsers.FirstOrDefault(f => string.Compare(loginName, f.DomainLoginName(), true) == 0);

			if (r != null)
				return r;

			r = Shell.GetService<IDatabaseService>().Proxy.Security.Users.Select(loginName);

			if (r != null)
			{
				DecryptSecurityCode(r);

				Set(r.Token, r, TimeSpan.Zero);
			}

			return r;
		}


		public IUser Resolve(string identifier)
		{
			IUser r = null;

			if (Guid.TryParse(identifier, out Guid g))
				r = Select(g);

			if (r != null)
				return r;

			if (identifier.Contains("@"))
			{
				r = SelectByEmail(identifier);

				if (r != null)
					return r;
			}

			r = SelectByLoginName(identifier);

			if (r != null)
				return r;

			return null;
		}

		public Guid Insert(string loginName, string email, UserStatus status, string firstName, string lastName, string description,
			string pin, Guid language, string timezone, bool notificationEnabled, string mobile, string phone,
			DateTime passwordChange, string securityCode)
		{
			ValidateSecurityCode(Guid.Empty, securityCode);

			if (!string.IsNullOrWhiteSpace(email) && SelectByEmail(email) != null)
				throw new SysException(SR.ErrEmailInUse);

			if (!string.IsNullOrWhiteSpace(loginName) && SelectByLoginName(loginName) != null)
				throw new SysException(SR.ErrLoginInUse);

			ILanguage l = null;

			if (language != Guid.Empty)
			{
				l = DataModel.Languages.Select(language);

				if (l == null)
					throw new SysException(SR.ErrLanguageNotFound);
			}

			var token = Guid.NewGuid();
			var pn = SecurityUtils.DisplayName(firstName, lastName, loginName, email, token);
			var url = Url(token, pn);

			Shell.GetService<IDatabaseService>().Proxy.Security.Users.Insert(token, loginName, url, email, status, firstName, lastName, description,
				pin, l, timezone, notificationEnabled, mobile, phone, Guid.Empty, Guid.NewGuid(), passwordChange, EncryptSecurityCode(securityCode));

			Select(token);
			CachingNotifications.UserChanged(token);

			return token;
		}

		public void Update(Guid token, string loginName, string email, UserStatus status, string firstName, string lastName, string description,
			string pin, Guid language, string timezone, bool notificationEnabled, string mobile, string phone, DateTime passwordChange, string securityCode)
		{
			var u = Select(token);

			if (u == null)
				throw new SysException(SR.ErrUserNotFound);

			if (!string.IsNullOrWhiteSpace(email) && string.Compare(email, u.Email, true) != 0 && SelectByEmail(email) != null)
				throw new SysException(SR.ErrEmailInUse);

			if (!string.IsNullOrWhiteSpace(loginName) && string.Compare(loginName, u.LoginName, true) != 0 && SelectByLoginName(loginName) != null)
				throw new SysException(SR.ErrLoginInUse);

			ValidateSecurityCode(u.Token, securityCode);

			Update(u, loginName, email, status, firstName, lastName, description, pin, language, timezone, notificationEnabled, mobile,
				phone, u.Avatar, passwordChange, EncryptSecurityCode(securityCode));
		}

		private void Update(IUser user, string loginName, string email, UserStatus status, string firstName, string lastName, string description,
			string pin, Guid language, string timezone, bool notificationEnabled, string mobile, string phone, Guid avatar, DateTime passwordChange, string securityCode)
		{
			ILanguage l = null;

			if (language != Guid.Empty)
			{
				l = DataModel.Languages.Select(language);

				if (l == null)
					throw new SysException(SR.ErrLanguageNotFound);
			}

			var dn = user.DisplayName();
			var pn = SecurityUtils.DisplayName(firstName, lastName, loginName, email, user.Token);
			var url = user.Url;

			if (string.Compare(dn, pn, true) != 0)
				url = Url(user.Token, pn);

			if (!string.IsNullOrWhiteSpace(email) && string.Compare(email, user.Email, true) != 0 && SelectByEmail(email) != null)
				throw new SysException(SR.ErrEmailInUse);

			if (!string.IsNullOrWhiteSpace(loginName) && string.Compare(loginName, user.LoginName, true) != 0 && SelectByLoginName(loginName) != null)
				throw new SysException(SR.ErrLoginInUse);

			ValidateSecurityCode(u.Token, securityCode);

			Shell.GetService<IDatabaseService>().Proxy.Security.Users.Update(user, loginName, url, email, status, firstName, lastName, description,
				pin, l, timezone, notificationEnabled, mobile, phone, avatar, passwordChange, EncryptSecurityCode(securityCode));

			Refresh(user.Token);
			CachingNotifications.UserChanged(user.Token);
		}

		public void ChangeAvatar(Guid token, Guid blob)
		{
			var u = Select(token);

			if (u == null)
				throw new SysException(SR.ErrUserNotFound);

			Update(u, u.LoginName, u.Email, u.Status, u.FirstName, u.LastName, u.Description, u.Pin, u.Language,
				u.TimeZone, u.NotificationEnabled, u.Mobile, u.Phone, blob, u.PasswordChange, u.SecurityCode);
		}

		public List<IUser> Query()
		{
			return All();
		}

		private string Url(Guid token, string displayName)
		{
			var ds = Query();
			var existing = new List<IUrlRecord>();

			foreach (var i in ds)
				existing.Add(new UrlRecord(i.Token.ToString(), i.DisplayName()));

			return UrlGenerator.GenerateUrl(token.ToString(), displayName, existing);
		}

		public void Delete(Guid token)
		{
			var u = Select(token);

			if (u == null)
				throw new SysException(SR.ErrUserNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Security.Users.Delete(u);

			Remove(token);
			CachingNotifications.UserChanged(token);
		}

		public IAuthenticationResult Authenticate(string user, string password)
		{
			var u = Resolve(user);

			if (u == null)
				return AuthenticationResult.Fail(AuthenticationResultReason.NotFound);

			if (u.Status == UserStatus.Inactive)
				return AuthenticationResult.Fail(AuthenticationResultReason.Inactive);

			if (u.Status == UserStatus.Locked)
				return AuthenticationResult.Fail(AuthenticationResultReason.Locked);

			if (u.IsLocal())
			{
				var pwd = Shell.GetService<IDatabaseService>().Proxy.Security.Users.SelectPassword(u);

				if (string.IsNullOrWhiteSpace(pwd))
					return AuthenticationResult.Fail(AuthenticationResultReason.NoPassword);

				if (u.PasswordChange != DateTime.MinValue && u.PasswordChange < DateTime.UtcNow)
					return AuthenticationResult.Fail(AuthenticationResultReason.PasswordExpired);

				if (!Shell.GetService<ICryptographyService>().VerifyHash(password, pwd))
					return AuthenticationResult.Fail(AuthenticationResultReason.InvalidPassword);
			}
			else
			{
				var ldap = AuthenticateLdap(u.LoginName, password);

				if (!ldap.Success)
					return ldap;
			}

			Shell.GetService<IDatabaseService>().Proxy.Security.Users.UpdateLoginInformation(u, u.AuthenticationToken, DateTime.UtcNow);
			Refresh(u.Token);
			CachingNotifications.UserChanged(u.Token);

			return AuthenticationResult.OK(new JwtSecurityTokenHandler().WriteToken(TomPITAuthenticationHandler.CreateToken(u.AuthenticationToken)));

		}

		private IAuthenticationResult AuthenticateLdap(string loginName, string password)
		{
			var tokens = loginName.Split('\\');

			if (tokens.Length != 2)
				return AuthenticationResult.Fail(AuthenticationResultReason.NotFound);

			string domain = tokens[0];
			string userName = tokens[1];

			try
			{
				using (var adContext = new PrincipalContext(ContextType.Domain, domain))
				{
					if (adContext.ValidateCredentials(userName, password))
						return AuthenticationResult.OK(null);
					else
						return AuthenticationResult.Fail(AuthenticationResultReason.InvalidCredentials);
				}
			}
			catch
			{
				return AuthenticationResult.Fail(AuthenticationResultReason.NotFound);
			}
		}


		public void ResetPassword(string user, string newPassword)
		{
			var u = Resolve(user);

			if (u == null)
				throw SysException.UserNotFound();

			Shell.GetService<IDatabaseService>().Proxy.Security.Users.UpdatePassword(u, Shell.GetService<ICryptographyService>().Hash(newPassword));

			Update(u.Token, u.LoginName, u.Email, u.Status, u.FirstName, u.LastName, u.Description, u.Pin, u.Language,
				 u.TimeZone, u.NotificationEnabled, u.Mobile, u.Phone, DateTime.UtcNow, u.SecurityCode);
		}

		public void UpdatePassword(string user, string existingPassword, string newPassword)
		{
			IUser u = Resolve(user);

			if (u == null)
				throw SysException.UserNotFound();

			if (string.IsNullOrWhiteSpace(existingPassword))
				existingPassword = Shell.GetService<IDatabaseService>().Proxy.Security.Users.SelectPassword(u);

			if (!string.IsNullOrWhiteSpace(existingPassword) && u.PasswordChange >= DateTime.UtcNow)
			{
				var ar = Authenticate(user, existingPassword);

				if (!ar.Success)
					throw new SysException(ar.GetDescription());
			}

			Shell.GetService<IDatabaseService>().Proxy.Security.Users.UpdatePassword(u, Shell.GetService<ICryptographyService>().Hash(newPassword));

			if (string.IsNullOrWhiteSpace(newPassword))
			{
				Update(u.Token, u.LoginName, u.Email, u.Status, u.FirstName, u.LastName, u.Description, u.Pin, u.Language,
					 u.TimeZone, u.NotificationEnabled, u.Mobile, u.Phone, DateTime.UtcNow, u.SecurityCode);
			}
			else
			{
				Update(u.Token, u.LoginName, u.Email, u.Status, u.FirstName, u.LastName, u.Description, u.Pin, u.Language,
					 u.TimeZone, u.NotificationEnabled, u.Mobile, u.Phone, DateTime.MinValue, u.SecurityCode);
			}
		}

		private string EncryptSecurityCode(string code)
		{
			if (string.IsNullOrWhiteSpace(code))
				return code;

			return Shell.GetService<ICryptographyService>().Encrypt(this, code);
		}

		private void DecryptSecurityCode(IUser user)
		{
			var property = user.GetType().GetProperty(nameof(user.SecurityCode));

			if (!property.CanWrite)
				return;

			var value = property.GetValue(user) as string;

			if (string.IsNullOrEmpty(value))
				return;

			property.SetValue(user, Shell.GetService<ICryptographyService>().Decrypt(this, value));
		}

		private void ValidateSecurityCode(Guid user, string code)
		{
			if (string.IsNullOrEmpty(code))
				return;

			var u = SelectBySecurityCode(code);

			if (u == null || u.Token == user)
				return;

			throw new BadRequestException(SR.ValSecurityCodeExists);
		}
	}
}