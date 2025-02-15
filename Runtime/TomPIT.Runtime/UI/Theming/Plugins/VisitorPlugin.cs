﻿namespace TomPIT.UI.Theming.Plugins
{
	using Parser.Infrastructure.Nodes;
	using Parser.Tree;
	using TomPIT.UI.Theming.Parser.Infrastructure;

	public abstract class VisitorPlugin : IVisitorPlugin, IVisitor
    {
        public Root Apply(Root tree)
        {
            Visit(tree);

            return tree;
        }

        public abstract VisitorPluginType AppliesTo { get; }

        public Node Visit(Node node)
        {
            bool visitDeeper;
            node = Execute(node, out visitDeeper);
            if (visitDeeper && node != null)
                node.Accept(this);

            return node;
        }

        public abstract Node Execute(Node node, out bool visitDeeper);

        public virtual void OnPreVisiting(Env env)
        {
        }

        public virtual void OnPostVisiting(Env env)
        {
        }
    }
}