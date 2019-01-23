namespace TomPIT.Marketplace
{
	public interface IPublisher
	{
		string Company { get; }
		string FirstName { get; }
		string LastName { get; }
		string Country { get; }
		string Email { get; }
		string Phone { get; }
		string Website { get; }
		string Key { get; }
	}
}
