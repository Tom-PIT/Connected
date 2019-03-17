using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class StringTableProvider : ComponentAnalysisProvider
	{
		public StringTableProvider(IExecutionContext context) : base(context)
		{

		}

		protected override string ComponentCategory => "StringTable";
	}
}
