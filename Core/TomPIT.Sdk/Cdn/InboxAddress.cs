namespace TomPIT.Cdn
{
	public class InboxAddress : IInboxAddress
	{
		public string Name { get; set; }

		public string Address { get; set; }

		public bool IsInternational { get; set; }
	}
}
