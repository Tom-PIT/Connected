using System;
using TomPIT.Data.Sql;
using TomPIT.Security;

namespace TomPIT.SysDb.Sql.Security
{
	internal class User : PrimaryKeyRecord, IUser
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Url { get; set; }
		public string Email { get; set; }
		public UserStatus Status { get; set; }
		public Guid Token { get; set; }
		public Guid AuthenticationToken { get; set; }
		public Guid Language { get; set; }
		public string Description { get; set; }
		public DateTime LastLogin { get; set; }
		public string TimeZone { get; set; }
		public bool NotificationEnabled { get; set; }
		public string LoginName { get; set; }
		public string Pin { get; set; }
		public string Phone { get; set; }
		public string Mobile { get; set; }
		public Guid Avatar { get; set; }
		public DateTime PasswordChange { get; set; }
		public bool HasPassword { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			FirstName = GetString("first_name");
			LastName = GetString("last_name");
			Url = GetString("url");
			Email = GetString("email");
			Status = GetValue("status", UserStatus.Inactive);
			Token = GetGuid("token");
			AuthenticationToken = GetGuid("auth_token");
			Language = GetGuid("language_token");
			Description = GetString("description");
			LastLogin = GetDate("last_login");
			TimeZone = GetString("timezone");
			NotificationEnabled = GetBool("notification_enabled");
			LoginName = GetString("login_name");
			Pin = GetString("pin");
			Phone = GetString("phone");
			Mobile = GetString("mobile");
			Avatar = GetGuid("avatar");
			PasswordChange = GetDate("password_change");
			HasPassword = !string.IsNullOrWhiteSpace(GetString("password"));
		}
	}
}
