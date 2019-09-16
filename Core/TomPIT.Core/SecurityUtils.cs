using Microsoft.AspNetCore.Http;
using System;
using TomPIT.Connectivity;
using TomPIT.Security;

namespace TomPIT
{
	public static class SecurityUtils
	{
		public const string AuthenticationCookieName = "TomPITAuth";

		public static Guid FullControlRole { get { return new Guid("{C82BBDAD-E913-4779-8771-981349467860}"); } }
		public static Guid AuthenticatedRole { get { return new Guid("{05726180-AFA9-450E-AE55-2A96C4C4BA77}"); } }
		public static Guid AnonymousRole { get { return new Guid("{9A092D91-653C-4020-A8BC-C1F14D6E71BA}"); } }
		public static Guid EveryoneRole { get { return new Guid("{8F0C27C9-C638-4AB1-B298-74EA319D866F}"); } }
		public static Guid DomainIdentityRole { get { return new Guid("{F2448180-B0B5-4A6C-AE99-51B8B94EDE09}"); } }
		public static Guid LocalIdentityRole { get { return new Guid("{F8528DFD-1AD0-4E76-882A-E2972DB7B0B8}"); } }
		public static Guid Development { get { return new Guid("{E4CC67A6-B42D-4632-8744-772B040BD6FF}"); } }
		public static Guid Management { get { return new Guid("{80813853-FF79-4FFD-8439-3FF52F77E2D7}"); } }

		public static string DisplayName(string firstName, string lastName, string loginName, string email, Guid token)
		{
			string fn = FullName(firstName, lastName);

			if (string.IsNullOrWhiteSpace(fn))
			{
				if (!string.IsNullOrWhiteSpace(loginName))
					return loginName;
				else
				{
					int idx = email.IndexOf('@'); 

					if (idx == -1)
					{
						if (string.IsNullOrWhiteSpace(email))
							return token.ToString();
						else
							return email;
					}

					return email.Substring(0, idx);
				}
			}
			else
				return fn;
		}

		public static string FullName(string firstName, string lastName)
		{
			return string.Format("{0} {1}", firstName, lastName).Trim();
		}

		public static string DisplayName(this IUser user)
		{
			return DisplayName(user.FirstName, user.LastName, user.LoginName, user.Email, user.Token);
		}

		public static string DomainLoginName(this IUser user)
		{
			if (user.IsLocal())
				return user.LoginName;

			return user.LoginName.Split(new char[] { '\\' }, 2)[1];
		}

		public static bool IsLocal(this IUser user)
		{
			return !user.LoginName.Contains('\\');
		}

		public static string GetDescription(this IAuthenticationResult result)
		{
			switch (result.Reason)
			{
				case AuthenticationResultReason.NotFound:
					return SR.ErrUserNotFound;
				case AuthenticationResultReason.InvalidPassword:
					return SR.ErrAuthenticationFailed;
				case AuthenticationResultReason.Inactive:
					return SR.ErrUserInactive;
				case AuthenticationResultReason.Locked:
					return SR.ErrUserLocked;
				case AuthenticationResultReason.NoPassword:
					return SR.ErrNoPassword;
				case AuthenticationResultReason.PasswordExpired:
					return SR.ErrPasswordExpired;
				case AuthenticationResultReason.InvalidToken:
					return SR.ErrInvalidToken;
				case AuthenticationResultReason.InvalidCredentials:
					return SR.ErrInvalidCredentials;
				default:
					return null;
			}
		}

		public static bool IsValid(this IAuthenticationToken token, HttpRequest request, IUser user, AuthenticationTokenClaim claim)
		{
			if (user == null || user.Status != UserStatus.Active)
				return false;

			if (token.Status == AuthenticationTokenStatus.Disabled)
				return false;

			if (token.ValidFrom != DateTime.MinValue && DateTime.UtcNow < token.ValidFrom)
				return false;

			if (token.ValidTo != DateTime.MinValue && token.ValidTo < DateTime.UtcNow)
				return false;

			if (token.StartTime != TimeSpan.Zero && DateTime.UtcNow.TimeOfDay < token.StartTime)
				return false;

			if (token.EndTime != TimeSpan.Zero && token.EndTime < DateTime.UtcNow.TimeOfDay)
				return false;

			if (token.Claims != AuthenticationTokenClaim.All && ((token.Claims & claim) != claim))
				return false;

			if (!string.IsNullOrWhiteSpace(token.IpRestrictions))
			{
				if (!IPAddressRange.Check(request.HttpContext.Connection.RemoteIpAddress, token.IpRestrictions))
					return false;
			}

			return true;
		}
	}
}
