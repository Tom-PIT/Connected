namespace TomPIT.Ide.TextServices.Languages
{
	internal class DeltaDecoration : IDeltaDecoration
	{
		public IDeltaDecorationOptions Options { get; set; }

		public IRange Range { get; set; }
	}
}
