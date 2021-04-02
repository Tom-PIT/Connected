namespace TomPIT.Reflection
{
	public class ManifestAttribute : ManifestMember, IManifestAttribute
	{
		public bool IsValidation { get; set; }
		public string Description { get; set; }
	}
}
