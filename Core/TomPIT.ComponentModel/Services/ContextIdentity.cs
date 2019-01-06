namespace TomPIT.Services
{
	internal class ContextIdentity : IContextIdentity
	{
		public string Authority { get; set; }
		public string AuthorityId { get; set; }
		public string ContextId { get; set; }
	}
}
