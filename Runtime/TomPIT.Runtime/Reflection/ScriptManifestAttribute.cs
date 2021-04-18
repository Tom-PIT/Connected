namespace TomPIT.Reflection
{
	internal class ScriptManifestAttribute : ScriptManifestMember, IScriptManifestAttribute
	{
		public bool IsValidation {get;set;}

		public string Description {get;set;}
	}
}
