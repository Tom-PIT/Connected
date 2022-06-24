namespace TomPIT.UI.Theming.Parser.Infrastructure
{
	using Nodes;
	using TomPIT.UI.Theming.Parser.Tree;

	public interface IOperable
    {
        Node Operate(Operation op, Node other);
        Color ToColor();
    }
}