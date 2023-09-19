namespace TomPIT.UI.Theming.Plugins
{
	using Parser.Tree;
	using TomPIT.UI.Theming.Parser.Infrastructure;

	public interface IVisitorPlugin : IPlugin
	{
		Root Apply(Root tree);

		VisitorPluginType AppliesTo { get; }

		void OnPreVisiting(Env env);
		void OnPostVisiting(Env env);
	}

	public enum VisitorPluginType
	{
		BeforeEvaluation,
		AfterEvaluation
	}
}