namespace TomPIT.Reflection
{
	internal class ManifestAttribute : ManifestMember, IManifestAttribute
	{
		public bool IsValidation { get; set; }
		public string Description { get; set; }
	}
}
