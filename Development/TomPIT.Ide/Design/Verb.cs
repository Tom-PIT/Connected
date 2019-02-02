namespace TomPIT.Design
{
	public class Verb : IVerb
	{
		public string Name { get; set; }
		public string Id { get; set; }
		public string Confirm { get; set; }
		public VerbAction Action { get; set; }
	}
}
