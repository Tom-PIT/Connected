using System;
using TomPIT.Security;

namespace TomPIT
{
	public static class SecurityUtils
	{
		public const string AuthenticationCookieName = "TomPITAuth";

		public static Guid FullControlRole { get { return new Guid("{C82BBDAD-E913-4779-8771-981349467860}"); } }
		//public static Guid ExecuteRole { get { return new Guid("{CD690348-56EB-4168-9717-D8B8AC83D813}"); } }
		//public static Guid ApiRole { get { return new Guid("{DFF06D23-96F7-42EC-95D8-794CC99BFEBE}"); } }
		public static Guid AuthenticatedRole { get { return new Guid("{05726180-AFA9-450E-AE55-2A96C4C4BA77}"); } }
		public static Guid AnonymousRole { get { return new Guid("{9A092D91-653C-4020-A8BC-C1F14D6E71BA}"); } }
		public static Guid EveryoneRole { get { return new Guid("{8F0C27C9-C638-4AB1-B298-74EA319D866F}"); } }
		public static Guid DomainIdentityRole { get { return new Guid("{F2448180-B0B5-4A6C-AE99-51B8B94EDE09}"); } }
		public static Guid LocalIdentityRole { get { return new Guid("{F8528DFD-1AD0-4E76-882A-E2972DB7B0B8}"); } }
		public static Guid Development { get { return new Guid("{E4CC67A6-B42D-4632-8744-772B040BD6FF}"); } }
		public static Guid Management { get { return new Guid("{80813853-FF79-4FFD-8439-3FF52F77E2D7}"); } }

		//public static Guid CreateMicroServiceRole { get { return new Guid("{1AEE8C2D-093A-4141-A46A-164C082BECFD}"); } }
		//public static Guid ImplementMicroServiceRole { get { return new Guid("{5317DABB-7AF1-46C4-9DA8-501F6F7AEA53}"); } }

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
							return token.AsString();
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

		public static bool IsLocal(this IUser user)
		{
			return !user.LoginName.Contains('\\');
		}
	}
}
