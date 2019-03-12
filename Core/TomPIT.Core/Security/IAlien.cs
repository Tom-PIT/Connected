using System;

namespace TomPIT.Security
{
	public interface IAlien
	{
		string FirstName { get; }
		string LastName { get; }
		string Email { get; }
		string Mobile { get; }
		string Phone { get; }
		Guid Token { get; }
		Guid Language { get; }
		string Timezone { get; }
	}
}
