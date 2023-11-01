using System;

namespace TomPIT.Security
{
	public enum UserStatus
	{
		Inactive = 0,
		Active = 1,
		Locked = 2
	}
	public interface IUser
	{
		int Id { get; }
		string FirstName { get; }
		string LastName { get; }
		string Url { get; }
		string Email { get; }
		UserStatus Status { get; }
		Guid Token { get; }
		Guid AuthenticationToken { get; }
		Guid Language { get; }
		string Description { get; }
		DateTime LastLogin { get; }
		string TimeZone { get; }
		bool NotificationEnabled { get; }
		string LoginName { get; }
		string Pin { get; }
		string Phone { get; }
		string Mobile { get; }
		Guid Avatar { get; }
		DateTime PasswordChange { get; }
		bool HasPassword { get; }
		string SecurityCode { get; }
	}
}
