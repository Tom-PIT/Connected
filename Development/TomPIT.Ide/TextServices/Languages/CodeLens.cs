namespace TomPIT.Ide.TextServices.Languages
{
	internal class CodeLens : ICodeLens
	{
		public ICommand Command {get;set;}

		public string Id {get;set;}

		public IRange Range {get;set;}
	}
}
