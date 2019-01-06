namespace TomPIT.Runtime
{
	public interface IContextIdentity
	{
		string Authority { get; }
		string AuthorityId { get; }
		string ContextId { get; }
	}
}
