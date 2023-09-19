namespace TomPIT.UI.Theming.Plugins
{
	using Parser.Infrastructure.Nodes;

	public interface IVisitor
    {
        Node Visit(Node node);
    }
}