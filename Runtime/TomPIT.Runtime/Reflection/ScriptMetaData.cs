namespace TomPIT.Reflection
{
	internal class ScriptMetaData
	{
		private ScriptDocumentation _doc;
		public ScriptDocumentation Documentation => _doc ??= new ScriptDocumentation();
	}
}
