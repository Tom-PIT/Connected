namespace TomPIT.Runtime
{
	public interface IIdentityBinder
	{
		void Bind(string authorityId, string authority, string contextId);
	}
}
